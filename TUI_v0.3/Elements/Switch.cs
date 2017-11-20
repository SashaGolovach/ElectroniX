using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace TUI_v0._3.Elements
{
    class Switch : Element
    {
        public Switch()
        {
            text.Margin = new System.Windows.Thickness(40, -10, 0, 0);
            background = new BitmapImage(new Uri("CircuitIcons\\switch.png", UriKind.Relative));
            background_v = new BitmapImage(new Uri("CircuitIcons\\switch_v.png", UriKind.Relative));
            br.ImageSource = background;
            rec.Width = 100;
            rec.Height = 100;
            rec.Fill = br;
            this.MouseDown += Resistor_MouseDown;
        }

        private void Resistor_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Mouse.RightButton == MouseButtonState.Pressed)
            {
                isWire = !isWire;
                if (!isWire)
                {
                    background = new BitmapImage(new Uri("CircuitIcons\\switch.png", UriKind.Relative));
                    background_v = new BitmapImage(new Uri("CircuitIcons\\switch_v.png", UriKind.Relative));
                }
                else
                {
                    background = new BitmapImage(new Uri("CircuitIcons\\wire.png", UriKind.Relative));
                    background_v = new BitmapImage(new Uri("CircuitIcons\\wire_v.png", UriKind.Relative));
                }
                if (horizontal)
                    br.ImageSource = background;
                else
                    br.ImageSource = background_v;
                rec.Fill = br;
            }
            checkMe(name.Text);
            showmenu(name.Text);
        }
    }
}
