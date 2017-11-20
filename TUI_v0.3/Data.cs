using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace TUI_v0._3
{
    public class Data
    {
        public string Name { get; set; }
        public string Component { get; set; }
        public string Resistance { get; set; }
        public string Voltage { get; set; }
        public string I { get; set; }
        public Data(string name, string type, string r, string u, string i, SolidColorBrush br)

        {
            this.Name = name;
            this.Component = type;
            this.Resistance = r;
            this.Voltage = u;
            this.I = i;
        }
    }
}
