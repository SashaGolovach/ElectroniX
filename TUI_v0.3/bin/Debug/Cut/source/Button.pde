class Button {
  PVector pos;
  int sz;
  String name;
  color bg;

  Button(int x, int y, int _sz, String _name) {
    pos = new PVector(x, y);
    sz = _sz;
    name = _name;
    bg = color(255);
  }

  void show() {
    noStroke();
    fill(bg);
    rect(pos.x, pos.y, sz, sz);
    fill(0);
    text(name, pos.x+sz/4, pos.y+sz/2);
  }

  void shadow() {
    if (mouseX > pos.x && mouseX < pos.x + sz && 
      mouseY > pos.y && mouseY < pos.y + sz)
      bg = color(100);
    else
      bg = color(255);
  }

  boolean action() {
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