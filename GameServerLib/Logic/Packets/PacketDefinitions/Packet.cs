using System;
using System.Collections.Generic;
using System.IO;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions
{
    public class Packet
    {
        protected readonly Game Game = Program.ResolveDependency<Game>();

        protected List<byte> _bytes = new List<byte>();

        public Packet(PacketCmd cmd)
        {
            Write((byte)cmd);
        }

        internal byte[] GetBytes()
        {
            return _bytes.ToArray();
        }

        public void Fill(byte data, int length)
        {
            var arr = new byte[length];
            for (var i = 0; i < length; ++i)
            {
                arr[i] = data;
            }

            Write(arr);
        }

        public void Write(bool b)
        {
            Write((byte)(b ? 1u : 0u));
        }

        public void Write(byte b)
        {
            _bytes.Add(b);
        }

        public void Write(byte[] b)
        {
            _bytes.AddRange(b);
        }

        public void Write(short s)
        {
            var arr = new byte[2];
            for (var i = 0; i < 2; i++)
            {
                arr[i] = (byte)(s >> (i * 8));
            }

            Write(arr);
        }

        public void Write(ushort s)
        {
            var arr = new byte[2];
            for (var i = 0; i < 2; i++)
            {
                arr[i] = (byte)(s >> (i * 8));
            }

            Write(arr);
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

        public void Write(long l)
        {
            var arr = new byte[8];
            for (var i = 0; i < 8; i++)
            {
                arr[i] = (byte)(l >> (i * 8));
            }

            Write(arr);
        }

        public void Write(ulong l)
        {
            var arr = new byte[8];
            for (var i = 0; i < 8; i++)
            {
                arr[i] = (byte)(l >> (i * 8));
            }

            Write(arr);
        }

        public void Write(float f)
        {
            Write(BitConverter.GetBytes(f));
        }

        public void Write(double d)
        {
            Write(BitConverter.GetBytes(d));
        }
    }
}
