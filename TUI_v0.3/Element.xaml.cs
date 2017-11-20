using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TUI_v0._3
{
    /// <summary>
    /// Логика взаимодействия для Element.xaml
    /// </summary>
    public partial class Element : UserControl
    {
        public bool isWire = false;
        public double resistance = 1.0;
        public double E = 1.0;
        public double power = 30;
        internal Action<string> showmenu;
        internal Action<string> checkMe;
        public int ID; // id for scheme
        public bool isdrawing = false;
        public bool horizontal = true;
        public bool IsChecked = true;
        public ElementsTypes type = ElementsTypes.None;
        public delegate void ChangeCursor();
        public event ChangeCursor changecursor;
        public delegate void UnCheck();
        public event UnCheck uncheck;
        public delegate bool NotDrawing(Element el);
        public event NotDrawing notdrawing;
        public delegate void SaveLine(int id);
        public event SaveLine saveline;
        public delegate void TurnResitor(int id);
        public event TurnResitor turnme;
        public Action<int, string> removeFromWire;
        public BitmapImage background;
        public BitmapImage background_v;
        public ImageBrush br = new ImageBrush();
        public bool _r_checked = false, _l_checked = false;
        public Thickness margin;
        public double EDS = 5.0;
        public Point point;
        public Element()
        {
            InitializeComponent();
            rec.MouseLeave += Element_MouseLeave;
            rec.MouseEnter += Element_MouseEnter;
            _l.MouseLeave += _Clinch_MouseLeave;
            _l.MouseEnter += _Clinch_MouseEnter;
            _l.MouseDown += _Clinch_l_MouseDown;
            _r.MouseLeave += _Clinch_MouseLeave;
            _r.MouseEnter += _Clinch_MouseEnter;
            _r.MouseDown += _Clinch_r_MouseDown;
        }

        private void _Clinch_l_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!notdrawing(this))
            {
                if (_r_checked)
                {
                    MessageBox.Show("Do not connect element with itself");
                    return;
                }
                _l_checked = true;
                isdrawing = true;
            }
            else
            {
                _l_checked = true;
                saveline(ID);
            }
            uncheck();
        }

        private void _Clinch_r_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!notdrawing(this))
            {
                if (_l_checked)
                {
                    MessageBox.Show("Do not connect element with itself");
                    return;
                }
                _r_checked = true;
                isdrawing = true;
            }
            else
            {
                _r_checked = true;
                saveline(ID);
            }
            uncheck();
        }

        private void _Clinch_MouseEnter(object sender, MouseEventArgs e)
        {
            changecursor();
            if (sender == _l)
                _l.Opacity = 100;
            else
                _r.Opacity = 100;
        }

        private void _Clinch_MouseLeave(object sender, MouseEventArgs e)
        {
            changecursor();
            if (sender == _l)
                _l.Opacity = 0;
            else
                _r.Opacity = 0;
        }

        public void TurnLeft()
        {
            if (horizontal)
            {
                panel.Orientation = Orientation.Horizontal;
                _l.Margin = new Thickness(-57, -5, 0, 0);
                _r.Margin = new Thickness(-57, 92, 0, 0);
                _l_point.Margin = new Thickness(-50, 2, 0, 0);
                _r_point.Margin = new Thickness(-50, 98, 0, 0);
                xLeft.Margin = new Thickness(-70, -36, 0, 0);
                xRight.Margin = new Thickness(-70, 98, 0, 0);
                br.ImageSource = background_v;
                rec.Fill = br;
                if (type == ElementsTypes.Lamp)
                    text.Margin = new Thickness(15, 35, 0, 0);
                else
                    text.Margin = new Thickness(0, 35, 0, 0);
                horizontal = false;
            }
            else
            {
                panel.Orientation = Orientation.Vertical;
                _l.Margin = new Thickness(-5, -57, 0, 0);
                _r.Margin = new Thickness(92, -57, 0, 0);
                _l_point.Margin = new Thickness(2, -50, 0, 0);
                _r_point.Margin = new Thickness(98, -50, 0, 0);
                xLeft.Margin = new Thickness(-36, -70, 0, 0);
                xRight.Margin = new Thickness(98, -70, 0, 0);
                if (type == ElementsTypes.Lamp)
                    text.Margin = new Thickness(40, -30, 0, 0);
                else
                    text.Margin = new Thickness(40, -10, 0, 0);
                br.ImageSource = background;
                rec.Fill = br;
                text.Margin = new Thickness(40, -10, 0, 0);
                horizontal = true;
            }
            //turnme(ID);
        }

        private void Element_MouseEnter(object sender, MouseEventArgs e)
        {
            changecursor();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            removeFromWire(ID, "l");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            removeFromWire(ID, "r");
        }

        private void element_grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            point = Mouse.GetPosition(element_grid);
        }

        private void Element_MouseLeave(object sender, MouseEventArgs e)
        {
            changecursor();
        }
    }
}
