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
    class Resistor : Element
    {
        public Resistor()
        {
            background = new BitmapImage(new Uri("CircuitIcons\\resistor.png", UriKind.Relative));
            background_v = new BitmapImage(new Uri("CircuitIcons\\resistor_v.png", UriKind.Relative));
            br.ImageSource = background;
            rec.Width = 100;
            rec.Height = 100;
            rec.Fill = br;
            this.MouseDown += Resistor_MouseDown;
        }

        private void Resistor_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
            checkMe(name.Text);
            showmenu(name.Text);
        }
    }
}
