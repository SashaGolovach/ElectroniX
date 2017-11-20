using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace TUI_v0._3
{
    class Voltmeter : Element
    {
        public Voltmeter()
        {
            text.Margin = new System.Windows.Thickness(40, -10, 0, 0);
            background = new BitmapImage(new Uri("CircuitIcons\\voltmeter.png", UriKind.Relative));
            background_v = new BitmapImage(new Uri("CircuitIcons\\voltmeter_v.png", UriKind.Relative));
            br.ImageSource = background;
            rec.Width = 100;
            rec.Height = 100;
            rec.Fill = br;
            this.MouseDown += Resistor_MouseDown;
        }

        private void Resistor_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            checkMe(name.Text);
            showmenu(name.Text);
        }
    }
}
