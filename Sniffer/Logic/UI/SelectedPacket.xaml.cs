using SnifferApp.Logic.Config;
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
        private SelectedPacket()
        {
            InitializeComponent();
        }

        public SelectedPacket(Packet p) : this()
        {
            var config = PacketConfig.getInstance().getConfig(p.header, p.s2c);
            var reader = new BinaryReader(new MemoryStream(p.data));

            foreach (var conf in config)
            {
                if (reader.BaseStream.Position + getLenght(conf.Key) >= reader.BaseStream.Length)
                    continue;

                switch (conf.Key)
                {
                    case "b": //byte
                        appendBytes(reader.ReadByte(), conf.Key, conf.Value);
                        break;
                    case "s": //short
                        appendBytes(reader.ReadInt16(), conf.Key, conf.Value);
                        break;
                    case "d": //int
                        appendBytes(reader.ReadInt32(), conf.Key, conf.Value);
                        break;
                    case "d+": //int
                        appendBytes(reader.ReadUInt32(), conf.Key, conf.Value);
                        break;
                    case "l": //long
                        appendBytes(reader.ReadInt64(), conf.Key, conf.Value);
                        break;
                    case "f": //float
                        appendBytes(reader.ReadSingle(), conf.Key, conf.Value);
                        break;
                    case "str": //string
                        appendString(reader.ReadString(), conf.Key, conf.Value);
                        break;
                    case "fill":
                        break;
                }
            }
            if (reader.BaseStream.Position < reader.BaseStream.Length)
                appendBytes(reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position)), "unk", "unk");
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
            appendHex(f.ToString("X2").Split('-'), type, fieldName);
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
    }
}
