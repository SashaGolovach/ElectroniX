import processing.core.*; 
import processing.data.*; 
import processing.event.*; 
import processing.opengl.*; 

import processing.net.*; 
import processing.video.*; 

import java.util.HashMap; 
import java.util.ArrayList; 
import java.io.File; 
import java.io.BufferedReader; 
import java.io.PrintWriter; 
import java.io.InputStream; 
import java.io.OutputStream; 
import java.io.IOException; 

public class Cut extends PApplet {



float EPS = 0.1f, INF = 1000000007;
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

public void setup() {
  
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

public void draw() {
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

public void mousePressed() {
  if (!start.action())
    if (!finish.action())
      if (!started) {
        PVector mouse = new PVector(mouseX, mouseY);
        if (choosing) {
          br = mouse;     
          pos = decide(tl, br);
          window = createImage(PApplet.parseInt(abs(tl.x-br.x)), PApplet.parseInt(abs(tl.y - br.y)), RGB);
          etalon = cutOut(img, window, pos);
        } else
          tl = mouse;
        choosing = !choosing;
      }
}
class Button {
  PVector pos;
  int sz;
  String name;
  int bg;

  Button(int x, int y, int _sz, String _name) {
    pos = new PVector(x, y);
    sz = _sz;
    name = _name;
    bg = color(255);
  }

  public void show() {
    noStroke();
    fill(bg);
    rect(pos.x, pos.y, sz, sz);
    fill(0);
    text(name, pos.x+sz/4, pos.y+sz/2);
  }

  public void shadow() {
    if (mouseX > pos.x && mouseX < pos.x + sz && 
      mouseY > pos.y && mouseY < pos.y + sz)
      bg = color(100);
    else
      bg = color(255);
  }

  public boolean action() {
    noFill();
    if (mouseX > pos.x && mouseX < pos.x + sz && 
      mouseY > pos.y && mouseY < pos.y + sz) {
      if (name == "Start")
        if (br.mag() > 0) {
          started = true;
          start_time = millis();
        }
      if (name == "Stop")
        if (started) {
          finished = true;
          server.write(",");
          background(0);
          exit();
        }
      return true;
    }
    return false;
  }
}
public void overallFilter(PImage im) {
  //GaussianFilter(im);
  //sharpen(im);
  //medFilter(im);
  //grayFilter(im);
  //blur(im);
  //balance(im);
  //SobelOperator(im);
  binarize(im, 75);
}

public void grayFilter(PImage im) {
  im.loadPixels();  
  for (int i = 0; i < im.width*im.height; i += delta)
    im.pixels[i] = color(2*brightness(im.pixels[i]));
  im.updatePixels();
}

public void GaussianFilter(PImage im) {
  PImage res = im.copy();
  im.loadPixels();
  int w = im.width, h = im.height;
  for (int x = 0; x < w; x += delta)
    for (int y = 0; y < h; y += delta) {
      int loc = x + y * w;
      float sum = 0, count = 0;
      for (int i = 0; i < rate*rate; i++) {
        int pos = (x+moves[i][0]) + (y+moves[i][1])*w;
        if (pos > 0 && pos < w*h) {
          sum += brightness(im.pixels[pos])*GaussMask[i];
          count += GaussMask[i];
        }
      }
      res.pixels[loc] = color(sum/count);
    }
  for (int i = 0; i < w*h; i++)
    im.pixels[i] = res.pixels[i];
  im.updatePixels();
}

public void medFilter(PImage im) {
  PImage res = im.copy();
  im.loadPixels();
  int w = im.width, h = im.height;
  for (int x = 0; x < w; x += delta)
    for (int y = 0; y < h; y += delta) {
      int loc = x + y * w;
      FloatList med = new FloatList();
      for (int i = 0; i < rate*rate; i++) {
        int pos = (x+moves[i][0]) + (y+moves[i][1])*w;
        if (pos > 0 && pos < w*h)
          med.append(brightness(im.pixels[pos]));
      }
      med.sort();
      res.pixels[loc] = color(med.get(med.size()/2));
    }
  for (int i = 0; i < w*h; i++)
    im.pixels[i] = res.pixels[i];
  im.updatePixels();
}

public void blur(PImage im) {
  PImage res = im.copy();
  im.loadPixels();
  int w = im.width, h = im.height;
  for (int x = 0; x < w; x += delta)
    for (int y = 0; y < h; y += delta) {
      int loc = x + y * w;
      float sum = 0;
      int count = 0;
      for (int i = 0; i < rate*rate; i++) {
        int pos = (x+moves[i][0]) + (y+moves[i][1])*w;
        if (pos > 0 && pos < w*h) {
          sum += brightness(im.pixels[pos]);
          count++;
        }
        res.pixels[loc] = color(sum/count);
      }
    }
  res.updatePixels();
  for (int i = 0; i < w*h; i++)
    im.pixels[i] = res.pixels[i];
  im.updatePixels();
}

public void sharpen(PImage im) {
  //float[][] shmask = {{0, -0.25, 0}, {-0.25, 2, -0.25}, {0, -0.25, 0}};
  float[][] shmask = {{0, -1, 0}, {-1, 5, -1}, {0, -1, 0}};
  //float[][] shmask = {{1, -2, 1}, {-2, 5, -2}, {1, -2, 1}};

  PImage res = im.copy();
  im.loadPixels();
  int w = im.width, h = im.height;
  for (int x = 0; x < w; x += delta)
    for (int y = 0; y < h; y += delta) {
      int loc = x + y * w;
      float sum = 0;
      for (int i = -1; i < 2; i++) 
        for (int j = -1; j < 2; j++) {
          int pos = (x+i) + (y+j)*w;
          if (pos > 0 && pos < w*h)
            sum += brightness(im.pixels[pos])*shmask[i+1][j+1];
        }
      res.pixels[loc] = color(sum);
    }
  res.updatePixels();
  for (int i = 0; i < w*h; i++)
    im.pixels[i] = res.pixels[i];
  im.updatePixels();
}

public void SobelOperator(PImage im) {
  float[][] xmask = {{-1, -2, -1}, {0, 0, 0}, {1, 2, 1}};
  float[][] ymask = {{-1, 0, 1}, {-2, 0, 2}, {-1, 0, 1}};

  PImage res = im.copy();
  im.loadPixels();
  int w = im.width, h = im.height;
  for (int x = 0; x < w; x += delta)
    for (int y = 0; y < h; y += delta) {
      int loc = x + y * w;
      float sumx = 0, sumy = 0;
      for (int i = -1; i < 2; i++) 
        for (int j = -1; j < 2; j++) {
          int pos = (x+i) + (y+j)*w;
          if (pos > 0 && pos < w*h) {
            sumx += brightness(im.pixels[pos])*xmask[i+1][j+1];
            sumy += brightness(im.pixels[pos])*ymask[i+1][j+1];
          }
        }
      sumx /= 7;
      sumy /= 7;
      res.pixels[loc] = color(sqrt(sumx*sumx + sumy*sumy));
    }
  res.updatePixels();
  for (int i = 0; i < w*h; i++)
    im.pixels[i] = res.pixels[i];
  im.updatePixels();
}

public void balance(PImage im) {
  float avg = 0;
  int w = im.width, h = im.height;
  for (int i = 0; i < w*h; i += delta)
    avg += brightness(im.pixels[i]);
  avg /= w*h;
  for (int i = 0; i < w*h; i += delta)
    im.pixels[i] = color(abs(brightness(im.pixels[i])-avg)/2);
  im.updatePixels();
}

public void binarize(PImage im, float entry) {
  im.loadPixels();
  float avg = 0;
  for (int i = 0; i < im.width*im.height; i += delta)
    avg += brightness(im.pixels[i]);
  entry = avg/im.width/im.height;
  for (int i = 0; i < im.width*im.height; i += delta)
    im.pixels[i] = color((PApplet.parseInt(brightness(im.pixels[i]) > entry))*255);
  im.updatePixels();
}
public PVector decide(PVector a, PVector b) {
  //<decides wich corner is topLeft>//
  if (a.x < b.x)
    if (a.y < b.y)
      return a.copy();
    else
      return new PVector(a.x, b.y);
  else
    if (a.y < b.y)
      return new PVector(b.x, a.y);
    else
      return b.copy();
}

public PImage cutOut(PImage img, PImage box, PVector pos) {
  int w = box.width;
  int h = box.height;
  PImage cut = createImage(w, h, RGB);
  cut.copy(img, PApplet.parseInt(pos.x), PApplet.parseInt(pos.y), w, h, 0, 0, w, h);
  return cut;
}

public int recognize(PImage im) {
  //<recognizes and updates gr>//

  //add to output folder as an image////
  im.save("output" + 0 + ".jpg");
  delay(500);
  server.write(".");
  String data;
  int time = millis();
  println("Waiting...");
  while (true) {
    delay(100);
    if (server.available() > 0)
      break;
  }
  data = server.readString();
  println("Done: " + data);
  return PApplet.parseInt(data);
}

public float same(PImage main, PImage frag, int x0, int y0, float dt) {
  float counter = 0;
  for (int x = 0; x < frag.width; x += dt)
    for (int y = 0; y < frag.height; y += dt) {
      int Floc = x + frag.width*y;
      int Mloc = x + x0 + (y + y0)*main.width;
      if (Mloc > 0 && Mloc < main.width*main.height && Floc > 0 && Floc < frag.width*frag.height) 
        counter += abs(brightness(main.pixels[Mloc]) - brightness(frag.pixels[Floc]));
    }
  return 1/counter;
}

public PVector find(PImage main, PImage frag, PVector prev) {
  int mW = main.width, mH = main.height;
  int fW = frag.width, fH = frag.height;
  PVector best = new PVector(0, 0);
  float record = 0.0f;
  int move = 100, dt = 10;
  //for (int x = int(prev.x)-move; x < prev.x+move; x += dt)
  //  for (int y = int(prev.y)-move; y < prev.y+move; y += dt) 
  for (int x = 0; x < mW-fW; x += dt)
    for (int y = 0; y < mH-fH; y += dt) 
      if (x+y*mW > 0 && x+y*mW < mW*mH) {
        float sameness = same(main, frag, x, y, dt);
        if (record < sameness) {
          best.set(x, y);
          record = sameness;
        }
      } 
  stroke(255, 0, 0);
  rect(best.x, best.y, fW, fH);
  stroke(0);
  return best;
}
  public void settings() {  size(640, 480); }
  static public void main(String[] passedArgs) {
    String[] appletArgs = new String[] { "Cut" };
    if (passedArgs != null) {
      PApplet.main(concat(appletArgs, passedArgs));
    } else {
      PApplet.main(appletArgs);
    }
  }
}
