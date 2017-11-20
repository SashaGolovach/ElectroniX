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
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System.Collections.ObjectModel;
using System.IO;
using Microsoft.Win32;

namespace TUI_v0._3
{
    /// <summary>
    /// Логика взаимодействия для ElectronicCircuitLab.xaml
    /// </summary>
    [Serializable]
    public class Wire
    {
        public List<Polyline> lines = new List<Polyline>();
        public List<Tuple<int, string>> versits = new List<Tuple<int, string>>(); // list of id and pos of each element
        public bool ischecked = false;
        public int ID;
        public Wire(int a, int b, string c, string d)
        {
            versits.Add(new Tuple<int, string>(a, c));
            versits.Add(new Tuple<int, string>(b, d));
        }
        public Wire()
        {

        }
    }

    public partial class ElectronicCircuitLab : UserControl
    {
        public Action<string, Kinds> createCircuit;
        public static double scale = 1;
        public Cursor cursor = Cursors.Arrow;
        public List<Ellipse> ellipses = new List<Ellipse>();
        public List<Element> elements = new List<Element>();
        public List<Wire> wires = new List<Wire>();
        public delegate void ChangeCursor();
        public event ChangeCursor changecursor;
        public delegate void CloseWindow();
        public event CloseWindow closewindow;
        public delegate void SaveProject(dynamic p);
        public event SaveProject saveproject;
        public delegate void WindowChanged(int type);
        public event WindowChanged windowchanged;
        public delegate void Undo();
        public event Undo undo;
        public delegate void Redo();
        public event Redo redo;
        int object1, object2;
        bool mouse_up = false;
        public Line path_temp = new Line() { StrokeThickness = 4 * scale, Stroke = Brushes.DarkBlue };
        ScriptEngine engine = Python.CreateEngine(); // engine for python
        ScriptScope scope; // scope
        public dynamic s;
        bool isTesting = false;
        Rectangle rec = new Rectangle();
        public bool showgrid { get; set; } = false;
        public string path;
        public Point? lastDragPoint;

        public ElectronicCircuitLab()
        {
            //try
            //{
            InitializeComponent();
            Rectangle r = new Rectangle() { Width = 5, Height = 5, Margin = new Thickness(8000, 8000, 0, 0) };
            workspace.Children.Add(r);
            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + 4000);
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + 4000);
            rec.Width = workspace.Width;
            rec.Height = workspace.Height;
            rec.Fill = Brushes.Gray;
            rec.Opacity = 0;
            rec.MouseDown += WorkSpace_MouseDown;
            SetCards();
            workspace.Children.Add(rec);
            workspace.MouseMove += ElectronicCircuitLab_MouseMove;
            workspace.Children.Add(path_temp);
            // connection with scheme
            ICollection<string> paths = engine.GetSearchPaths();
            string dir = @"Lib\";
            paths.Add(dir);
            engine.SetSearchPaths(paths);
            scope = engine.CreateScope();
            string path = System.IO.Directory.GetCurrentDirectory();
            engine.ExecuteFile(path + @"\scheme.py", scope);
            s = scope.GetVariable("s");
            //}
            //catch(Exception ex)
            //{
            //    MessageBox.Show(ex.ToString());
            //}
        }

        void ShowResults()
        {
            RenderTargetBitmap bmp = new RenderTargetBitmap(1920, 1080, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(this.workspace);
            var brush = new ImageBrush() {ImageSource = bmp, Stretch = Stretch.UniformToFill};
            screen.Background = brush;
            Grid.SetZIndex(data_grid, 100);
            data_grid_panel.Opacity = 100;
            workspace.Opacity = 0;
            workspace.IsEnabled = false;
            Grid.SetZIndex(data_grid_panel, 100000);
        }

        void CloseResults()
        {
            Grid.SetZIndex(data_grid, -10);
            data_grid_panel.Opacity = 0;
            workspace.Opacity = 100;
            workspace.IsEnabled = true;
            Grid.SetZIndex(data_grid_panel, 0);
        }

        private void Path_temp_MouseLeave(object sender, MouseEventArgs e)
        {
            var temp = (Polyline)sender;
            for (int i = 0; i < wires.Count; i++)
            {
                if(!wires[i].ischecked)
                {
                    for (int j = 0; j < wires[i].lines.Count; j++)
                    {
                        wires[i].lines[j].Stroke = Brushes.Black;
                        wires[i].lines[j].StrokeThickness = 4 * scale;
                    }
                }
            }
            cursor = Cursors.Arrow;
            changecursor();
        }

        private void Path_temp_MouseEnter(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < wires.Count; i++)
            {
                if (wires[i].lines.Contains((Polyline)sender))
                {
                    for (int j = 0; j < wires[i].lines.Count; j++)
                    {
                        wires[i].lines[j].StrokeThickness = 6.5 * scale;
                    }
                    cursor = Cursors.Hand;
                    changecursor();
                    return;
                }
            }
        }

        private void Path_temp_MouseDown(object sender, MouseButtonEventArgs e)
        {
            uncheck();
            var temp = (Polyline)sender;
            for (int i = 0; i < wires.Count; i++)
            {
                if (wires[i].lines.Contains((Polyline)sender))
                {
                    for (int j = 0; j < wires[i].lines.Count; j++)
                    {
                        wires[i].lines[j].StrokeThickness = 6.5 * scale;
                    }
                    cursor = Cursors.Hand;
                    changecursor();
                }
            }
            for (int i = 0; i < wires.Count; i++)
                for (int j = 0; j < wires[i].lines.Count; j++)
                    if (wires[i].lines[j] == temp)
                {
                        foreach (var v in wires[i].versits)
                        {
                            if (v.Item2 == "l")
                            {
                                elements[indexbyid(v.Item1)].xLeft.Opacity = 1;
                                elements[indexbyid(v.Item1)].xLeft.IsEnabled = true;
                            }
                            else
                            {
                                elements[indexbyid(v.Item1)].xRight.Opacity = 1;
                                elements[indexbyid(v.Item1)].xRight.IsEnabled = true;
                            }
                        }
                    wires[i].ischecked = true;
                }
        }

        private void WorkSpace_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            lastDragPoint = Mouse.GetPosition(scrollViewer);
            workspace.Children.Remove(path_temp);
            path_temp = new Line() { StrokeThickness = 4 * scale, Stroke = Brushes.DarkBlue };
            workspace.Children.Add(path_temp);
            uncheck();
        }

        public void ElectronicCircuitLab_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = Mouse.GetPosition(workspace);
            bool rez = false;
            for (int i = 0; i < elements.Count; i++)
                if (elements[i].isdrawing) rez = true;
            if (e.LeftButton == MouseButtonState.Pressed && !rez)
            {
                bool notChecked = true;
                for (int i = 0; i < elements.Count; i++)
                {
                    if (elements[i].IsChecked && !(elements[i]._l_checked || elements[i]._r_checked))
                    {
                        notChecked = false;
                        var acc = elements[i].element_grid.Width / 2;
                        if (elements[i].point != new Point(0,0))
                            elements[i].Margin = new Thickness(p.X - elements[i].point.X, p.Y - elements[i].point.Y, 0, 0);
                        else
                            elements[i].Margin = new Thickness(p.X - acc, p.Y - acc, 0, 0);
                        if (!mouse_up)
                        {
                            mouse_up = true;
                            MouseUp += ElectronicCircuitLab_MouseUp;
                        }
                    }
                }
                if (notChecked)
                {
                    if (lastDragPoint.HasValue)
                    {
                        Point posNow = Mouse.GetPosition(scrollViewer);

                        double dX = posNow.X - lastDragPoint.Value.X;
                        double dY = posNow.Y - lastDragPoint.Value.Y;

                        lastDragPoint = posNow;

                        scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - dX);
                        scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - dY);
                    }
                }
            }
            else
            {
                for (int i = 0; i < elements.Count; i++)
                {
                    if (elements[i].isdrawing)
                    {
                        Point relativePoint = elements[i]._l_point.TransformToAncestor(workspace).Transform(new Point(0, 0));
                        double l_top = relativePoint.Y, l_left = relativePoint.X;
                        relativePoint = elements[i]._r_point.TransformToAncestor(workspace).Transform(new Point(0, 0));
                        double r_top = relativePoint.Y, r_left = relativePoint.X;
                        if (elements[i]._l_checked)
                        {
                            path_temp.X1 = l_left;
                            path_temp.Y1 = l_top;
                            path_temp.X2 = p.X - 5;
                            path_temp.Y2 = p.Y - 5;
                            break;
                        }
                        else if (elements[i]._r_checked)
                        {
                            path_temp.X1 = r_left;
                            path_temp.Y1 = r_top;
                            path_temp.X2 = p.X - 5;
                            path_temp.Y2 = p.Y - 5;
                            break;
                        }
                    }
                }
            }
        }

        private void ElectronicCircuitLab_MouseUp(object sender, MouseButtonEventArgs e)
        {
            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].IsChecked)
                {
                    mouse_up = false;
                    MouseUp -= ElectronicCircuitLab_MouseUp;
                    UpdateWires();
                    windowchanged(0); // save moment
                    break;
                }
            }
        }

        private void AddComponentClick(object sender, RoutedEventArgs e)
        {
            if (!isTesting)
            {
                drawerhost.IsBottomDrawerOpen = true;
            }
        }

        private void CloseAddComponentPanel(object sender, RoutedEventArgs e)
        {
            drawerhost.IsBottomDrawerOpen = false;
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            cursor = Cursors.Hand;
            changecursor();
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            cursor = Cursors.Arrow;
            changecursor();
        }

        public void CreateResistor_Click(object sender, MouseButtonEventArgs e)
        {
            drawerhost.IsBottomDrawerOpen = false;
            Resistor el = new Resistor(); // change the name
            el.ID = (int)s.addElement("resistor"); // add el to scheme
            el.shortname.Text = "R";
            el.scale.ScaleX = scale;
            el.scale.ScaleY = scale;
            el.element_grid.Width = 400;
            el.element_grid.Height = 400;
            int name = 1;
            while (true)
            {
                if (ContainsName(name.ToString()))
                {
                    name++;
                }
                else
                    break;
            }
            el.removeFromWire += removeFromWire;
            el.name.Text = name.ToString();
            el.changecursor += callevent;
            el.showmenu += ShowMenu;
            el.checkMe += El_checkMe;
            el.uncheck += El_uncheck;
            El_checkMe(el.name.Text);
            el.saveline += El_saveline;
            el.notdrawing += El_notdrawing;
            el.type = ElementsTypes.Resistor;
            elements.Add(el);
            ShowMenu(el.name.Text);
            Point p = Mouse.GetPosition(workspace);
            el.HorizontalAlignment = HorizontalAlignment.Left;
            el.VerticalAlignment = VerticalAlignment.Top;
            el.Margin = new Thickness(p.X - 50, p.Y - 50, 0, 0);
            el.turnme += El_turnme;
            Panel.SetZIndex(el, 30);
            workspace.Children.Add(el);
            if (sender != null)
                windowchanged(1); // save moment
        }

        private void El_uncheck()
        {
            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].IsChecked) UpdateProperties(i);
                elements[i].IsChecked = false;
            }
        }

        private void El_turnme(int id)
        {
            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].IsChecked)
                {
                    UpdateWires();
                    break;
                }
            }
        }

        private bool El_notdrawing(Element el)
        {
            int v = -1;
            bool rez = false;
            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i] == el) v = i;
                else if (elements[i].isdrawing) rez = true;
            }
            if (rez) object2 = elements[v].ID;
            else object1 = elements[v].ID;
            return rez;
        }

        private void El_saveline(int id)
        {
            try
            {
                int first = -1, second = -1;
                for (int i = 0; i < elements.Count; i++)
                {
                    if (elements[i].ID == id)
                        second = i;
                    else if (elements[i].isdrawing)
                        first = i;
                    elements[i].isdrawing = false;
                }
                string cf, cs;
                if (elements[first]._l_checked) cf = "l"; else cf = "r";
                if (elements[second]._l_checked) cs = "l"; else cs = "r";
                var temp = new Tuple<int, int, string, string>(object1, object2, cf, cs);
                s.connectElement(object1, cf, object2, cs); // connect with scheme
                bool found = false;
                /////////////
                Wire w1 = new Wire(), w2 = new Wire();
                foreach (var w in wires)
                {
                    if (w.versits.Contains(new Tuple<int, string>(object1, cf)))
                        w2 = w;
                    if (w.versits.Contains(new Tuple<int, string>(object2, cs)))
                        w1 = w;
                }
                if(w1 != w2 && !(w1.versits.Count == 0 && w2.versits.Count == 0))
                {
                    if(w1.versits.Count == 0)
                    {
                        wires[wires.IndexOf(w2)].versits.Add(new Tuple<int, string>(object2, cs));
                    }
                    else if(w2.versits.Count == 0)
                    {
                        wires[wires.IndexOf(w1)].versits.Add(new Tuple<int, string>(object1, cf));
                    }
                    else
                    {
                        var versits = new List<Tuple<int, string>>();
                        foreach (var el in w1.versits)
                            versits.Add(el);
                        foreach (var el in w2.versits)
                            versits.Add(el);
                        wires[wires.IndexOf(w1)].versits = versits;
                        foreach (var l in w2.lines)
                            workspace.Children.Remove(l);
                        wires.Remove(w2);
                    }
                    found = true;
                }
                ////////////
                if (!found)
                {
                    wires.Add(new Wire(object1, object2, cf, cs));
                }
                workspace.Children.Remove(path_temp);
                path_temp = new Line() { StrokeThickness = 4 * scale, Stroke = Brushes.DarkBlue };
                workspace.Children.Add(path_temp);
                elements[first]._l_checked = false; elements[first]._r_checked = false;
                elements[second]._l_checked = false; elements[second]._r_checked = false;
                El_uncheck();
                UpdateWires();
                windowchanged(1); // save moment
                return;
            }
            catch (Exception ex)
            {
               // MessageBox.Show(ex.ToString());
            }
        }

        private void El_checkMe(string a)
        {
            for (int i = 0; i < elements.Count; i++)
                elements[i].IsChecked = elements[i].name.Text == a;
            for (int i = 0; i < wires.Count; i++)
            {
                wires[i].ischecked = false;
                try
                {
                    foreach (var v in wires[i].versits)
                    {
                        elements[indexbyid(v.Item1)].xLeft.Opacity = 0;
                        elements[indexbyid(v.Item1)].xLeft.IsEnabled = false;
                        elements[indexbyid(v.Item1)].xRight.Opacity = 0;
                        elements[indexbyid(v.Item1)].xRight.IsEnabled = false;
                    }
                }
                catch { }
                foreach (var el in wires[i].lines)
                {
                    el.StrokeThickness = 4 * scale;
                }
            }
        }

        void callevent()
        {
            if (cursor == Cursors.Hand) cursor = Cursors.Arrow;
            else cursor = Cursors.Hand;
            changecursor();
        }

        public void uncheck()
        {
            try
            {
                workspace.Children.Remove(path_temp);
                path_temp = new Line() { StrokeThickness = 4 * scale, Stroke = Brushes.DarkBlue };
                workspace.Children.Add(path_temp);
                for (int i = 0; i < elements.Count; i++)
                {
                    if (elements[i].IsChecked) UpdateProperties(i);
                    elements[i].IsChecked = false;
                    elements[i].isdrawing = false;
                    elements[i]._r_checked = false;
                    elements[i]._l_checked = false;
                }
                dhost.IsOpen = false;
                for (int i = 0; i < wires.Count; i++)
                {
                    foreach (var v in wires[i].versits)
                    {
                        elements[indexbyid(v.Item1)].xLeft.Opacity = 0;
                        elements[indexbyid(v.Item1)].xLeft.IsEnabled = false;
                        elements[indexbyid(v.Item1)].xRight.Opacity = 0;
                        elements[indexbyid(v.Item1)].xRight.IsEnabled = false;
                    }
                    wires[i].ischecked = false;
                    foreach (var el in wires[i].lines)
                    {
                        el.StrokeThickness = 4 * scale;
                    }
                }
            }
            catch { }
        }

        void UpdateProperties(int i)
        {
            try
            {
                if (!ContainsName(NameTextBox.Text) && !String.IsNullOrWhiteSpace(NameTextBox.Text))
                    elements[i].name.Text = NameTextBox.Text;
                if (!String.IsNullOrWhiteSpace(resistance_box.Text))
                {
                    try
                    {
                        if (resistance_box.Text.Contains("."))
                        {
                            resistance_box.Text = resistance_box.Text.Insert(resistance_box.Text.IndexOf("."), ",");
                            resistance_box.Text = resistance_box.Text.Remove(resistance_box.Text.IndexOf("."), 1);
                        }
                        elements[i].resistance = double.Parse(resistance_box.Text);
                        if (elements[i].type == ElementsTypes.Lamp)
                            s.setProperty(elements[i].ID, "critP", double.Parse(resistance_box.Text)); // property to scheme
                        else
                            s.setProperty(elements[i].ID, "R", double.Parse(resistance_box.Text)); // property to scheme
                    }
                    catch (Exception ex){
                        MessageBox.Show("В даному параметрі 0 неможливий");
                    }
                }
                if (elements[i].type == ElementsTypes.EDS)
                {
                    if (!String.IsNullOrWhiteSpace(EDS_box.Text))
                    {
                        try
                        {
                            if (EDS_box.Text.Contains("."))
                            {
                                EDS_box.Text = EDS_box.Text.Insert(EDS_box.Text.IndexOf("."), ",");
                                EDS_box.Text = EDS_box.Text.Remove(EDS_box.Text.IndexOf("."), 1);
                            }
                            elements[i].E = double.Parse(EDS_box.Text);
                            s.setProperty(elements[i].ID, "E", double.Parse(EDS_box.Text)); // property to scheme
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("В даному параметрі 0 неможливий");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private bool ContainsName(string text)
        {
            for (int i = 0; i < elements.Count; i++)
                if (elements[i].name.Text == text) return true;
            return false;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!isTesting)
                {
                    for (int i = 0; i < elements.Count; i++)
                        if (elements[i].IsChecked)
                        {
                            removeFromWire(elements[i].ID, "r");
                            removeFromWire(elements[i].ID, "l");
                            workspace.Children.Remove((UIElement)elements[i]);
                            workspace.Children.Remove(elements[i]._l);
                            workspace.Children.Remove(elements[i]._r);
                            s.removeElement(elements[i].ID);
                            elements.RemoveAt(i);
                            windowchanged(1); // save moment
                            break;
                        }
                    dhost.IsOpen = false;
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }

        int indexbyid(int id)
        {
            for (int i = 0; i < elements.Count; i++)
                if (elements[i].ID == id) return i;
            for (int i = 0; i < wires.Count; i++)
                if (wires[i].ID == id) return i;// throw exception
            return -1;
        }

        private void TurnButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isTesting)
            {
                foreach (Element el in elements)
                    if (el.IsChecked)
                    {
                        el.TurnLeft();
                        ExtensionMethods.Refresh(el);
                        El_turnme(el.ID);
                        windowchanged(0); // save moment
                        break;
                    }
            }
        }

        void ShowMenu(string name)
        {
            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].type == ElementsTypes.Switch)
                    s.setProperty(elements[i].ID, "state", elements[i].isWire);
                if (elements[i].name.Text == name)
                {
                     NameTextBox.Text = elements[i].name.Text;
                    if(elements[i].type == ElementsTypes.Switch)
                    {
                        resistance_box.IsEnabled = false;
                    }
                    if (elements[i].type == ElementsTypes.Lamp)
                        resistance_short.Text = "P";
                    else
                        resistance_short.Text = "r";
                    resistance_box.Text = elements[i].resistance.ToString();
                    dhost.IsOpen = true;
                    if (elements[i].type == ElementsTypes.EDS)
                    {
                        EDS_box.IsEnabled = true;
                        EDS_box.Text = elements[i].E.ToString();
                        EDSPanel.Opacity = 1;
                    }
                    else
                    {
                        EDS_box.IsEnabled = false;
                        EDS_box.Text = "";
                        EDSPanel.Opacity = 0;
                    }
                    UpdateProperties(i);
                    break;
                }
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                for (int i = 0; i < elements.Count; i++)
                    if (elements[i].IsChecked) UpdateProperties(i);
                windowchanged(1); // save moment
            }
        }

        void SetCards()
        {
            BitmapImage bmp = new BitmapImage(new Uri("CircuitIcons\\resistor.png", UriKind.Relative));
            ImageBrush br = new ImageBrush();
            br.ImageSource = bmp;
            br.Stretch = Stretch.Fill;
            resistor_card.Fill = br; // backbround of a card
            bmp = new BitmapImage(new Uri("CircuitIcons\\rCell.png", UriKind.Relative));
            br = new ImageBrush();
            br.ImageSource = bmp;
            rEds_card.Fill = br;
            bmp = new BitmapImage(new Uri("CircuitIcons\\ammeter.png", UriKind.Relative));
            br = new ImageBrush();
            br.ImageSource = bmp;
            Ammeter.Fill = br;
            bmp = new BitmapImage(new Uri("CircuitIcons\\voltmeter.png", UriKind.Relative));
            br = new ImageBrush();
            br.ImageSource = bmp;
            Voltmeter.Fill = br;
            bmp = new BitmapImage(new Uri("CircuitIcons\\lamp.png", UriKind.Relative));
            br = new ImageBrush();
            br.ImageSource = bmp;
            Lamp.Fill = br;
            bmp = new BitmapImage(new Uri("CircuitIcons\\switch.png", UriKind.Relative));
            br = new ImageBrush();
            br.ImageSource = bmp;
            Switch_card.Fill = br;
        }

        private void CreateLabWorkClick(object sender, RoutedEventArgs e)
        {
            createCircuit("Experiment", Kinds.LabWork);
        }

        private void StartButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {              
                if (!isTesting)
                {
                    s.updateI(); // calculate scheme
                    string strstrstr = "";
                    foreach (var el in elements)
                        strstrstr += el.ID + " ";
                    //MessageBox.Show(strstrstr);
                    for (int i = 0; i < elements.Count; i++)
                        if (elements[i].IsChecked) UpdateProperties(i);
                    ObservableCollection<Data> items = new ObservableCollection<Data>();
                    ShowResults();
                    for (int i = 0; i < elements.Count; i++)
                    {
                        if (elements[i].type != ElementsTypes.Switch)
                        {
                            dynamic el = s.ids[elements[i].ID];
                            if (elements[i].type == ElementsTypes.Lamp && double.Parse(((object)el.U).ToString()) * double.Parse(((object)el.I).ToString()) >= elements[i].resistance)
                                items.Add(new Data(elements[i].shortname.Text + elements[i].name.Text, elements[i].type.ToString(), ((object)el.R).ToString() + " Om", ((object)el.U).ToString() + " V", ((object)el.I).ToString() + " Am", Brushes.DarkRed));
                            else
                                items.Add(new Data(elements[i].shortname.Text + elements[i].name.Text, elements[i].type.ToString(), ((object)el.R).ToString() + " Om", ((object)el.U).ToString() + " V", ((object)el.I).ToString() + " Am", Brushes.Black));
                        }
                    }
                    isTesting = true;
                    data_grid.ItemsSource = items;
                    dhost.IsOpen = false;
                }
                else
                {
                    CloseResults();
                    isTesting = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void Show_Grid_Click(object sender, RoutedEventArgs e)
        {
            if (!isTesting)
            {
                showgrid = !showgrid;
                MaterialDesignThemes.Wpf.PackIcon icon = new MaterialDesignThemes.Wpf.PackIcon();
                if (showgrid)
                    icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.GridOff;
                else
                    icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Grid;
                ShowGridButton.Content = icon;
                work_grid.Opacity = showgrid ? 1 : 0;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.Z))
                undo();
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.Y))
                redo();
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.S))
                saveproject(Serialization.PreSerialize(MainWindow.lab));
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&  e.Key == Key.R)
            {
                CreateResistor_Click(null, null);
                windowchanged(1);
            }
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.A)
            {
                CreateAmmeter_Click(null, null);
                windowchanged(1);
            }
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.L)
            {
                CreateLamp_Click(null, null);
                windowchanged(1);
            }
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.V)
            {
                CreateVoltmeter_Click(null, null);
                windowchanged(1);
            }
            else if(Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.E)
            {
                CreateRCapacitor_Click(null, null);
                windowchanged(1);
            }
            else if(Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.A)
            {
                CreateAmmeter_Click(null, null);
                windowchanged(1);
            }
            switch (e.Key)
            {
                case Key.Delete:
                    DeleteButton_Click(null, null);
                    break;
            }
        }

        private void CreateCurcuitClick(object sender, RoutedEventArgs e)
        {
            createCircuit("CircuitProject", Kinds.ElectronicCircuit);
        }

        public void CreateRCapacitor_Click(object sender, MouseButtonEventArgs e)
        {
            drawerhost.IsBottomDrawerOpen = false;
            EDS el = new EDS(1); // change the name
                el.ID = (int)s.addElement("rEMF"); // add el to scheme
            el.shortname.Text = "E";
            el.scale.ScaleX = scale;
            el.scale.ScaleY = scale;
            el.element_grid.Width = 400;
            el.element_grid.Height = 400;
            int name = 1;
            while (true)
            {
                if (ContainsName(name.ToString()))
                {
                    name++;
                }
                else
                    break;
            }
            el.removeFromWire += removeFromWire;
            el.name.Text = name.ToString();
            el.changecursor += callevent;
            el.showmenu += ShowMenu;
            el.checkMe += El_checkMe;
            el.uncheck += El_uncheck;
            El_checkMe(el.name.Text);
            el.saveline += El_saveline;
            el.notdrawing += El_notdrawing;
            el.type = ElementsTypes.EDS;
            elements.Add(el);
            ShowMenu(el.name.Text);
            Point p = Mouse.GetPosition(workspace);
            el.HorizontalAlignment = HorizontalAlignment.Left;
            el.VerticalAlignment = VerticalAlignment.Top;
            el.Margin = new Thickness(p.X - 50, p.Y - 50, 0, 0);
            el.turnme += El_turnme;
            Panel.SetZIndex(el, 30);
            workspace.Children.Add(el);
            if (sender != null)
                windowchanged(1); // save moment
        }

        private void RedoButtonClick(object sender, RoutedEventArgs e)
        {
            if (!isTesting)
            {
                redo();
            }
        }

        private void UndoButtonClick(object sender, RoutedEventArgs e)
        {
            if (!isTesting)
            {
                undo();
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            closewindow();
        }

        private void SaveAsButton_Click(object sender, RoutedEventArgs e)
        {
            saveproject(Serialization.PreSerialize(MainWindow.lab));
        }

        public void ShowElements()
        {
            workspace.Children.Clear();
            foreach (var el in elements) // change me
            {
                el.changecursor += callevent;
                el.showmenu += ShowMenu;
                el.checkMe += El_checkMe;
                el.uncheck += El_uncheck;
                el.saveline += El_saveline;
                el.notdrawing += El_notdrawing;
                el.turnme += El_turnme;
                el.HorizontalAlignment = HorizontalAlignment.Left;
                el.VerticalAlignment = VerticalAlignment.Top;
                el.Margin = el.margin;
                workspace.Children.Add(el);
            }
            foreach (var el in wires)
            {
                foreach (var l in el.lines)
                {
                    l.MouseDown += Path_temp_MouseDown;
                    l.MouseEnter += Path_temp_MouseEnter;
                    l.MouseLeave += Path_temp_MouseLeave;
                    if (!workspace.Children.Contains(l))
                        workspace.Children.Add(l);
                }
            }
            UpdateWires();
            WorkSpace_MouseDown(null, null);
        }

        public void SaveScheme(string save_path)
        {
            try
            {
                string path = save_path.Remove(save_path.Length - 4, 4) + "_scheme.bin";
                s.save(path);
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }

        public void LoadScheme(string save_path)
        {
            s = s.load(save_path);
        }

        public void CreateVoltmeter_Click(object sender, MouseButtonEventArgs e)
        {
            drawerhost.IsBottomDrawerOpen = false;
            Voltmeter el = new Voltmeter(); // change the name
                el.ID = (int)s.addElement("voltmeter"); // add el to scheme
            s.setProperty(el.ID, "R", 100000000);
            el.resistance = 100000000;
            el.shortname.Text = "U";
            el.scale.ScaleX = scale;
            el.scale.ScaleY = scale;
            el.element_grid.Width = 400;
            el.element_grid.Height = 400;
            int name = 1;
            while (true)
            {
                if (ContainsName(name.ToString()))
                {
                    name++;
                }
                else
                    break;
            }
            el.name.Text = name.ToString();
            el.changecursor += callevent;
            el.showmenu += ShowMenu;
            el.checkMe += El_checkMe;
            el.uncheck += El_uncheck;
            El_checkMe(el.name.Text);
            el.saveline += El_saveline;
            el.notdrawing += El_notdrawing;
            el.type = ElementsTypes.Voltmeter;
            elements.Add(el);
            el.removeFromWire += removeFromWire;
            ShowMenu(el.name.Text);
            Point p = Mouse.GetPosition(workspace);
            el.HorizontalAlignment = HorizontalAlignment.Left;
            el.VerticalAlignment = VerticalAlignment.Top;
            el.Margin = new Thickness(p.X - 50, p.Y - 50, 0, 0);
            el.turnme += El_turnme;
            Panel.SetZIndex(el, 30);
            workspace.Children.Add(el);
            if (sender != null)
                windowchanged(1); // save moment
        }

        public void CreateAmmeter_Click(object sender, MouseButtonEventArgs e)
        {
            drawerhost.IsBottomDrawerOpen = false;
            Ammeter el = new Ammeter(); // change the name
                el.ID = (int)s.addElement("ammeter"); // add el to scheme
            el.shortname.Text = "I";
            el.scale.ScaleX = scale;
            el.scale.ScaleY = scale;
            el.element_grid.Width = 400;
            el.element_grid.Height = 400;
            int name = 1;
            while (true)
            {
                if (ContainsName(name.ToString()))
                {
                    name++;
                }
                else
                    break;
            }
            el.name.Text = name.ToString();
            el.changecursor += callevent;
            el.showmenu += ShowMenu;
            el.checkMe += El_checkMe;
            el.uncheck += El_uncheck;
            El_checkMe(el.name.Text);
            el.saveline += El_saveline;
            el.notdrawing += El_notdrawing;
            el.type = ElementsTypes.Ammeter;
            elements.Add(el);
            ShowMenu(el.name.Text);
            el.removeFromWire += removeFromWire;
            Point p = Mouse.GetPosition(workspace);
            el.HorizontalAlignment = HorizontalAlignment.Left;
            el.VerticalAlignment = VerticalAlignment.Top;
            el.Margin = new Thickness(p.X - 50, p.Y - 50, 0, 0);
            el.turnme += El_turnme;
            Panel.SetZIndex(el, 30);
            workspace.Children.Add(el);
            if (sender != null)
                windowchanged(1); // save moment
        }

        public void CreateLamp_Click(object sender, MouseButtonEventArgs e)
        {
            drawerhost.IsBottomDrawerOpen = false;
            Lamp el = new Lamp(); // change the name
                el.ID = (int)s.addElement("lamp"); // add el to scheme
            int name = 1;
            while(true)
            {
                if (ContainsName(name.ToString()))
                {
                    name++;
                }
                else
                    break;
            }
            el.shortname.Text = "L";
            el.name.Text = name.ToString();
            el.scale.ScaleX = scale;
            el.scale.ScaleY = scale;
            el.element_grid.Width = 400;
            el.element_grid.Height = 400;
            el.changecursor += callevent;
            el.showmenu += ShowMenu;
            el.checkMe += El_checkMe;
            el.uncheck += El_uncheck;
            El_checkMe(el.name.Text);
            el.saveline += El_saveline;
            el.removeFromWire += removeFromWire;
            el.notdrawing += El_notdrawing;
            el.type = ElementsTypes.Lamp;
            elements.Add(el);
            ShowMenu(el.name.Text);
            Point p = Mouse.GetPosition(workspace);
            el.HorizontalAlignment = HorizontalAlignment.Left;
            el.VerticalAlignment = VerticalAlignment.Top;
            el.Margin = new Thickness(p.X - 50, p.Y - 50, 0, 0);
            el.turnme += El_turnme;
            Panel.SetZIndex(el, 30);
            workspace.Children.Add(el);
            if (sender != null)
                windowchanged(1); // save moments
        }

        private void BackToHome_Click(object sender, RoutedEventArgs e)
        {
            window.Content = MainWindow.hpage;
        }

        PointCollection Poly(double x1, double y1, double x2, double y2, string c1, string c2, bool v1, bool v2)
        {
            var ellipse = new Ellipse();
            double x, y;
            PointCollection points = new PointCollection();
            if(v1 || v2)
            {
                if(v1 && v2 || (c1 == "p" || c2 == "p"))
                {
                    var c = x1;
                    x1 = y1;
                    y1 = c;
                    c = x2;
                    x2 = y2;
                    y2 = c;
                }
                else
                {
                    if(y2 >y1 - 100 && y2 < y1 + 100)
                    {
                        if ((v1 && c1 == "l") || (v2 && c2 == "l"))
                        {
                            points.Add(new Point(x1, y1));
                            points.Add(new Point(x1, Math.Min(y1, y2) - 30));
                            points.Add(new Point(x2, Math.Min(y1, y2) - 30));
                            points.Add(new Point(x2, y2));
                        }
                        else
                        {
                            points.Add(new Point(x1, y1));
                            points.Add(new Point(x1, Math.Max(y1, y2) + 30));
                            points.Add(new Point(x2, Math.Max(y1, y2) + 30));
                            points.Add(new Point(x2, y2));
                        }
                        return points;
                    }
                    if (v1)
                    {
                        points.Add(new Point(x1,y1));
                        points.Add(new Point(x2, y1));
                        points.Add(new Point(x2, y2));
                    }
                    else
                    {
                        points.Add(new Point(x1, y1));
                        points.Add(new Point(x1, y2));
                        points.Add(new Point(x2, y2));
                    }
                    return points;
                }
            }
            if (c2 != "p" && c1 != "p")
            {
                points.Add(new Point(x1, y1));
                if (x1 > x2 && y1 > y2 && c1 == "r" && c2 == "l" && x2 > x1 - 100)
                {
                    points.Add(new Point(x1, y1 + 30));
                    points.Add(new Point(x1 - 130, y1 + 30));
                    points.Add(new Point(x1 - 130, y2));
                }
                else if (x1 < x2 && y1 < y2 && c1 == "l" && c2 == "r" && x2 < x1 + 100)
                {
                    points.Add(new Point(x2 - 130, y1));
                    points.Add(new Point(x2 - 130, y2 + 30));
                    points.Add(new Point(x2, y2 + 30));
                }
                else if (x1 > x2 && y1 < y2 && c1 == "r" && c2 == "l" && x2 > x1 - 100)
                {
                    points.Add(new Point(x1, y1 - 30));
                    points.Add(new Point(x1 - 130, y1 - 30));
                    points.Add(new Point(x1 - 130, y2));
                }
                else if (x1 < x2 && y1 > y2 && c1 == "l" && c2 == "r" && x2 < x1 + 100)
                {
                    points.Add(new Point(x2 - 130, y1));
                    points.Add(new Point(x2 - 130, y2 - 30));
                    points.Add(new Point(x2, y2 - 30));
                }
                else if (x1 < x2 && y1 < y2 && c1 == "l" && c2 == "r")
                {
                    points.Add(new Point(x1, y2 + 30));
                    points.Add(new Point(x2, y2 + 30));
                }
                else if (x1 > x2 && y1 > y2 && c1 == "r" && c2 == "l")
                {
                    points.Add(new Point(x1, y1 + 30));
                    points.Add(new Point(x2, y1 + 30));
                }
                else if (x1 > x2 && y1 < y2 && c1 == "r" && c2 == "l")
                {
                    points.Add(new Point(x1, y1 - 30));
                    points.Add(new Point(x2, y1 - 30));
                }
                else if (x1 < x2 && y1 > y2 && c1 == "l" && c2 == "r")
                {
                    points.Add(new Point(x1, y2 - 30));
                    points.Add(new Point(x2, y2 - 30));
                }
                else if (y2 < y1 + 100 && y2 > y1 - 100 && c1 == c2 && (x1 > x2 + 100 || x1 < x2 - 100))
                {
                    points.Add(new Point(x1, Math.Max(y1, y2) + 30));
                    points.Add(new Point(x2, Math.Max(y1, y2) + 30));
                }
                else if (x1 < x2 && y1 < y2 && c1 == "r" && c2 == "r")
                {
                    points.Add(new Point(x2, y1));
                }
                else if (x1 > x2 && y1 > y2 && c1 == "r" && c2 == "r")
                {
                    points.Add(new Point(x1, y2));
                }
                else if (x1 > x2 && y1 < y2 && c1 == "r" && c2 == "r")
                {
                    points.Add(new Point(x1, y2));
                }
                else if (x1 < x2 && y1 > y2 && c1 == "r" && c2 == "r")
                {
                    points.Add(new Point(x2, y1));
                }
                else if ((x1 < x2 && y1 < y2) || (x1 > x2 && y1 > y2))
                {
                    x = System.Math.Min(x1, x2);
                    y = System.Math.Max(y1, y2);
                    points.Add(new Point(x, y));
                }
                else
                {
                    x = System.Math.Min(x1, x2);
                    y = System.Math.Min(y1, y2);
                    points.Add(new Point(x, y));
                }
                points.Add(new Point(x2, y2));
            }
            else
            {
                if (x1 == x2)
                {
                    points.Add(new Point(x1, y1));
                    points.Add(new Point(x2, y2));
                }
                else if(y1 == y2)
                {
                    if (x1 > x2 && c1 == "r" || (x1 < x2 && c1 == "l"))
                    {
                        points.Add(new Point(x1, y1));
                        points.Add(new Point(x1, y1 + 30));
                        points.Add(new Point(x2, y2 + 30));
                        points.Add(new Point(x2, y2));
                        ellipse = new Ellipse() { Margin = new Thickness(x2 - 6 * scale, y2 + 24 * scale, 0, 0), Width = 12 * scale, Height = 12 * scale, Fill = Brushes.Black, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
                        if (v1 || v2)
                            ellipse.Margin = new Thickness(ellipse.Margin.Top, ellipse.Margin.Left, 0, 0);
                        ellipses.Add(ellipse); // uzel
                        workspace.Children.Add(ellipse);
                        if (v1 || v2)
                        {
                            for (int i = 0; i < points.Count; i++)
                                points[i] = new Point(points[i].Y, points[i].X);
                        }
                        return points;
                    }
                    else
                    {
                        points.Add(new Point(x1, y1));
                        points.Add(new Point(x2, y2));
                    }
                }
                else
                {
                    points.Add(new Point(x1, y1));
                    points.Add(new Point(x1, y2));
                    points.Add(new Point(x2, y2));
                }
                ellipse = new Ellipse() { Margin = new Thickness(x2 - 6 * scale, y2 - 6 * scale, 0, 0), Width = 12 * scale, Height = 12 * scale, Fill = Brushes.Black, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
                if (v1 || v2)
                    ellipse.Margin = new Thickness(ellipse.Margin.Top, ellipse.Margin.Left, 0, 0);
                ellipses.Add(ellipse); // uzel
                workspace.Children.Add(ellipse);
            }
            if(v1 || v2)
            {
                for(int i = 0; i < points.Count; i++)
                    points[i] = new Point(points[i].Y, points[i].X);
            }
            return points;
        }

        public List<Tuple<Point, Point>> ancestors = new List<Tuple<Point, Point>>();


        public void UpdateWires()
        {
            try
            {
                foreach (var el in ellipses)
                    workspace.Children.Remove(el);
                ellipses.Clear();
                foreach (var w in wires)
                {
                    foreach (var el in w.lines)
                        workspace.Children.Remove(el);
                    w.lines.Clear();
                }
                ellipses.Clear();
                for (int w = 0; w < wires.Count; w++)
                {
                    PointCollection points = new PointCollection();
                    Point p, p1, p2;
                    string c1, c2;
                    bool v1 = !elements[indexbyid(wires[w].versits[0].Item1)].horizontal, v2 = !elements[indexbyid(wires[w].versits[1].Item1)].horizontal;
                    if (wires[w].versits[0].Item2 == "l")
                    {
                        c1 = "l";
                        try
                        {
                            p1 = elements[indexbyid(wires[w].versits[0].Item1)]._l_point.TransformToAncestor(workspace).Transform(new Point(0, 0));
                        }
                        catch
                        {
                                p1 = ancestors[indexbyid(wires[w].versits[0].Item1)].Item1;
                        }
                    }
                    else
                    {
                        try
                        {
                            p1 = elements[indexbyid(wires[w].versits[0].Item1)]._r_point.TransformToAncestor(workspace).Transform(new Point(0, 0));
                        }
                        catch
                        {
                            p1 = ancestors[indexbyid(wires[w].versits[0].Item1)].Item2;
                        }
                        c1 = "r";
                    }
                    if (wires[w].versits[1].Item2 == "l")
                    {
                        c2 = "l";
                        try
                        {
                            p2 = elements[indexbyid(wires[w].versits[1].Item1)]._l_point.TransformToAncestor(workspace).Transform(new Point(0, 0));
                        }
                        catch
                        {
                            p2 = ancestors[indexbyid(wires[w].versits[1].Item1)].Item1;
                        }
                    }
                    else
                    {
                        c2 = "r";
                        try
                        {
                            p2 = elements[indexbyid(wires[w].versits[1].Item1)]._r_point.TransformToAncestor(workspace).Transform(new Point(0, 0));
                        }
                        catch
                        {
                            p2 = ancestors[indexbyid(wires[w].versits[1].Item1)].Item2;
                        }
                    }
                    /////////
                    points = Poly(p1.X, p1.Y, p2.X, p2.Y, c1, c2, v1, v2);
                    var newLine = new Polyline() { Points = points, StrokeThickness = 4 * scale, Stroke = Brushes.Black };
                    newLine.MouseLeave += Path_temp_MouseLeave;
                    newLine.MouseDown += Path_temp_MouseDown;
                    newLine.MouseEnter += Path_temp_MouseEnter;
                    wires[w].lines.Add(newLine);
                    workspace.Children.Add(newLine);
                    /////////
                    for (int i = 2; i < wires[w].versits.Count; i++)
                    {
                        int t;
                        if (wires[w].versits[i].Item2 == "l")
                        {
                            c2 = "l";
                            t = 1;
                            try
                            {
                                p = elements[indexbyid(wires[w].versits[i].Item1)]._l_point.TransformToAncestor(workspace).Transform(new Point(0, 0));
                            }
                            catch
                            {
                                    p = ancestors[indexbyid(wires[w].versits[i].Item1)].Item1;
                            }
                        }
                        else
                        {
                            c2 = "r";
                            t = 2;
                            try
                            {
                                p = elements[indexbyid(wires[w].versits[i].Item1)]._r_point.TransformToAncestor(workspace).Transform(new Point(0, 0));
                            }
                            catch
                            {
                                p = ancestors[indexbyid(wires[w].versits[i].Item1)].Item2;
                            }
                            v1 = !elements[indexbyid(wires[w].versits[i].Item1)].horizontal;
                        }

                        NearestPoint(points, p, w, c2, v1);
                    }
                    try
                    {
                        for (int i = 0; i < elements.Count; i++)
                        {
                            Point l = elements[i]._l_point.TransformToAncestor(workspace).Transform(new Point(0, 0));
                            Point r = elements[i]._r_point.TransformToAncestor(workspace).Transform(new Point(0, 0));
                            ancestors.Add(new Tuple<Point, Point>(l, r));
                        }
                        ancestors.Clear();
                        for (int i = 0; i < elements.Count; i++)
                        {
                            Point l = elements[i]._l_point.TransformToAncestor(workspace).Transform(new Point(0, 0));
                            Point r = elements[i]._r_point.TransformToAncestor(workspace).Transform(new Point(0, 0));
                            ancestors.Add(new Tuple<Point, Point>(l, r));
                        }
                    }
                    catch { }
                }
            }
            catch(Exception ex) { //MessageBox.Show(ex.ToString());
            }
        }

        private void workspace_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            if (e.Delta > 0)
                scale = Math.Min(1.3, scale + 0.1);
            else
                scale = Math.Max(0.4, scale - 0.1);
            foreach (var el in elements)
            {
                el.scale.ScaleX = scale;
                el.scale.ScaleY = scale;
            }
            foreach (var l in wires)
                foreach(var el in l.lines)
                    el.StrokeThickness = 4 * scale;
            path_temp.StrokeThickness = 4 * scale;
            UpdateWires();
        }

        void NearestPoint(PointCollection points, Point p, int w, string c2, bool v1)
        {
            var pts = new List<Tuple<double, int, Point, string, bool>>();
            // distance, id_min, point, c1
            for (int i = 1; i < points.Count; i++)
            {
                Point a = points[i], b = points[i - 1];
                if (p.X < a.X && p.X > b.X || p.Y < a.Y && p.Y > b.Y || p.X > a.X && p.X < b.X || p.Y > a.Y && p.Y < b.Y)
                {
                    if (p.X < a.X && p.X > b.X || p.X > a.X && p.X < b.X)
                    {
                        pts.Add(new Tuple<double, int, Point, string, bool>(Math.Abs(b.Y - a.Y), i, new Point(p.X, a.Y), "p", false));
                    }
                    else
                    {
                        pts.Add(new Tuple<double, int, Point, string, bool>(Math.Abs(b.X - a.X), i, new Point(a.X, p.Y), "p", false));
                    }
                }
            }
            for (int i = 0; i < points.Count; i++)
            {
                double r = Math.Sqrt(Math.Pow(points[i].X - p.X, 2) + Math.Pow(points[i].Y - p.Y, 2));
                string c1 = "p";
                bool v2 = false;
                foreach (var el in elements)
                {
                    try
                    {
                        var tf = (el._l_point.TransformToAncestor(workspace).Transform(new Point(0, 0)));
                        var ts = (el._r_point.TransformToAncestor(workspace).Transform(new Point(0, 0)));
                        if (points[i] == tf)
                        {
                            c1 = "l";
                            v2 = !el.horizontal;
                            break;
                        }
                        else if (ts == points[i])
                        {
                            c1 = "r";
                            v2 = !el.horizontal;
                            break;
                        }
                    }
                    catch
                    {
                        var tf = ancestors[i].Item1;
                        var ts = ancestors[i].Item2;
                        if (points[i] == tf)
                        {
                            c1 = "l";
                            v2 = !el.horizontal;
                            break;
                        }
                        else if (ts == points[i])
                        {
                            c1 = "r";
                            v2 = !el.horizontal;
                            break;
                        }
                    }
                }
                pts.Add(new Tuple<double, int, Point, string, bool>(r, i, points[i], c1, v2));
            }
            pts.Sort();
            var nearest = pts[0];
            var ps = Poly(p.X, p.Y, nearest.Item3.X, nearest.Item3.Y, c2, nearest.Item4, v1, nearest.Item5);
            var newLine = new Polyline() { Points = ps, StrokeThickness = 4 * scale, Stroke = Brushes.Black};
            newLine.MouseLeave += Path_temp_MouseLeave;
            newLine.MouseDown += Path_temp_MouseDown;
            newLine.MouseEnter += Path_temp_MouseEnter;
            wires[w].lines.Add(newLine);
            workspace.Children.Add(newLine);
        }

        private void SaveImage_Click(object sender, RoutedEventArgs e)
        {
            RenderTargetBitmap bmp = new RenderTargetBitmap(1920, 1080, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(this.workspace);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));

            SaveFileDialog d = new SaveFileDialog();
            d.FileName = "Screen.png";
            d.Filter = "Your image | *.png";
            if ((bool)d.ShowDialog())
            {
                using (Stream stm = File.Create(d.FileName))
                    encoder.Save(stm);
            }
        }

        private void screen_MouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void CreateSwitch_Click(object sender, MouseButtonEventArgs e)
        {
            drawerhost.IsBottomDrawerOpen = false;
            Elements.Switch el = new Elements.Switch(); // change the name
            el.ID = (int)s.addElement("switch"); // add el to scheme
            s.setProperty(el.ID, "state", false);
            el.shortname.Text = "S";
            el.scale.ScaleX = scale;
            el.scale.ScaleY = scale;
            el.element_grid.Width = 400;
            el.element_grid.Height = 400;
            int name = 1;
            while (true)
            {
                if (ContainsName(name.ToString()))
                {
                    name++;
                }
                else
                    break;
            }
            el.name.Text = name.ToString();
            el.changecursor += callevent;
            el.showmenu += ShowMenu;
            el.checkMe += El_checkMe;
            el.uncheck += El_uncheck;
            El_checkMe(el.name.Text);
            el.saveline += El_saveline;
            el.notdrawing += El_notdrawing;
            el.type = ElementsTypes.Switch;
            elements.Add(el);
            ShowMenu(el.name.Text);
            el.removeFromWire += removeFromWire;
            Point p = Mouse.GetPosition(workspace);
            el.HorizontalAlignment = HorizontalAlignment.Left;
            el.VerticalAlignment = VerticalAlignment.Top;
            el.Margin = new Thickness(p.X - 50, p.Y - 50, 0, 0);
            el.turnme += El_turnme;
            Panel.SetZIndex(el, 30);
            workspace.Children.Add(el);
            if (sender != null)
                windowchanged(1); // save moment
        }

        public void removeFromWire(int id, string pos)
        {
            try
            {
                s.removePoint(id, pos);
            }
            catch { }
            foreach(Wire w in wires)
            {
                if (w.versits.Contains(new Tuple<int, string>(id, pos)))
                {
                    foreach (var v in w.versits)
                    {
                        elements[indexbyid(v.Item1)].xLeft.Opacity = 0;
                        elements[indexbyid(v.Item1)].xRight.Opacity = 0;
                        elements[indexbyid(v.Item1)].xLeft.IsEnabled = false;
                        elements[indexbyid(v.Item1)].xRight.IsEnabled = false;
                    }
                    if (w.versits.Count > 2)
                        w.versits.Remove(new Tuple<int, string>(id, pos));
                    else
                    {
                        foreach(var el in w.lines)
                            workspace.Children.Remove(el);
                        wires.Remove(w);
                    }
                    UpdateWires();
                    return;
                }    
            }
        }
    }
}
