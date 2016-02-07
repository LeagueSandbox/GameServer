using IntWarsSharp.Core.Logic.PacketHandlers;
using SnifferApp.net.Packets;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace SnifferApp.Logic.UI
{
    /// <summary>
    /// Interaction logic for SelectedPacket.xaml
    /// </summary>
    public partial class SelectedPacket : UserControl
    {
        public event EventHandler ContentSelectionChanged;

        private SelectedPacket()
        {
            InitializeComponent();
        }

        public SelectedPacket(Packet p) : this()
        {
            Type type;
            if (p.s2c)
                type = Type.GetType("SnifferApp.net.Packets." + ((PacketCmdS2C)p.header).ToString());
            else
                type = Type.GetType("SnifferApp.net.Packets." + ((PacketCmdC2S)p.header).ToString());

            if (type == null)
            {
                appendBytes(p.data, "unk", "unk");
                return;
            }

            var instance = Activator.CreateInstance(type, p.data) as Packets;
            foreach (var conf in instance.data)
            {
                switch (conf.Item1)
                {
                    case "b": //byte
                        appendBytes((byte)conf.Item3, conf.Item1, conf.Item2);
                        break;
                    case "s": //short
                        appendBytes((short)conf.Item3, conf.Item1, conf.Item2);
                        break;
                    case "d": //int
                        appendBytes((int)conf.Item3, conf.Item1, conf.Item2);
                        break;
                    case "d+": //uint
                        appendBytes((uint)conf.Item3, conf.Item1, conf.Item2);
                        break;
                    case "l": //long
                        appendBytes((long)conf.Item3, conf.Item1, conf.Item2);
                        break;
                    case "f": //float
                        appendBytes((float)conf.Item3, conf.Item1, conf.Item2);
                        break;
                    case "str": //string
                        appendString((string)conf.Item3, conf.Item1, conf.Item2);
                        break;
                    case "fill":
                        appendBytes((byte[])conf.Item3, conf.Item1, conf.Item2);
                        break;
                }
            }
        }

        private long getLenght(string key)
        {
            switch (key)
            {
                case "b": //byte
                    return 1;
                case "s": //short
                    return 2;
                case "d": //int
                    return 4;
                case "l": //long
                    return 8;
                case "f": //float
                    return 4;
            }
            return 0;
        }

        private void appendBytes(byte b, string type, string fieldName)
        {
            appendHex(new string[] { b.ToString("X2") }, type, fieldName);
        }
        private void appendBytes(short s, string type, string fieldName)
        {
            appendHex(s.ToString("X2").Split('-'), type, fieldName);
        }
        private void appendBytes(int i, string type, string fieldName)
        {
            appendHex(i.ToString("X2").Split('-'), type, fieldName);
        }
        private void appendBytes(long l, string type, string fieldName)
        {
            appendHex(l.ToString("X2").Split('-'), type, fieldName);
        }
        private void appendBytes(float f, string type, string fieldName)
        {
            appendHex(BitConverter.DoubleToInt64Bits(f).ToString("X2").Split('-'), type, fieldName);
        }
        private void appendBytes(byte[] bytes, string type, string fieldName)
        {
            var line = new SelectedPacketLine(type, fieldName);
            var cont = "";
            foreach (var b in bytes)
                cont += b.ToString("X2") + " ";
            line.Content.Content = cont;
            Content.Items.Add(line);
        }
        private void appendString(string s, string type, string fieldName)
        {
            var line = new SelectedPacketLine(type, fieldName);
            line.Content.Content = s;
            Content.Items.Add(line);
        }
        private void appendHex(string[] hex, string type, string fieldName)
        {
            var line = new SelectedPacketLine(type, fieldName);
            var cont = "";
            foreach (var h in hex)
                cont += h + " ";
            line.Content.Content = cont;
            Content.Items.Add(line);
        }

        private void Content_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ContentSelectionChanged != null)
                ContentSelectionChanged(sender, e);
        }
    }
}
