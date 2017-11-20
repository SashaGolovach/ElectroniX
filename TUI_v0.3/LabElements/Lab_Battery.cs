using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace TUI_v0._3
{
    class Lab_Battery : LabElement
    {
        public double resistance = 1.0;
        public double U = 5.0;
        BitmapImage background = new BitmapImage(new Uri("Icons\\eds.png", UriKind.Relative));

        private void Resistor_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            checkMe(name);
            showmenu(name);
        }
        public Lab_Battery(string name)
        {
            br.ImageSource = background;
            rec.Width = 100;
            rec.Height = 100;
            rec.Fill = br;
            this.name = name;
            this.MouseDown += Resistor_MouseDown;
            _l.Margin = new System.Windows.Thickness(29, 10,0,0);
            _r.Margin = new System.Windows.Thickness(52, 10, 0, 0);
            _l_point.Margin = new System.Windows.Thickness(33,21, 0, 0);
            _r_point.Margin = new System.Windows.Thickness(56, 21, 0, 0);
        }
    }
}
