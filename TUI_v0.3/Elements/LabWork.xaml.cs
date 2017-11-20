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

namespace TUI_v0._3
{
    /// <summary>
    /// Логика взаимодействия для ElectronicCircuitLab.xaml
    /// </summary>
    [Serializable]
    public class Ischecked
    {
        public bool ischecked = false;
    }

    public partial class LabWork : UserControl
    {
        public Action<string, Kinds> createCircuit;
        public string TL;
        public int count;
        public bool canAdd;
        public List<string> selected = new List<string>();
        public string content = "Хід роботи", goal = "Мета", header = "Тема", imagePath;
        public double scale = 1;
        public List<dynamic> elements = new List<dynamic>();
        public List<Tuple<Polyline, int, int, string, string, Ischecked, int>> lines = new List<Tuple<Polyline, int, int, string, string, Ischecked, int>>();
        // line, id1, id2, c1, c2, ischecked, id(line)
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
        public LabList aboutLab = new LabList();
        public Line path_temp = new Line() { StrokeThickness = 4, Stroke = Brushes.DarkBlue };
        ScriptEngine engine = Python.CreateEngine(); // engine for python
        ScriptScope scope; // scope
        public dynamic s;
        bool isTesting = false;
        Rectangle rec = new Rectangle();
        public bool showgrid { get; set; } = false;
        public string path;
        public Point? lastDragPoint;
        public System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        public LabWork()
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
            engine.ExecuteFile(path + @"\scheme_lab.py", scope);
            s = scope.GetVariable("s");
            Grid.SetRow(aboutLab, 3);
            Grid.SetRowSpan(aboutLab, 100);
            timer.Interval = 1000;
            timer.Tick += Timer_Tick;
            canAdd = true;
            count = 100;
            //}
            //catch(Exception ex)
            //{
            //    MessageBox.Show(ex.ToString());
            //}
        }


        public void StartHelp()
        {
            using (StreamReader fs = new StreamReader(System.IO.Directory.GetCurrentDirectory() + "//web//lab.html"))
            {
                string code = fs.ReadToEnd();
                try
                {
                    while (code.IndexOf("Тема") != -1)
                    {
                        int index = code.IndexOf("Тема");
                        code = code.Remove(index, 4).Insert(index, header);
                    }
                }
                catch { }

                try
                {
                    while (code.IndexOf("Мета") != -1)
                    {
                        int index = code.IndexOf("Мета");
                        code = code.Remove(index, 4).Insert(index, goal);
                    }
                }
                catch { }
                try
                {
                    while (code.IndexOf("Хід роботи") != -1)
                    {
                        int index = code.IndexOf("Хід роботи");
                        code = code.Remove(index, 10).Insert(index, content);
                    }
                }
                catch { }
                using (StreamWriter writer = new StreamWriter(System.IO.Directory.GetCurrentDirectory() + "//web//lab_temp.html"))
                    writer.Write(code);
            }
            aboutLab.wbrowser.URL = System.IO.Directory.GetCurrentDirectory() + "//web//lab_temp.html";
        }

        public long sec = 0, hours = 0, min = 0;

        private void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (sec > 0 || min > 0 || hours > 0)
                {
                    if (sec != 0)
                        sec--;
                    else if (min != 0)
                    {
                        min--;
                        sec = 59;
                    }
                    else if (hours != 0)
                    {
                        hours--;
                        min = 59;
                    }
                    tl_box.Text = hours + " : " + min + " : " + sec;
                }
                else
                {
                    MessageBox.Show("You lose");
                }
            }
            catch { }
        }

        private void Path_temp_MouseLeave(object sender, MouseEventArgs e)
        {
            var temp = (Polyline)sender;
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Item1 == sender)
                    if (!lines[i].Item6.ischecked)
                    {
                        ((Polyline)sender).StrokeThickness = 4 * scale;
                    }
            }
            changecursor();
        }

        private void Path_temp_MouseEnter(object sender, MouseEventArgs e)
        {
            ((Polyline)sender).StrokeThickness = 6 * scale;
            changecursor();
        }

        private void Path_temp_MouseDown(object sender, MouseButtonEventArgs e)
        {
            uncheck();
            Polyline temp = (Polyline)sender;
            ((Polyline)sender).StrokeThickness = 6 * scale;
            for (int i = 0; i < lines.Count; i++)
                if (lines[i].Item1 == temp)
                    lines[i].Item6.ischecked = true;
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
                        double acc = elements[i].type == ElementsTypes.Multimeter ? 150 * scale : 50 * scale;
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
                        Point relativePoint = elements[i]._l.TransformToAncestor(workspace).Transform(new Point(0, 0));
                        double l_top = relativePoint.Y, l_left = relativePoint.X;
                        relativePoint = elements[i]._r.TransformToAncestor(workspace).Transform(new Point(0, 0));
                        double r_top = relativePoint.Y, r_left = relativePoint.X;
                        if (elements[i]._l_checked)
                        {
                            path_temp.X1 = l_left;
                            path_temp.Y1 = l_top;
                            path_temp.X2 = p.X;
                            path_temp.Y2 = p.Y;
                            break;
                        }
                        else if (elements[i]._r_checked)
                        {
                            path_temp.X1 = r_left;
                            path_temp.Y1 = r_top;
                            path_temp.X2 = p.X;
                            path_temp.Y2 = p.Y;
                            break;
                        }
                    }
                }
            }
        }

        private void ElectronicCircuitLab_MouseUp(object sender, MouseButtonEventArgs e)
        {
            UpdateWires();
            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].IsChecked)
                {
                    mouse_up = false;
                    MouseUp -= ElectronicCircuitLab_MouseUp;
                    for (int j = 0; j < lines.Count; j++)
                    {
                        if (elements[i].ID == lines[j].Item2 || elements[i].ID == lines[j].Item3)
                        {
                            var updated = Poly(lines[j].Item2, lines[j].Item3, lines[j].Item4, lines[j].Item5);
                            lines[j].Item1.Points = updated.Points;
                        }
                    }
                    windowchanged(0); // save moment
                    break;
                }
            }
        }

        private void AddComponentClick(object sender, RoutedEventArgs e)
        {
            drawerhost.IsBottomDrawerOpen = true;
        }

        private void CloseAddComponentPanel(object sender, RoutedEventArgs e)
        {
            drawerhost.IsBottomDrawerOpen = false;
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            changecursor();
        }

        public void AddElement(ElementsTypes type)
        {
            if (!canAdd)
                throw new Exception();
            else if (elements.Count > count)
                throw new Exception();
            else
            {
                //if (!selected.Contains(type.ToString()))
                    //throw new Exception();
            }
        }
        public void CreateResistor_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender != null)
                    AddElement(ElementsTypes.Resistor);
                drawerhost.IsBottomDrawerOpen = false;
                Lab_Resistor el = new Lab_Resistor(""); // change the name
                el.ID = (int)s.addElement("resistor"); // add el to scheme
                el.Height = 100;
                el.Width = 100;
                el.scale.ScaleX = scale;
                el.scale.ScaleY = scale;
                el.name = el.ID.ToString();
                el.changecursor += callevent;
                el.showmenu += ShowMenu;
                el.checkMe += El_checkMe;
                el.uncheck += El_uncheck;
                El_checkMe(el.name);
                el.saveline += El_saveline;
                el.notdrawing += El_notdrawing;
                el.type = ElementsTypes.Resistor;
                elements.Add(el);
                ShowMenu(el.name);
                Point p = Mouse.GetPosition(workspace);
                if (sender == null)
                    p = new Point(1000, 1000);
                el.HorizontalAlignment = HorizontalAlignment.Left;
                el.VerticalAlignment = VerticalAlignment.Top;
                el.Margin = new Thickness(p.X - 50, p.Y - 50, 0, 0);
                el.turnme += El_turnme;
                Panel.SetZIndex(el, 30);
                workspace.Children.Add(el);
                if (sender != null)
                    windowchanged(1); // save moment
            }
            catch { }
        }

        private bool El_notdrawing1(LabElement el)
        {
            throw new NotImplementedException();
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
                    for (int j = 0; j < lines.Count; j++)
                    {
                        if (elements[i].ID == lines[j].Item2 || elements[i].ID == lines[j].Item3)
                        {
                            var updated = Poly(lines[j].Item2, lines[j].Item3, lines[j].Item4, lines[j].Item5);
                            lines[j].Item1.Points = updated.Points;
                        }
                    }
                    break;
                }
            }
        }

        private bool El_notdrawing(dynamic el)
        {
            int v = -1;
            bool rez = false;
            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].type == el.type && elements[i] == el) v = i;
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
                bool contains = false;
                var temp = new Tuple<int, int, string, string>(object1, object2, cf, cs);
                foreach (var item in elements[first].connections)
                    if (item.Item1 == temp.Item1 && item.Item2 == temp.Item2 && item.Item3 == temp.Item3 && item.Item4 == temp.Item4
                        || item.Item2 == temp.Item1 && item.Item1 == temp.Item2 && item.Item4 == temp.Item3 && item.Item3 == temp.Item4)
                        contains = true;
                if (!contains)
                {
                    int ID = (int)s.connectElement(object1, cf, object2, cs); // connect with scheme
                    elements[first].connections.Add(new Tuple<int, int, string, string, int>(object1, object2, cf, cs, ID));
                    elements[second].connections.Add(new Tuple<int, int, string, string, int>(object1, object2, cf, cs, ID));
                    // test
                    var l = Poly(object1, object2, cf, cs);
                    l.MouseLeave += Path_temp_MouseLeave;
                    l.MouseDown += Path_temp_MouseDown;
                    l.MouseEnter += Path_temp_MouseEnter;
                    lines.Add(new Tuple<Polyline, int, int, string, string, Ischecked, int>(l, object1, object2, cf, cs, new Ischecked(), ID));
                    workspace.Children.Add(lines.Last().Item1);
                    workspace.Children.Remove(path_temp);
                    path_temp = new Line() { StrokeThickness = 4 * scale, Stroke = Brushes.DarkBlue };
                    workspace.Children.Add(path_temp);
                    windowchanged(1); // save moment
                }
                else
                {
                    MessageBox.Show("You cannot do this"); // change me
                    workspace.Children.Remove(path_temp);
                    path_temp = new Line() { StrokeThickness = 4 * scale, Stroke = Brushes.DarkBlue };
                    workspace.Children.Add(path_temp);
                }
                elements[first]._l_checked = false; elements[first]._r_checked = false;
                elements[second]._l_checked = false; elements[second]._r_checked = false;
                El_uncheck();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void El_checkMe(string a)
        {
            for (int i = 0; i < elements.Count; i++)
                elements[i].IsChecked = elements[i].name == a;
            for (int i = 0; i < lines.Count; i++)
            {
                //lines[i].Item6 = false;
                lines[i].Item1.StrokeThickness = 4 * scale;
            }
        }

        void callevent() { changecursor(); }

        public void uncheck()
        {
            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].IsChecked) UpdateProperties(i);
                elements[i].IsChecked = false;
                elements[i].isdrawing = false;
                elements[i]._r_checked = false;
                elements[i]._l_checked = false;
            }
            dhost.IsOpen = false;
            for (int i = 0; i < lines.Count; i++)
            {
                //lines[i].Item6.ischecked = false;
                (lines[i].Item1).StrokeThickness = 4 * scale;
            }
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
                        s.setProperty(elements[i].ID, "R", double.Parse(resistance_box.Text)); // property to scheme
                    }
                    catch { }
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
                        catch { }
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
                if (elements[i].name == text) return true;
            return false;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                for (int i = 0; i < elements.Count; i++)
                    if (elements[i].IsChecked)
                    {
                        foreach (var c in elements[i].connections)
                        {
                            if (c.Item2 == elements[i].ID)
                                elements[indexbyid(c.Item1)].connections.Remove(c);
                            else
                                elements[indexbyid(c.Item2)].connections.Remove(c);
                            workspace.Children.Remove(lines[indexbyid(c.Item5)].Item1);
                            lines.Remove(lines[indexbyid(c.Item5)]);
                        }
                        workspace.Children.Remove((UIElement)elements[i]);
                        workspace.Children.Remove(elements[i]._l);
                        workspace.Children.Remove(elements[i]._r);
                        s.removeElement(elements[i].ID);
                        elements.RemoveAt(i);
                        windowchanged(1); // save moment
                        break;
                    }
                dhost.IsOpen = false;
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].Item6.ischecked)
                    {
                        workspace.Children.Remove(lines[i].Item1);
                        elements[indexbyid(lines[i].Item2)].connections.Remove(new Tuple<int, int, string, string, int>(lines[i].Item2, lines[i].Item3, lines[i].Item4, lines[i].Item5, lines[i].Item7));
                        elements[indexbyid(lines[i].Item3)].connections.Remove(new Tuple<int, int, string, string, int>(lines[i].Item2, lines[i].Item3, lines[i].Item4, lines[i].Item5, lines[i].Item7));
                        s.removeElement(lines[i].Item7);
                        lines.RemoveAt(i);
                        windowchanged(1); // save moment
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }

        int indexbyid(int id)
        {
            for (int i = 0; i < elements.Count; i++)
                if (elements[i].ID == id) return i;
            for (int i = 0; i < lines.Count; i++)
                if (lines[i].Item7 == id) return i;
            return -1;
        }

        private void TurnButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (dynamic el in elements)
                if (el.IsChecked)
                {
                    el.TurnLeft();
                    ExtensionMethods.Refresh(el);
                    El_turnme(el.ID);
                    windowchanged(0); // save moment
                    break;
                }
        }

        void ShowMenu(string name)
        {
            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].name == name)
                {
                    NameTextBox.Text = elements[i].name;
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
            BitmapImage bmp = new BitmapImage(new Uri("Icons\\resistor.png", UriKind.Relative));
            ImageBrush br = new ImageBrush();
            br.ImageSource = bmp;
            br.Stretch = Stretch.Fill;
            resistor_card.Fill = br; // backbround of a card
            bmp = new BitmapImage(new Uri("Icons\\eds.png", UriKind.Relative));
            br = new ImageBrush();
            br.ImageSource = bmp;
            capacitor_card.Fill = br;
            bmp = new BitmapImage(new Uri("Icons\\lamp.png", UriKind.Relative));
            br = new ImageBrush();
            br.ImageSource = bmp;
            Lamp.Fill = br;
            bmp = new BitmapImage(new Uri("Icons\\multimeter.png", UriKind.Relative));
            br = new ImageBrush();
            br.ImageSource = bmp;
            br.Stretch = Stretch.Uniform;
            Multimeter.Fill = br;

        }

        private void CreateLabWorkClick(object sender, RoutedEventArgs e)
        {
            createCircuit("Experiment", Kinds.LabWork);
        }

        private void StartButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                dhost.IsOpen = false;
                foreach (var el in elements)
                    if (el.IsChecked)
                    {
                        UpdateProperties(elements.IndexOf(el));
                        break;
                    }
                foreach (var el in elements)
                    if (el.type == ElementsTypes.Multimeter)
                    {
                        s.setProperty(el.ID, "R", el.resistance);
                        //if (el.dimension == MultimeterDimension.Voltmeter)
                        //    s.setProperty(el.ID, "R", 10000000000);
                        //else if (el.dimension == MultimeterDimension.Ammeter)
                        //    s.setProperty(el.ID, "R", 0.0001);
                    }
                else if(el.type == ElementsTypes.Lamp)
                        s.setProperty(el.ID, "critP", el.power);
                ObservableCollection<Data> items = new ObservableCollection<Data>();
                if (!isTesting)
                {
                    isTesting = true;
                    s.updateI(); // calculate scheme
                    for (int i = 0; i < elements.Count; i++)
                    {           
                        if (elements[i].type == ElementsTypes.Multimeter)
                        {
                            elements[i].iButton.IsEnabled = false;
                            elements[i].uButton.IsEnabled = false;
                            dynamic result = s.ids[elements[i].ID];
                            if (elements[i].dimension == MultimeterDimension.Ammeter)
                                elements[i].result.Text = ((object)result.I).ToString() + " Am";
                            else if (elements[i].dimension == MultimeterDimension.Voltmeter)
                                elements[i].result.Text = ((object)result.U).ToString() + " V";
                        }
                        else if (elements[i].type == ElementsTypes.Lamp)
                        {
                            dynamic result = s.ids[elements[i].ID];
                            if (result.I != 0)
                                elements[i].ChangeTheme("1");
                            else if (result.state == false)
                            {
                                elements[i].background = new BitmapImage(new Uri("Icons\\lamp_exp.png", UriKind.Relative));
                                elements[i].br.ImageSource = elements[i].background;
                                elements[i].rec.Fill = elements[i].br;
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < elements.Count; i++)
                        if (elements[i].type == ElementsTypes.Multimeter)
                        {
                            elements[i].iButton.IsEnabled = true;
                            elements[i].uButton.IsEnabled = true;
                        }
                        else if (elements[i].type == ElementsTypes.Lamp)
                        {
                            elements[i].ChangeTheme("");
                            elements[i].background = new BitmapImage(new Uri("Icons\\lamp.png", UriKind.Relative));
                            elements[i].br.ImageSource = elements[i].background;
                            elements[i].rec.Fill = elements[i].br;
                        }
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
            showgrid = !showgrid;
            MaterialDesignThemes.Wpf.PackIcon icon = new MaterialDesignThemes.Wpf.PackIcon();
            if (showgrid)
                icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.GridOff;
            else
                icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Grid;
            ShowGridButton.Content = icon;
            work_grid.Opacity = showgrid ? 1 : 0;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.E))
                closewindow();
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.Z))
                undo();
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.Y))
                redo();
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.S))
            {
                var p = (LabSerialization.PreSerialize(MainWindow.work));
                p.hours = 0; sec = 0; min = 0;
                saveproject(p);
            }
            switch (e.Key)
            {
                case Key.Delete:
                    DeleteButton_Click(null, null);
                    break;
                case Key.G:
                    Show_Grid_Click(null, null);
                    break;
            }
        }

        private void CreateCurcuitClick(object sender, RoutedEventArgs e)
        {
            createCircuit("Circuit", Kinds.ElectronicCircuit);
        }

        public void CreateCapacitor_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender != null)
                    AddElement(ElementsTypes.EDS);
                drawerhost.IsBottomDrawerOpen = false;
                Lab_Battery el = new Lab_Battery(""); // change the name
                el.ID = (int)s.addElement("rEMF"); // add el to scheme
                el.Height = 100;
                el.Width = 100;
                el.scale.ScaleX = scale;
                el.scale.ScaleY = scale;
                el.name = el.ID.ToString();
                el.changecursor += callevent;
                el.showmenu += ShowMenu;
                el.checkMe += El_checkMe;
                el.uncheck += El_uncheck;
                El_checkMe(el.name);
                el.saveline += El_saveline;
                el.notdrawing += El_notdrawing;
                el.type = ElementsTypes.EDS;
                elements.Add(el);
                ShowMenu(el.name);
                Point p = Mouse.GetPosition(workspace);
                el.HorizontalAlignment = HorizontalAlignment.Left;
                el.VerticalAlignment = VerticalAlignment.Top;
                el.Margin = new Thickness(p.X - 50, p.Y - 50, 0, 0);
                el.turnme += El_turnme;
                Panel.SetZIndex(el, 30);
                workspace.Children.Add(el);
            }
            catch { }
        }

        private void RedoButtonClick(object sender, RoutedEventArgs e)
        {
            redo();
        }

        private void UndoButtonClick(object sender, RoutedEventArgs e)
        {
            undo();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            closewindow();
        }

        private void SaveAsButton_Click(object sender, RoutedEventArgs e)
        {
            saveproject(LabSerialization.PreSerialize(MainWindow.work));
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
            s.load(save_path);
        }

        public void CreateLamp_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender != null)
                    AddElement(ElementsTypes.Lamp);
                drawerhost.IsBottomDrawerOpen = false;
                Lab_Lamp el = new Lab_Lamp("L"); // change the name
                el.Height = 200;
                el.Width = 200;
                el.scale.ScaleX = scale;
                el.scale.ScaleY = scale;
                el.ID = (int)s.addElement("lamp"); // add el to scheme
                s.setProperty(el.ID, "critP", 30);
                el.name = el.ID.ToString();
                el.changecursor += callevent;
                el.showmenu += ShowMenu;
                el.checkMe += El_checkMe;
                el.uncheck += El_uncheck;
                El_checkMe(el.name);
                el.saveline += El_saveline;
                el.notdrawing += El_notdrawing;
                el.type = ElementsTypes.Lamp;
                elements.Add(el);
                ShowMenu(el.name);
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
            catch { }
        }

        private void BackToHome_Click(object sender, RoutedEventArgs e)
        {
            window.Content = MainWindow.hpage;
        }

        public void Multimeter_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if(sender != null)
                AddElement(ElementsTypes.Multimeter);
                drawerhost.IsBottomDrawerOpen = false;
                Multimeter el = new Multimeter(""); // change the name
                el.Height = 300;
                el.Width = 300;
                el.scale.ScaleX = scale - 0.35;
                el.scale.ScaleY = scale - 0.35;
                el.ID = (int)s.addElement("ammeter"); // add el to scheme
                el.name = el.ID.ToString();
                el.changecursor += callevent;
                el.showmenu += ShowMenu;
                el.checkMe += El_checkMe;
                el.uncheck += El_uncheck;
                El_checkMe(el.name);
                el.saveline += El_saveline;
                el.notdrawing += El_notdrawing;
                el.type = ElementsTypes.Multimeter;
                elements.Add(el);
                ShowMenu(el.name);
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
            catch { }
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
                if (el.type == ElementsTypes.Multimeter)
                {
                    el.scale.ScaleX = scale * (1 - 0.35);
                    el.scale.ScaleY = scale * (1 - 0.35);
                }
                else
                {
                    el.scale.ScaleX = scale;
                    el.scale.ScaleY = scale;
                }

            }
            foreach (var l in lines)
                if (!l.Item6.ischecked)
                    l.Item1.StrokeThickness = 4 * scale;
                else
                    l.Item1.StrokeThickness = 6 * scale;
            path_temp.StrokeThickness = 4 * scale;
            UpdateWires();
        }

        private void AboutClick(object sender, RoutedEventArgs e)
        {
            try
            {
                main_grid.Children.Add(aboutLab);
                back_button.Opacity = 100;
                uncheck();
                dhost.IsOpen = false;
            }
            catch { }
        }

        private void BackClick(object sender, RoutedEventArgs e)
        {
            try
            {
                main_grid.Children.Remove(aboutLab);
                back_button.Opacity = 0;
            }
            catch { }
        }

        public List<Tuple<Point, Point>> ancestors = new List<Tuple<Point, Point>>();

        Polyline Poly(int a, int b, string c1, string c2)
        {
            #region GetCoordinates
            Polyline l = new Polyline();
            PointCollection points = new PointCollection();
            int x1, x2, y1, y2, _a = indexbyid(a), _b = indexbyid(b);
            Point relativePoint;
            try
            {
                relativePoint = elements[_a]._l_point.TransformToAncestor(workspace).Transform(new Point(0, 0));
            }
            catch
            {
                relativePoint = ancestors[_a].Item1;
            }
            double l_top = relativePoint.Y, l_left = relativePoint.X;
            try
            {
                relativePoint = elements[_a]._r_point.TransformToAncestor(workspace).Transform(new Point(0, 0));
            }
            catch
            {
                relativePoint = ancestors[_a].Item2;
            }
            double r_top = relativePoint.Y, r_left = relativePoint.X;
            if (c1 == "l")
            {
                x1 = (int)l_left;
                y1 = (int)l_top;
            }
            else
            {
                x1 = (int)r_left;
                y1 = (int)r_top;
            }
            points.Add(new Point(x1, y1)); // first point
            try
            {
                relativePoint = elements[_b]._l_point.TransformToAncestor(workspace).Transform(new Point(0, 0));
            }
            catch
            {
                relativePoint = ancestors[_b].Item1;
            }
            l_top = relativePoint.Y; l_left = relativePoint.X;
            try
            {
                relativePoint = elements[_b]._r_point.TransformToAncestor(workspace).Transform(new Point(0, 0));
            }
            catch
            {
                relativePoint = ancestors[_b].Item2;
            }
            r_top = relativePoint.Y; r_left = relativePoint.X;
            if (c2 == "l")
            {
                x2 = (int)l_left;
                y2 = (int)l_top;
            }
            else
            {
                x2 = (int)r_left;
                y2 = (int)r_top;
            }
            #endregion
            points.Add(new Point(x1, y1));
            points.Add(new Point(x2, y2));
            l.Points = points;
            l.Stroke = Brushes.DarkOliveGreen; l.StrokeThickness = 4 * scale;
            return l;
        }

        public void UpdateWires()
        {
            try
            {
                for (int j = 0; j < lines.Count; j++)
                {
                    var updated = Poly(lines[j].Item2, lines[j].Item3, lines[j].Item4, lines[j].Item5);
                    lines[j].Item1.Points = updated.Points;
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
}
