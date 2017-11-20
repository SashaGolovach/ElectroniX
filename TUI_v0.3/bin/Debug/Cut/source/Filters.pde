void overallFilter(PImage im) {
  //GaussianFilter(im);
  //sharpen(im);
  //medFilter(im);
  //grayFilter(im);
  //blur(im);
  //balance(im);
  //SobelOperator(im);
  binarize(im, 75);
}

void grayFilter(PImage im) {
  im.loadPixels();  
  for (int i = 0; i < im.width*im.height; i += delta)
    im.pixels[i] = color(2*brightness(im.pixels[i]));
  im.updatePixels();
}

void GaussianFilter(PImage im) {
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

void medFilter(PImage im) {
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

void blur(PImage im) {
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

void sharpen(PImage im) {
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

void SobelOperator(PImage im) {
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

void balance(PImage im) {
  float avg = 0;
  int w = im.width, h = im.height;
  for (int i = 0; i < w*h; i += delta)
    avg += brightness(im.pixels[i]);
  avg /= w*h;
  for (int i = 0; i < w*h; i += delta)
    im.pixels[i] = color(abs(brightness(im.pixels[i])-avg)/2);
  im.updatePixels();
}

void binarize(PImage im, float entry) {
  im.loadPixels();
  float avg = 0;
  for (int i = 0; i < im.width*im.height; i += delta)
    avg += brightness(im.pixels[i]);
  entry = avg/im.width/im.height;
  for (int i = 0; i < im.width*im.height; i += delta)
    im.pixels[i] = color((int(brightness(im.pixels[i]) > entry))*255);
  im.updatePixels();
}