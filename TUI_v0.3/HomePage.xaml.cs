using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Логика взаимодействия для HomePage.xaml
    /// </summary>
    public partial class HomePage : UserControl
    {
        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        public delegate void CreateWorkSheet(string name, Kinds type);
        public event CreateWorkSheet createworksheet;
        public delegate void CloseApplication();
        public event CloseApplication OnClose;
        public delegate void OpenFromFile(string path, Kinds type);
        public event OpenFromFile openproject;
        AboutPage p = new AboutPage();
        LabList list = new LabList();
        public HomePage()
        {
            InitializeComponent();
            BitmapImage bmp = new BitmapImage(new Uri("CircuitIcons\\list.png", UriKind.Relative));
            ImageBrush br = new ImageBrush();
            br.ImageSource = bmp;
            FileButton.Background = br;
            bmp = new BitmapImage(new Uri("CircuitIcons\\CircuitLogo.jpg", UriKind.Relative));
            br = new ImageBrush();
            br.ImageSource = bmp;
            CircuitImage.Fill = br;
            bmp = new BitmapImage(new Uri("CircuitIcons\\LabWorkLogo.jpg", UriKind.Relative));
            br = new ImageBrush();
            br.ImageSource = bmp;
            LabWorkImage.Fill = br;
            /////
            Grid.SetRow(p, 1);
            Grid.SetRowSpan(p, 10);
            Grid.SetRow(list, 1);
            Grid.SetRowSpan(list, 10);
            p.wbrowser.URL = System.IO.Directory.GetCurrentDirectory() + @"\AboutPage\index.html";
            list.wbrowser.URL = System.IO.Directory.GetCurrentDirectory() + @"\web\events.html";
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                if(main_grid.Children.Contains(list))
                    {
                    string dir = System.IO.Directory.GetCurrentDirectory() + "//Labs//";
                    if (list.wbrowser.URL.Contains("zakon_oma"))
                    {
                        openproject(dir + "zakon_oma.bin", Kinds.LabWork);
                        BackClick(null, null);
                    }
                    else if (list.wbrowser.URL.Contains("lamp"))
                    {
                        openproject(dir + "zakon_oma.bin", Kinds.LabWork);
                        BackClick(null, null);
                    }
                    else if (list.wbrowser.URL.Contains("poslidovne"))
                    {
                        openproject(dir + "poslidovne.bin", Kinds.LabWork);
                        BackClick(null, null);
                    }
                    else if (list.wbrowser.URL.Contains("parallel"))
                    {
                        openproject(dir + "parallel.bin", Kinds.LabWork);
                        BackClick(null, null);
                    }
                    else if (list.wbrowser.URL.Contains("shunt"))
                    {
                        openproject(dir + "shunt.bin", Kinds.LabWork);
                        BackClick(null, null);
                    }
                    else if (list.wbrowser.URL.Contains("voltmeter"))
                    {
                        openproject(dir + "voltmeter.bin", Kinds.LabWork);
                        BackClick(null, null);
                    }
                }
            }
            catch { }
        }

        private void Exit_Click(object sender, RoutedEventArgs e) { OnClose(); }

        private void CreateCurcuitClick(object sender, RoutedEventArgs e)
        {
            createworksheet("ElectronicCircuitProject", Kinds.ElectronicCircuit);
        }

        private void OpenProjectButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Your project | *.bin";
            if (openFileDialog.ShowDialog() == true)
                openproject(openFileDialog.FileName, Kinds.ElectronicCircuit);
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            main_grid.Children.Add(p);
            back_button.Opacity = 100;
        }

        private void CreateLabWorkClick(object sender, RoutedEventArgs e)
        {

            main_grid.Children.Add(list);
            list.wbrowser.URL = System.IO.Directory.GetCurrentDirectory() + @"\web\events.html";
            back_button.Opacity = 100;
            //createworksheet("LabWorkProject", Kinds.LabWork);
        }

        void Create(object sender, RoutedEventArgs e)
        {
            createworksheet("LabWork", Kinds.LabWork);
        }

        private void BackClick(object sender, RoutedEventArgs e)
        {
            if(main_grid.Children.Contains(p))
                main_grid.Children.Remove(p);
            if (main_grid.Children.Contains(list))
                main_grid.Children.Remove(list);
            back_button.Opacity = 0;
        }

        private void PhotoClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Для викорсистання даного режиму переконайтесь у наявності Anaconda, Tensorflow, Keras." +
                "У разі відсутності встановіть відповідні бібліотеки з кореневої папки проекту." +
                System.IO.Directory.GetCurrentDirectory() + "\\Installers");
            string s = System.IO.Directory.GetCurrentDirectory() + @"/Cut/start.bat";
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = s;
            info.UseShellExecute = false;
            info.WorkingDirectory = s.Remove(s.Length - 9, 9);
            Process p = new Process();
            p.StartInfo = info;
            p.Start();
        }

        private void OpenScheme(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Для викорсистання даного режиму переконайтесь у наявності Anaconda, Tensorflow, Keras." +
                "У разі відсутності встановіть відповідні бібліотеки з кореневої папки проекту." +
                System.IO.Directory.GetCurrentDirectory() + "\\Installers");
            string s = System.IO.Directory.GetCurrentDirectory() + @"/Schemes/start.bat";
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = s;
            info.UseShellExecute = false;
            info.WorkingDirectory = s.Remove(s.Length - 9, 9);
            Process p = new Process();
            p.StartInfo = info;
            p.Start();
        }

        private void OpenLabButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Your project | *.bin";
            if (openFileDialog.ShowDialog() == true)
                openproject(openFileDialog.FileName, Kinds.LabWork);
        }
    }
}
