using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TUI_v0._3
{
    [Serializable]
    public class ElementSerializible
    {
        public bool isWire = false;
        public bool horizontal;
        public double resistance;
        public double E = 1.0;
        public List<Tuple<int, int>> connections;
        public int ID; // id for scheme
        public ElementsTypes type;
        public string name;
        public double l, r, top, bottom;
    }
}
