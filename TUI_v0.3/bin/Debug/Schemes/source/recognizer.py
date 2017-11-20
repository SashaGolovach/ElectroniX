from keras.models import load_model
from keras import backend as K
from PIL import Image, ImageOps
from socket import socket
import pickle
import os
import numpy as np

K.set_image_dim_ordering('th')
clf = load_model('my_model.h5')
print("Model loaded")
server = socket()
server.bind(('localhost', 8888))
print("Server set up")
server.listen(1)
conn, _ = server.accept()
print("Connection established")

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

def recognize():
    result = []
    for file in sorted(os.listdir("segments")):
        predictions = []
        segment = ImageOps.invert(Image.open("segments/"+file).convert('L'))
        if segment.getbbox() is None:
            result.append((-1, -1))
            continue
        for i in range(4):
            sample = preprocess(segment).reshape((1, 1, 28, 28))/255
            predictions.append(clf.predict(sample))
            segment = segment.rotate(90)
        ans = divmod(int(np.argmax(predictions)), 7)
        result.append(ans)
    return result

try:
    while True:
        conn.recv(1024)
        result = recognize()
        conn.send(pickle.dumps(result, protocol=2))
        print("Sent:", result)
except Exception as e:
    print("Connection lost:", e)