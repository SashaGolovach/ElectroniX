from socket import socket
import pickle

scl = 200
dots = []
PS = 5
client = socket()
images = []
names = ['source', '-', 'L', 'lamp', 'resistor', 'T', '+']

def setup():
    size(1200, 600)
    background(255)
    fill(255)
    grid()
    client.connect(('localhost', 8888))
    for name in names:
        images.append(loadImage('images/'+name+'.jpg'))
    print(images)

def draw():
    pass

def grid():
    stroke(200)
    strokeWeight(PS)
    for i in range(scl, width, scl):
        line(i, 0, i, height)
    for i in range(scl, height, scl):
        line(0, i, width, i)
    strokeWeight(1)
    stroke(0)

def mouseDragged(event):
    x, y = mouseX, mouseY
    if dots:
        xprev, yprev = dots.pop()
        line(xprev, yprev, x, y)
    point(x, y)
    dots.append((x, y))

def mouseReleased():
    dots[:] = []

def keyPressed():
    if key == ' ':
        #background(255)
        x, y = mouseX/scl*scl, mouseY/scl*scl
        noStroke()
        rect(x+PS, y+PS, scl-2*PS, scl-2*PS)
        grid()
    elif key == ENTER:
        recognize()
    elif key == 's':
        saveScheme()

def recognize():
    global result
    img = copy()
    elt = createImage(scl-2*PS, scl-2*PS, RGB)
    for i in range(width//scl):
        for j in range(height//scl):
            elt.copy(img, i*scl+PS, j*scl+PS, scl-2*PS,
                     scl-2*PS, 0, 0, scl-2*PS, scl-2*PS)
            elt.save('segments/'+str(i)+str(j)+'.jpg')
    client.send('.'.encode())
    result = pickle.loads(client.recv(4096))
    background(255)
    c = -1
    for i in range(scl//2, width, scl):
        for j in range(scl//2, height, scl):
            c += 1
            if result[c][0] == -1: 
                continue
            pushMatrix()
            translate(i, j)
            point(i, j)
            rotate(HALF_PI*result[c][0])
            image(images[result[c][1]], -scl//2+PS, 
                  -scl//2+PS, scl-2*PS, scl-2*PS)
            popMatrix()
    grid()
    
def saveScheme():
    with open("scheme.pkl", 'wb') as f:
        pickle.dump(result, f)
    save("scheme.jpg")