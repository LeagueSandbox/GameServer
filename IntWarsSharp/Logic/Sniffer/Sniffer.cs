using IntWarsSharp.Core.Logic;
using IntWarsSharp.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SnifferApp.Logic
{
    public class Sniffer
    {
        private static Sniffer _instance;
        private BinaryWriter writer;
        private BinaryReader reader;
        private Game game;

        private unsafe Sniffer()
        {
            var listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 5478);
            listener.Start();
            new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    var c = listener.AcceptTcpClient();
                    writer = new BinaryWriter(c.GetStream());
                    reader = new BinaryReader(c.GetStream());
                }
            })).Start();


            new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    try
                    {
                        var p = Receive();
                        if (p == null)
                            continue;

                        var players = game.getPlayers();
                        if (players.Count < 1)
                            return;

                        var player = players[0];
                        if (player.Item2 == null || player.Item2.getPeer() == null)
                            return;

                        if (p.s2c)
                        {
                            if (p.broadcast)
                                PacketHandlerManager.getInstace().broadcastPacket(p.data, IntWarsSharp.Core.Logic.PacketHandlers.Channel.CHL_S2C);
                            else
                                PacketHandlerManager.getInstace().sendPacket(player.Item2.getPeer(), p.data, IntWarsSharp.Core.Logic.PacketHandlers.Channel.CHL_S2C);
                        }
                        else
                        {
                            PacketHandlerManager.getInstace().handlePacket(player.Item2.getPeer(), p.data, IntWarsSharp.Core.Logic.PacketHandlers.Channel.CHL_C2S);
                        }
                    }
                    catch { reader = null; }
                }
            })).Start();
        }

        public void Send(byte[] data, bool s2c, bool broadcast = false)
        {
            try
            {
                if (writer == null || !writer.BaseStream.CanWrite)
                    return;

                writer.Write((int)data.Length);
                writer.Write((byte)(s2c ? 1 : 0));
                writer.Write((byte)(broadcast ? 1 : 0));
                writer.Write(data, 0, data.Length);
            }
            catch { writer = null; }
        }

        public SnifferPacket Receive()
        {
            if (reader == null || !reader.BaseStream.CanRead)
                return null;

            var packet = new SnifferPacket();
            packet.len = reader.ReadInt32();
            packet.s2c = reader.ReadByte() == 1 ? true : false;
            packet.broadcast = reader.ReadByte() == 1 ? true : false;
            if (packet.len < 1)
                return null;

            return packet;
        }

        public static Sniffer getInstance()
        {
            if (_instance == null)
                _instance = new Sniffer();

            return _instance;
        }

        public void setGame(Game game)
        {
            this.game = game;
        }
    }
}
