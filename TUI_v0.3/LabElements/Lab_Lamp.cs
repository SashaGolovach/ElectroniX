using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace TUI_v0._3
{
    class Lab_Lamp : LabElement
    {
        public Lab_Lamp(string name)
        {
            background = new BitmapImage(new Uri("Icons\\lamp.png", UriKind.Relative));
            br.ImageSource = background;
            rec.Width = 100;
            rec.Height = 100;
            rec.Fill = br;
            this.name = name;
            this.MouseDown += Resistor_MouseDown;
            _l.Margin = new System.Windows.Thickness(35, 88, 0, 0);
            _r.Margin = new System.Windows.Thickness(54, 93, 0, 0);
            _l_point.Margin = new System.Windows.Thickness(39, 90, 0, 0);
            _r_point.Margin = new System.Windows.Thickness(59, 95, 0, 0);
        }

        private void Resistor_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            checkMe(name);
            showmenu(name);
        }


        public void ChangeTheme(string theme)
        {
            background = new BitmapImage(new Uri("Icons\\lamp" + theme + ".png", UriKind.Relative));
            br.ImageSource = background;
            rec.Width = 100;
            rec.Height = 100;
            rec.Fill = br;
        }
    }
}
