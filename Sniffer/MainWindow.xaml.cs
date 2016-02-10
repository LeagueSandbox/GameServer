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
        private List<Packet> packetsReceived = new List<Packet>();

        public MainWindow()
        {
            InitializeComponent();

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

        public void Send(byte[] data, bool s2c, bool broadcast)
        {
            if (writer == null || !writer.BaseStream.CanWrite)
                return;

            writer.Write(data.Length);
            writer.Write((byte)(s2c ? 1 : 0));
            writer.Write((byte)(broadcast ? 1 : 0));
            writer.Write(data, 0, data.Length);
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
            p.ContentSelectionChanged += (a, b) => { selectedPacketDesc.SelectedIndex = ((ListView)a).SelectedIndex; };
            selectedPacketDesc.Items.Clear();
            try
            {
                foreach (SelectedPacketLine i in p.Content.Items)
                    selectedPacketDesc.Items.Add(new SelectedPacketDesc(i));
            }
            catch { }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            TextRange range = new TextRange(packetContent.ContentStart, packetContent.ContentEnd);
            if (string.IsNullOrWhiteSpace(range.Text))
                return;

            var packets = range.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var packet in packets)
            {
                var list = new List<byte>();
                var bytes = packet.Split(' ');
                foreach (var b in bytes)
                    list.Add(byte.Parse(b, System.Globalization.NumberStyles.HexNumber));

                if (list.Count > 0)
                    Send(list.ToArray(), packetDirectionS2C.IsChecked.Value, packetIsBroadcast.IsChecked.Value);
            }
        }

        private void RichTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length < 1)
                return;

            if (!Char.IsDigit(e.Text[0]))
                e.Handled = true;
        }
    }
}
