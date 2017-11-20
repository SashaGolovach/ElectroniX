using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace TUI_v0._3
{
    public class EDS : Element
    {
        private void Resistor_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            checkMe(name.Text);
            showmenu(name.Text);
        }
        public EDS(int type)
        {
            if (type == 1)
            {
                background = new BitmapImage(new Uri("CircuitIcons\\rCell.png", UriKind.Relative));
                background_v = new BitmapImage(new Uri("CircuitIcons\\rCell_v.png", UriKind.Relative));
            }
            else
            {
                background = new BitmapImage(new Uri("CircuitIcons\\lCell.png", UriKind.Relative));
                background_v = new BitmapImage(new Uri("CircuitIcons\\lCell_v.png", UriKind.Relative));
            }
            br.ImageSource = background;
            rec.Width = 100;
            rec.Height = 100;
            rec.Fill = br;
            this.MouseDown += Resistor_MouseDown;
        }
    }
}
