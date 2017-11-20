import processing.net.*;
import processing.video.*;
float EPS = 0.1, INF = 1000000007;
PImage img, window, etalon;
PVector tl, br, pos;
boolean choosing = false, started = false, finished = false;
int w, h, start_time;
int rate = 7, delta = 1; //filter size
int[][] moves;
float[] GaussMask;
Client server;
Capture cam;
Button start, finish;

void setup() {
  size(640, 480);
  tl = new PVector(0, 0);
  br = new PVector(0, 0);
  //create masks
  moves = new int[rate*rate][2];
  GaussMask = new float[rate*rate];
  for (int i = 0; i < rate; i++)
    for (int j = 0; j < rate; j++) {
      moves[i*rate+j][0] = i - rate/2;
      moves[i*rate+j][1] = j - rate/2;
      GaussMask[i*rate+j] = 10*exp(-((pow(i-rate/2, 2)/4 + pow(j-rate/2, 2)/4)));
    }

  //load image
  //img = loadImage("tests/test1.jpg");

  //connect to a local server
  server = new Client(this, "127.0.0.1", 8080);

  //connect camera
  String[] cameras = Capture.list();
  for (String s : cameras)
    println(s);
  if (cameras.length == 0) {
    println("No cameras available");
    noLoop();
  }
  //int[] WHFPS = int(cameras[0]);
  cam = new Capture(this, 640, 480);//, "DroidCam Source 3", 0);
  cam.start();
  w = cam.width;
  h = cam.height;
  img = createImage(w, h, RGB);

  //create buttons
  start = new Button(5, 5, 50, "Start");
  finish = new Button(60, 5, 50, "Stop");
  noFill();
}

void draw() {
  if (!finished) {
    background(0);
    if (cam.available()) {
      cam.read();
    }
    cam.loadPixels();
    img.loadPixels();
    for (int i = 0; i < w*h; i++)
      img.pixels[i] = cam.pixels[i];
    img.updatePixels();
    image(img, 0, 0);
    
    //buttons process
    start.show();
    finish.show();
    if (!mousePressed) {
      start.shadow();
      finish.shadow();
    }

    if (choosing) {
      //show the forming rectangle
      noFill();
      stroke(255, 0, 0);
      rect(tl.x, tl.y, mouseX-tl.x, mouseY-tl.y);
    } else if (br.mag() > 0) {
      //show the reading box
      stroke(255, 0, 0);
      noFill();
      rect(pos.x, pos.y, window.width, window.height);
      if (started) {
        //cut out the selected part/////
        window = cutOut(img, window, pos);

        //filter it/////////////////////
        overallFilter(window);

        //read the recognized result from the server
        image(window, 100, 0);
        println(recognize(window));

        //update the box position
        pos = find(img, etalon, pos);
        //delay(1000);
      }
    }
  } 
}

void mousePressed() {
  if (!start.action())
    if (!finish.action())
      if (!started) {
        PVector mouse = new PVector(mouseX, mouseY);
        if (choosing) {
          br = mouse;     
          pos = decide(tl, br);
          window = createImage(int(abs(tl.x-br.x)), int(abs(tl.y - br.y)), RGB);
          etalon = cutOut(img, window, pos);
        } else
          tl = mouse;
        choosing = !choosing;
      }
}