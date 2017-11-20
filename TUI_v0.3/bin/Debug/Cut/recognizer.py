from keras.models import load_model
from keras import backend as K
from PIL import Image, ImageOps
from socket import socket
from time import time
import numpy as np
import matplotlib.pyplot as plt

K.set_image_dim_ordering('th')
clf = load_model('best_model_ever.h5')
print("Model loaded")
server = socket()
server.bind(('localhost', 8080))
print("Server set up")
server.listen(1)
conn, _ = server.accept()
print("Connection established")

def recognize():
    im = Image.open('output0.jpg').convert('L')
    w, h = im.size
##    im = im.crop((2, 2, w-2, h-2))
##    h -= 4
##    w -= 4
    pix = im.load()
    proj = [0] * w
    threshlen = 3
    threshcount = 0.05*h
    sep = []
    flag = True
    proj = [len([j for j in range(h)
                 if pix[i, j] < 127])
            for i in range(w)]
    if proj[0] > threshcount:
        sep.append(0)
    for i in range(w):
        if all(proj[min(i+j, w-1)] < threshcount
               for j in range(threshlen)) and flag:
            sep.append(i+threshlen//2)
            flag = False
        if proj[i] > threshcount:
            flag = True
    if proj[w-1] > threshcount:
        sep.append(w)
    answer = ''
    for i in range(len(sep)-1):
        segment = ImageOps.invert(im.crop((sep[i], 0, sep[i+1], h)))
        #segment.show()
        sample = preprocess(segment).reshape((1, 1, 28, 28))/255
        prediction = np.argmax(clf.predict(sample))
        answer += str(prediction)
    return answer if answer else '-1'

def preprocess(im):
    bbox = list(im.getbbox())
    w, h = bbox[2]-bbox[0], bbox[3]-bbox[1]
    if w > h:
        bbox[1] -= (w-h)//2
        bbox[3] += (w-h)//2
    else:
        bbox[0] -= (h-w)//2
        bbox[2] += (h-w)//2
    im = im.crop(bbox).resize((400, 400))
    pix = np.asarray(im)
    pix = [[pix[i*20: i*20+20, j*20: j*20+20]
            for j in range(20)] for i in range(20)]
    gs = np.zeros((20, 20))
    mass = np.zeros(2)
    for i in range(20):
        for j in range(20):
            gs[i][j] = min(255, np.sum(pix[i][j])/350)
            mass += np.array([i, j])*gs[i][j]
    mass = (mass/np.sum(gs)).astype('uint8')
    sample = np.zeros((28, 28), dtype='float32')
    for i in range(20):
        for j in range(20):
            k = max(0, min(i + 14 - mass[0], 27))
            l = max(0, min(j + 14 - mass[1], 27))
            sample[k][l] = gs[i][j]
    return sample

x = []
y = []
flag = True

try:
    while True:
        sig = conn.recv(1024).decode()
        print("Signal:", sig)
        if ',' in sig:
            plt.plot(x, y)
            print("Showing")
            raise RuntimeError
        else:
            if flag:
                flag = False
                t = time()
            number = recognize()
            print("Got", number)
            if int(number) != -1:
                x.append(time() - t)
                y.append(int(number))
            conn.send(number.encode())
except Exception as e:
    print("Connection lost:", e)
    plt.show()

#print(recognize())
