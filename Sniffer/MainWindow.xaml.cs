using IntWarsSharp.Core.Logic.PacketHandlers;
using MahApps.Metro.Controls;
using SnifferApp.Logic;
using SnifferApp.Logic.Config;
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
        private List<Packet> packetsReceived = new List<Packet>();

        public MainWindow()
        {
            InitializeComponent();

            PacketConfig.getInstance();

            new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    try
                    {
                        var client = new TcpClient();
                        client.Connect("127.0.0.1", 5478);
                        reader = new BinaryReader(client.GetStream());
                        writer = new BinaryWriter(client.GetStream());
                        Thread.CurrentThread.Abort();
                    }
                    catch { }
                    Thread.Sleep(1000);
                }
            })).Start();

            new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    var p = Receive();
                    if (p == null)
                        continue;
                    packetsReceived.Add(p);
                }
            })).Start();

            var timer = new System.Timers.Timer(10);
            timer.AutoReset = true;
            timer.Elapsed += (a, b) =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (packetsReceived.Count < 1)
                        return;
                    var line = new PacketLine(packetsReceived[0]);
                    packets.Items.Add(line);
                    packetsReceived.RemoveAt(0);
                });

                /* var temp = packetsReceived.ToList();
                System.Diagnostics.Debug.WriteLine(temp.Count);
                Dispatcher.Invoke(() =>
                {
                    packets.Items.Clear();
                    foreach (var p in temp)
                    {
                        var line = new PacketLine(p);
                        packets.Items.Add(line);
                    }
                });*/
            };
            timer.Start();
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

            try
            {
                var packet = new Packet();

                packet.len = reader.ReadInt32();
                packet.s2c = reader.ReadByte() == 1 ? true : false;
                packet.broadcast = reader.ReadByte() == 1 ? true : false;
                if (packet.len < 1)
                    return null;

                packet.data = reader.ReadBytes(packet.len);
                packet.ReceivedTime = DateTime.Now;
                return packet;
            }
            catch
            {
                writer = null;
                reader = null;
            }
            return null;
        }

        private void packets_Selected(object sender, RoutedEventArgs e)
        {
            var view = sender as ListView;
            if (view == null)
                return;

            var line = view.SelectedItem as PacketLine;
            if (line == null)
                return;

            var p = new SelectedPacket(line.packet);
            selectedPacket.Items.Clear();
            selectedPacket.Items.Add(p);

            selectedPacketDesc.Items.Clear();
            try
            {
                foreach (SelectedPacketLine i in p.Content.Items)
                    selectedPacketDesc.Items.Add(new SelectedPacketDesc(i));
            }
            catch { }
        }
    }
}
