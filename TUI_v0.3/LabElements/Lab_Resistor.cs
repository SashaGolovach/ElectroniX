using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TUI_v0._3
{
    class Lab_Resistor : LabElement
    {
        public double resistance = 1.0;
        public Lab_Resistor(string name)
        {
            background = new BitmapImage(new Uri("Icons\\resistor.png", UriKind.Relative));
            br.ImageSource = background;
            rec.Width = 100;
            rec.Height = 100;
            rec.Fill = br;
            this.name = name;
            this.MouseDown += Resistor_MouseDown;
            _l.Margin = new System.Windows.Thickness(10, 10, 0, 0);
            _r.Margin = new System.Windows.Thickness(80, 80, 0, 0);
            _l_point.Margin = new System.Windows.Thickness(14, 15.5, 0, 0);
            _r_point.Margin = new System.Windows.Thickness(83.5, 82, 0, 0);
            RotateTransform transform = new RotateTransform(45, 5.5, 5.5);
            _l.RenderTransform = transform;
            _r.RenderTransform = transform;
        }

        private void Resistor_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            checkMe(name);
            showmenu(name);
        }
    }
}
