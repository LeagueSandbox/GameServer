using IntWarsSharp.Core.Logic.PacketHandlers;
using MahApps.Metro.Controls;
using SnifferApp.Logic;
using SnifferApp.Logic.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net.Sockets;
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

namespace SnifferApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private BinaryWriter writer;
        private BinaryReader reader;

        public MainWindow()
        {
            InitializeComponent();

            var client = new TcpClient();
            client.Connect("127.0.0.1", 5478);
            reader = new BinaryReader(client.GetStream());
            writer = new BinaryWriter(client.GetStream());

            new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    var r = Receive();
                    if (r == null)
                        continue;
                   
                    Dispatcher.Invoke(() =>
                    {
                        var line = new PacketLine(r);
                        packets.Items.Add(line);
                    });
                }
            })).Start();
        }

        public void Send(byte[] data)
        {
            if (writer == null || !writer.BaseStream.CanWrite)
                return;

            byte[] toWrite = new byte[data.Length + 4];
            BitConverter.GetBytes(data.Length).CopyTo(toWrite, 0);
            data.CopyTo(toWrite, 4);
            writer.Write(toWrite, 0, toWrite.Length);
        }

        public Packet Receive()
        {
            if (reader == null || !reader.BaseStream.CanRead)
                return null;
            var packet = new Packet();

            packet.len = reader.ReadInt32();
            packet.s2c = reader.ReadByte() == 1 ? true : false;
            packet.broadcast = reader.ReadByte() == 1 ? true : false;
            if (packet.len < 1)
                return null;

            packet.data = reader.ReadBytes(packet.len);

            return packet;
        }
    }
}
