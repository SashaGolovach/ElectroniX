using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TUI_v0._3.LabElements
{
    [Serializable]
    public class LabSerializible
    {
        public string Name;
        public bool type = false;
        public List<Tuple<Point, Point>> ancestors = new List<Tuple<Point, Point>>();
        public List<LabElementSerializible> elements = new List<LabElementSerializible>();
        public List<Tuple<int, int, string, string, Ischecked, int>> lines = new List<Tuple<int, int, string, string, Ischecked, int>>();
        public int countMax;
        public bool canAdd;
        public string header;
        public string pathImage;
        public string content;
        public string goal;
        public List<string> selectedTypes;
        public string TL;
        public long hours, sec, min;
    }

    [Serializable]
    public class LabElementSerializible
    {
        int angle;
        public double resistance;
        public double E = 1.0;
        public double resistanceMax;
        public double EMax;
        public List<Tuple<int, int, string, string, int>> connections;
        public int ID; // id for scheme
        public ElementsTypes type;
        public string name;
        public double l, r, top, bottom;
    }
}
