PVector decide(PVector a, PVector b) {
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

PImage cutOut(PImage img, PImage box, PVector pos) {
  int w = box.width;
  int h = box.height;
  PImage cut = createImage(w, h, RGB);
  cut.copy(img, int(pos.x), int(pos.y), w, h, 0, 0, w, h);
  return cut;
}

int recognize(PImage im) {
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
  return int(data);
}

float same(PImage main, PImage frag, int x0, int y0, float dt) {
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

PVector find(PImage main, PImage frag, PVector prev) {
  int mW = main.width, mH = main.height;
  int fW = frag.width, fH = frag.height;
  PVector best = new PVector(0, 0);
  float record = 0.0;
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