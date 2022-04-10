using GameServerCore.Packets.PacketDefinitions;
using PacketDefinitions420.PacketDefinitions;
using System.Collections.Generic;

namespace PacketDefinitions420
{
    public class Packet : IPacket
    {
        protected List<byte> _bytes = new List<byte>();

        public Packet(PacketCmd cmd)
        {
            Write((byte)cmd);
        }

        public byte[] GetBytes()
        {
            return _bytes.ToArray();
        }

        public void Write(byte b)
        {
            _bytes.Add(b);
        }

        public void Write(byte[] b)
        {
            _bytes.AddRange(b);
        }

        public void Write(int n)
        {
            var arr = new byte[4];
            for (var i = 0; i < 4; i++)
            {
                arr[i] = (byte)(n >> (i * 8));
            }

            Write(arr);
        }

        public void Write(uint n)
        {
            var arr = new byte[4];
            for (var i = 0; i < 4; i++)
            {
                arr[i] = (byte)(n >> (i * 8));
            }

            Write(arr);
        }
    }
}
