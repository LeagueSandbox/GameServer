using IntWarsSharp.Logic.Packets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace IntWarsSharp
{
    static class Extensions
    {
        public static float SqrLength(this Vector2 v)
        {
            return (v.X * v.X + v.Y * v.Y);
        }
        public static long ElapsedNanoSeconds(this Stopwatch watch)
        {
            return watch.ElapsedTicks * 1000000000 / Stopwatch.Frequency;
        }
        public static long ElapsedMicroSeconds(this Stopwatch watch)
        {
            return watch.ElapsedTicks * 1000000 / Stopwatch.Frequency;
        }
        public static void Add(this List<byte> list, int val)
        {
            list.AddRange(PacketHelper.intToByteArray(val));
        }
        public static void Add(this List<byte> list, string val)
        {
            list.AddRange(Encoding.BigEndianUnicode.GetBytes(val));
        }
        public static void fill(this BinaryWriter list, byte data, int length)
        {
            for (var i = 0; i < length; ++i)
            {
                list.Write(data);
            }
        }
    }
}
