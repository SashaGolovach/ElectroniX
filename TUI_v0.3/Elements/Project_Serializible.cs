using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows;

namespace TUI_v0._3
{
    [Serializable]
    public class Project_Serializible
    {
        public string path;
        public bool type = true;
        public List<Tuple<Point, Point>> ancestors = new List<Tuple<Point, Point>>();
        public List<List<Tuple<int, string>>> wires = new List<List<Tuple<int, string>>>(); // list of id and pos of each element
        public List<ElementSerializible> elements = new List<ElementSerializible>();
    }
}
