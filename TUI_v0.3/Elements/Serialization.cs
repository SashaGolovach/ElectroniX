using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TUI_v0._3.LabElements;

namespace TUI_v0._3
{
    public class Serialization
    {
        public static Project_Serializible PreSerialize(ElectronicCircuitLab lab)
        {
            Project_Serializible p = new Project_Serializible();
            p.elements = ConvertToElements(lab.elements);
            foreach (Wire w in lab.wires)
                p.wires.Add(w.versits);
            p.path = lab.path;
            p.ancestors = lab.ancestors;
            return p;
        }

        public static List<ElementSerializible> ConvertToElements(List<Element> elements)
        {
            List<ElementSerializible> list = new List<ElementSerializible>();
            foreach (var el in elements)
            {
                ElementSerializible res = new ElementSerializible()
                {
                    ID = el.ID,
                    name = el.name.Text,
                    resistance = el.resistance,
                    l = el.Margin.Left,
                    r = el.Margin.Right,
                    top = el.Margin.Top,
                    bottom = el.Margin.Bottom,
                    isWire = el.isWire,
                    type = el.type,
                    E = el.E,
                    horizontal = el.horizontal
                };
                list.Add(res);
            }
            return list;
        }

        public static ElectronicCircuitLab Deserialize(Project_Serializible lab)
        {
            var p = new ElectronicCircuitLab();
            foreach (var w in lab.wires)
            {
                p.wires.Add(new Wire() { versits = w });
            }
            foreach (var el in lab.elements)
            {
                if (el.type == ElementsTypes.Ammeter)
                {
                    p.CreateAmmeter_Click(null, null);
                }
                else if (el.type == ElementsTypes.Voltmeter)
                {
                    p.CreateVoltmeter_Click(null, null);
                }
                else if (el.type == ElementsTypes.Lamp)
                {
                    p.CreateLamp_Click(null, null);
                }
                else if (el.type == ElementsTypes.Resistor)
                {
                    p.CreateResistor_Click(null, null);
                }
                else if (el.type == ElementsTypes.EDS)
                {
                    p.CreateRCapacitor_Click(null, null);
                }
                p.elements[p.elements.Count - 1].E = el.E;
                p.elements[p.elements.Count - 1].isWire = el.isWire;
                if(el.isWire)
                {
                    p.elements[p.elements.Count - 1].background = new BitmapImage(new Uri("CircuitIcons\\wire.png", UriKind.Relative));
                    p.elements[p.elements.Count - 1].background_v = new BitmapImage(new Uri("CircuitIcons\\wire_v.png", UriKind.Relative));
                    if (p.elements[p.elements.Count - 1].horizontal)
                        p.elements[p.elements.Count - 1].br.ImageSource = p.elements[p.elements.Count - 1].background;
                    else
                        p.elements[p.elements.Count - 1].br.ImageSource = p.elements[p.elements.Count - 1].background_v;
                    p.elements[p.elements.Count - 1].rec.Fill = p.elements[p.elements.Count - 1].br;
                }
                p.elements[p.elements.Count - 1].resistance = el.resistance;
                p.elements[p.elements.Count - 1].ID = el.ID;
                p.elements[p.elements.Count - 1].name.Text = el.name;
                p.elements[p.elements.Count - 1].Margin = new Thickness(el.l, el.top, el.r, el.bottom);
                p.elements[p.elements.Count - 1].type = el.type;
                if (!el.horizontal) p.elements[p.elements.Count - 1].TurnLeft();
            }
            p.ancestors = lab.ancestors;
            p.path = lab.path;
            return p;
        }
    }


    public class LabSerialization
    {
        public static LabSerializible PreSerialize(LabWork lab)
        {
            LabSerializible p = new LabSerializible();
            p.elements = ConvertToLabElements(lab.elements);
            p.countMax = lab.count;
            p.canAdd = (bool)lab.canAdd;
            p.header = lab.header;
            p.hours = lab.hours;
            p.min = lab.min;
            p.sec = lab.sec;
            p.pathImage = lab.imagePath;
            p.goal = lab.goal;
            p.content = lab.content;
            p.ancestors = lab.ancestors;
            
            p.selectedTypes = new List<string>();
            var selected = lab.selected;
            p.TL = lab.TL;
            var lines = new List<Tuple<int, int, string, string, Ischecked, int>>();
            foreach (var l in lab.lines)
            {
                lines.Add(new Tuple<int, int, string, string, Ischecked, int>(l.Item2, l.Item3, l.Item4, l.Item5, l.Item6, l.Item7));
            }
            p.lines = lines;
            p.ancestors = lab.ancestors;
            return p;
        }

        public static List<LabElementSerializible> ConvertToLabElements(List<dynamic> elements)
        {
            List<LabElementSerializible> list = new List<LabElementSerializible>();
            foreach (var el in elements)
            {
                LabElementSerializible res = new LabElementSerializible()
                {
                    ID = el.ID,
                    name = el.name,
                    resistance = el.resistance,
                    l = el.Margin.Left,
                    r = el.Margin.Right,
                    top = el.Margin.Top,
                    bottom = el.Margin.Bottom,
                    type = el.type,
                    connections = el.connections,
                    EMax = el.EMax,
                    E = el.E,
                    resistanceMax = el.resistanceMax
                };
                list.Add(res);
            }
            return list;
        }

        public static LabWork Deserialize(dynamic lab1)
        {
            var p = new LabWork();
            dynamic lab;
            try
            {
                lab = (TeacherMode.Elements.LabSerializible)lab1;
            }
            catch {
                lab = (LabSerializible)lab1;
            }
            p.count = lab.countMax;
            p.header = lab.header;
            p.imagePath = lab.pathImage;
            p.content = lab.content;
            p.goal = lab.goal;
            p.ancestors = lab.ancestors;
            p.canAdd = lab.canAdd;
            foreach (var el in lab.selectedTypes)
            {
                p.selected.Add(el);
            }
            p.TL = lab.TL;
            string[] time = p.TL.Split(':');
            p.sec = long.Parse(time[2]);
            p.hours = long.Parse(time[0]);
            p.min = long.Parse(time[1]);
            try
            {
                p.hours = lab1.hours;
                p.min = lab1.min;
                p.sec = lab1.sec;
            }
            catch { }
            foreach (var el in lab.elements)
            {
                try
                {
                    if (el.type.ToString().Contains("Lamp"))//TeacherMode.KindOfProjectEnum.ElementsTypes.Lamp)
                    {
                        p.CreateLamp_Click(null, null);
                    }
                    else if (el.type.ToString().Contains("Resistor")) //TeacherMode.KindOfProjectEnum.ElementsTypes.Resistor)
                    {
                        p.CreateResistor_Click(null, null);
                    }
                    else if (el.type.ToString().Contains("EDS")) //TeacherMode.KindOfProjectEnum.ElementsTypes.EDS)
                    {
                        p.CreateCapacitor_Click(null, null);
                    }
                    else if (el.type.ToString().Contains("Multimeter")) //TeacherMode.KindOfProjectEnum.ElementsTypes.EDS)
                    {
                        p.Multimeter_Click(null, null);
                    }
                    p.elements[p.elements.Count - 1].E = el.E;
                    p.elements[p.elements.Count - 1].resistance = el.resistance;
                    p.elements[p.elements.Count - 1].connections = el.connections;
                    //p.elements[p.elements.Count - 1].ID = el.ID;
                    p.elements[p.elements.Count - 1].Margin = new Thickness(el.l, el.top, el.r, el.bottom);
                }
                catch { }
            }

            foreach (var w in lab.lines)
            {
                p.lines.Add(new Tuple<System.Windows.Shapes.Polyline, int, int, string, string, Ischecked, int>(new System.Windows.Shapes.Polyline() { Stroke = Brushes.DarkOliveGreen, StrokeThickness = 4 },
                    w.Item1, w.Item2, w.Item3, w.Item4, new Ischecked() { ischecked = w.Item5.ischecked }, w.Item6));
                p.workspace.Children.Add(p.lines.Last().Item1);
            }
            return p;
        }

    }
}
