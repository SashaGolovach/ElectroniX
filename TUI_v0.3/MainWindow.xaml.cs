using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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
using TUI_v0._3.LabElements;

namespace TUI_v0._3
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        enum ProjectType { LabWork, CircuitLab, None};
        public static HomePage hpage = new HomePage();
        public static ElectronicCircuitLab lab;
        public static LabWork work;
        ProjectType project = ProjectType.None;
        List<Tuple<MemoryStream, int, dynamic>> moments = new List<Tuple<MemoryStream, int, dynamic>>();
        int current_moment = -1;
        public MainWindow()
        {
            InitializeComponent();
            hpage.createworksheet += CreateProject;
            hpage.OnClose += OnClose;
            window.Content = hpage;
            hpage.openproject += Hpage_openproject;
        }

        private void Hpage_openproject(string path, Kinds type)
        {
            try
            {
                Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                BinaryFormatter formatter = new BinaryFormatter();
                dynamic p = formatter.Deserialize(stream);
                if (type == Kinds.LabWork)
                {
                    work = LabSerialization.Deserialize(p);
                    work.saveproject += Lab_saveproject;
                    work.changecursor += Lab_changecursor;
                    work.windowchanged += Memorize;
                    work.undo += Undo;
                    work.redo += Redo;
                    work.createCircuit += CreateProject;
                    work.path = path;
                    work.UpdateWires();
                    work.LoadScheme(path.Remove(path.Length - 4, 4) + "_scheme.bin");
                    work.uncheck();
                    window.Content = work;
                    work.timer.Start();
                    work.StartHelp();
                }
                else
                {
                    lab = Serialization.Deserialize(p);
                    lab.saveproject += Lab_saveproject;
                    lab.createCircuit += CreateProject;
                    lab.changecursor += Lab_changecursor;
                    lab.windowchanged += Memorize;
                    lab.undo += Undo;
                    lab.redo += Redo;
                    lab.path = path;
                    try
                    {
                        //lab.UpdateWires();
                        lab.LoadScheme(path.Remove(path.Length - 4, 4) + "_scheme.bin");
                        lab.uncheck();
                    }
                    catch { }
                    window.Content = lab;
                }
                project = type == Kinds.ElectronicCircuit ? ProjectType.CircuitLab : ProjectType.LabWork;
            }
            catch (Exception ex)
            {
            }
        }

        void CreateProject(string name, Kinds type)
        {
            try
            {
                if (type == Kinds.ElectronicCircuit)
                {
                    lab = new ElectronicCircuitLab();
                    lab.saveproject += Lab_saveproject;
                    window.Content = lab;
                    lab.name.Text = name;
                    lab.changecursor += Lab_changecursor;
                    lab.createCircuit += CreateProject;
                    lab.closewindow += OnClose;
                    lab.windowchanged += Memorize;
                    lab.undo += Undo;
                    lab.redo += Redo;
                    project = ProjectType.CircuitLab;
                    Memorize(0); // save moment
                }
                else if (type == Kinds.LabWork)
                {
                    work = new LabWork();
                    work.saveproject += Lab_saveproject;
                    window.Content = work;
                    work.name.Text = name;
                    work.createCircuit += CreateProject;
                    work.changecursor += Lab_changecursor;
                    work.closewindow += OnClose;
                    work.windowchanged += Memorize;
                    work.undo += Undo;
                    work.redo += Redo;
                    project = ProjectType.LabWork;
                    Memorize(0); // save moment
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void Lab_saveproject(dynamic p)
        {
            try
            {
                if (project == ProjectType.LabWork)
                {
                    string path = Directory.GetCurrentDirectory();
                    SaveFileDialog d = new SaveFileDialog();
                    if (work.path == null)
                    {
                        d.InitialDirectory = path;
                        d.FileName = work.name.Text;
                        d.Filter = "Your project | *.bin";
                        if (d.ShowDialog() == true)
                        {
                            if (!string.IsNullOrWhiteSpace(d.FileName))
                            {
                                string save_path = d.FileName;
                                BinaryFormatter formatter = new BinaryFormatter();
                                Stream stream = new FileStream(save_path, FileMode.OpenOrCreate, FileAccess.Write);
                                formatter.Serialize(stream, p);
                                work.path = save_path;
                                work.SaveScheme(save_path);
                                //MessageBox.Show("OK");
                            }
                        }
                    }
                    else
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        Stream stream = new FileStream(work.path, FileMode.OpenOrCreate, FileAccess.Write);
                        formatter.Serialize(stream, p);
                    }
                }
                else
                {
                    string path = Directory.GetCurrentDirectory();
                    SaveFileDialog d = new SaveFileDialog();
                    if (lab.path == null)
                    {
                        d.InitialDirectory = path;
                        d.FileName = lab.name.Text;
                        d.Filter = "Your project | *.bin";
                        if (d.ShowDialog() == true)
                        {
                            if (!string.IsNullOrWhiteSpace(d.FileName))
                            {
                                string save_path = d.FileName;
                                BinaryFormatter formatter = new BinaryFormatter();
                                Stream stream = new FileStream(save_path, FileMode.OpenOrCreate, FileAccess.Write);
                                formatter.Serialize(stream, p);
                                lab.path = save_path;
                                lab.SaveScheme(save_path);
                                //MessageBox.Show("OK");
                            }
                        }
                    }
                    else
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        Stream stream = new FileStream(lab.path, FileMode.OpenOrCreate, FileAccess.Write);
                        formatter.Serialize(stream, p);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public void Undo()
        {
            try
            {
                if(project == ProjectType.LabWork)
                {
                    if (current_moment - 1 >= 0)
                    {
                        LabElements.LabSerializible p = (LabElements.LabSerializible)DeserializeFromStream(moments[current_moment - 1].Item1);
                        work = LabSerialization.Deserialize(p);
                        work.s = moments[current_moment - 1].Item3;
                        work.saveproject += Lab_saveproject;
                        work.changecursor += Lab_changecursor;
                        work.windowchanged += Memorize;
                        work.closewindow += OnClose;
                        work.undo += Undo;
                        work.createCircuit += CreateProject;
                        work.redo += Redo;
                        work.UpdateWires();
                        window.Content = work;
                        work.UpdateWires();
                        work.StartHelp();
                        work.timer.Start();
                        current_moment--;
                        bool f = work.Focus();
                        Keyboard.Focus(work);
                    }
                }
                else
                {
                    if (current_moment - 1 >= 0)
                    {
                        Project_Serializible p = (Project_Serializible)DeserializeFromStream(moments[current_moment - 1].Item1);
                        lab = Serialization.Deserialize(p);
                        lab.s = moments[current_moment - 1].Item3;
                        lab.saveproject += Lab_saveproject;
                        lab.changecursor += Lab_changecursor;
                        lab.windowchanged += Memorize;
                        lab.createCircuit += CreateProject;
                        lab.closewindow += OnClose;
                        lab.undo += Undo;
                        lab.redo += Redo;
                        lab.UpdateWires();
                        window.Content = lab;
                        current_moment--;
                        bool f = lab.Focus();
                        Keyboard.Focus(lab);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public void Redo()
        {
            try
            {
                if (project == ProjectType.LabWork)
                {
                    if (current_moment + 1 < moments.Count)
                    {
                        LabElements.LabSerializible p = (LabElements.LabSerializible)DeserializeFromStream(moments[current_moment + 1].Item1);
                        work = LabSerialization.Deserialize(p);
                        work.s = moments[current_moment + 1].Item3;
                        work.saveproject += Lab_saveproject;
                        work.changecursor += Lab_changecursor;
                        work.windowchanged += Memorize;
                        work.closewindow += OnClose;
                        work.StartHelp();
                        work.timer.Start();
                        work.undo += Undo;
                        work.createCircuit += CreateProject;
                        work.redo += Redo;
                        work.UpdateWires();
                        window.Content = work;
                        current_moment++;
                        bool f = work.Focus();
                        Keyboard.Focus(work);
                    }
                }
                else
                {
                    if (current_moment + 1 < moments.Count)
                    {
                        Project_Serializible p = (Project_Serializible)DeserializeFromStream(moments[current_moment + 1].Item1);
                        lab = Serialization.Deserialize(p);
                        lab.s = moments[current_moment + 1].Item3;
                        lab.saveproject += Lab_saveproject;
                        lab.changecursor += Lab_changecursor;
                        lab.windowchanged += Memorize;
                        lab.closewindow += OnClose;
                        lab.undo += Undo;
                        lab.redo += Redo;
                        lab.UpdateWires();
                        lab.createCircuit += CreateProject;
                        window.Content = lab;
                        lab.UpdateWires();
                        current_moment++;
                        bool f = lab.Focus();
                        Keyboard.Focus(lab);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public void Memorize(int type)
        {
            if (current_moment >= 0)
            {
                moments.RemoveRange(current_moment + 1, moments.Count - current_moment - 1);
            }
            if (project == ProjectType.CircuitLab)
            {
                Project_Serializible p = Serialization.PreSerialize(lab);
                moments.Add(new Tuple<MemoryStream, int, dynamic>(SerializetoMemory(p), type, lab.s.cop()));
                current_moment++;
            }
            else if(project == ProjectType.LabWork)
            {
                var elements = new List<Element>();
                foreach (var el in work.elements)
                    elements.Add(new Element());
                LabElements.LabSerializible p = LabSerialization.PreSerialize(work);
                moments.Add(new Tuple<MemoryStream, int, dynamic>(SerializetoMemory(p), type, work.s.cop()));
                current_moment++;
            }
        }

        public static MemoryStream SerializetoMemory(object o)
        {
            var s = new MemoryStream();
            var formatter = new BinaryFormatter();
            formatter.Serialize(s, o);
            return s;
        }

        public static object DeserializeFromStream(MemoryStream stream)
        {
            var formatter = new BinaryFormatter();
            stream.Seek(0, SeekOrigin.Begin);
            object o = formatter.Deserialize(stream);
            return o;
        }

        private void Lab_changecursor()
        {
            if (project == ProjectType.CircuitLab)
            {
                this.Cursor = lab.cursor;
            }
            else
            {
                if (this.Cursor == Cursors.Hand) this.Cursor = Cursors.Arrow;
                else this.Cursor = Cursors.Hand;
            }
        }

        public void OnClose()
        {
            this.Close();
        }

        private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                System.Diagnostics.Process[] etc = System.Diagnostics.Process.GetProcesses();
                foreach (System.Diagnostics.Process anti in etc)
                {
                    if (anti.ProcessName.ToLower().Contains("tui"))
                    {
                        anti.Kill();
                    }
                }
            }
            catch { }
            ///////////////////////////
        }
    }
}
