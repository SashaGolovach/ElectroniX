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
    /// Interaction logic for Multimeter.xaml
    /// </summary>
    public partial class Multimeter : UserControl
    {
        int angle = 0;
        public double resistance = 0.001;
        public double E = 1.0;
        public double resistanceMax = 1.0;
        public double EMax = 1.0;
        internal Action<string> showmenu;
        internal Action<string> checkMe;
        public List<Tuple<int, int, string, string, int>> connections = new List<Tuple<int, int, string, string, int>>();
        public int ID; // id for scheme
        public bool isdrawing = false;
        public bool IsChecked = true;
        public ElementsTypes type;
        public delegate void ChangeCursor();
        public event ChangeCursor changecursor;
        public delegate void UnCheck();
        public event UnCheck uncheck;
        public delegate bool NotDrawing(dynamic el);
        public event NotDrawing notdrawing;
        public delegate void SaveLine(int id);
        public event SaveLine saveline;
        public delegate void TurnResitor(int id);
        public event TurnResitor turnme;
        public BitmapImage background;
        public ImageBrush br = new ImageBrush();
        public bool _r_checked = false, _l_checked = false;
        public Thickness margin;
        public string name;
        public MultimeterDimension dimension = MultimeterDimension.Ammeter;

        public Multimeter(string name)
        {
            InitializeComponent();
            this.name = name;
            this.MouseLeave += Element_MouseLeave;
            this.MouseEnter += Element_MouseEnter;
            _l.MouseLeave += _Clinch_MouseLeave;
            _l.MouseEnter += _Clinch_MouseEnter;
            _l.MouseDown += _Clinch_l_MouseDown;
            _r.MouseLeave += _Clinch_MouseLeave;
            _r.MouseEnter += _Clinch_MouseEnter;
            _r.MouseDown += _Clinch_r_MouseDown;
            this.MouseDown += Resistor_MouseDown;
            
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
            angle += 45;
            RotateTransform rotateTransform1 = new RotateTransform(45);
            var scale = element_grid.RenderTransform;
            TransformGroup g = new TransformGroup();
            g.Children.Add(rotateTransform1);
            g.Children.Add(scale);
            element_grid.RenderTransform = g;
        }

        private void Element_MouseEnter(object sender, MouseEventArgs e)
        {
            changecursor();
        }

        private void Element_MouseLeave(object sender, MouseEventArgs e)
        {
            changecursor();
        }

        private void RadioButton_Click_1(object sender, RoutedEventArgs e)
        {
            dimension = MultimeterDimension.Voltmeter;
            resistance = 1000 * 1000 * 1000;
        }

        private void RadioButton_Click_2(object sender, RoutedEventArgs e)
        {
            dimension = MultimeterDimension.Ammeter;
            resistance = 0.001;
        }

        private void Resistor_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            checkMe(name);
            showmenu(name);
        }
    }
}
