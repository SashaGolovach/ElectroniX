using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace TUI_v0._3
{
    class Lamp : Element
    {
        public Lamp()
        {
            background = new BitmapImage(new Uri("CircuitIcons\\lamp.png", UriKind.Relative));
            background_v = new BitmapImage(new Uri("CircuitIcons\\lamp_v.png", UriKind.Relative));
            br.ImageSource = background;
            rec.Width = 100;
            rec.Height = 100;
            rec.Fill = br;
            text.Margin = new System.Windows.Thickness(35, -5, 0, 0);
            this.MouseDown += Resistor_MouseDown;
        }

        private void Resistor_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            checkMe(name.Text);
            showmenu(name.Text);
        }
    }
}
