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
        private Sniffer()
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
                        if (reader != null)
                            System.Diagnostics.Debug.WriteLine(BitConverter.ToString(Receive()));
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

        public byte[] Receive()
        {
            if (!reader.BaseStream.CanRead)
                return null;

            var len = reader.ReadInt32();
            if (len < 1)
                return null;

            return reader.ReadBytes(len);
        }

        public static Sniffer getInstance()
        {
            if (_instance == null)
                _instance = new Sniffer();

            return _instance;
        }
    }
}
