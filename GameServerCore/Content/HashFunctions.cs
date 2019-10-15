using System;
using System.Text;

namespace GameServerCore.Content
{
    public class HashFunctions
    {
        public static uint HashString(string path)
        {
            uint hash = 0;
            var mask = 0xF0000000;
            for (var i = 0; i < path.Length; i++)
            {
                hash = char.ToLower(path[i]) + 0x10 * hash;
                if ((hash & mask) > 0)
                {
                    hash ^= hash & mask ^ (hash & mask) >> 24;
                }
            }

            return hash;
        }

        public static uint HashStringSdbm(string section, string name)
        {
            uint hash = 0;
            foreach (var c in section)
            {
                hash = char.ToLower(c) + 65599 * hash;
            }

            hash = char.ToLower('*') + 65599 * hash;
            foreach (var c in name)
            {
                hash = char.ToLower(c) + 65599 * hash;
            }

            return hash;
        }


        // From http://sanity-free.org/12/crc32_implementation_in_csharp.html
        public static class Crc32
        {
            static readonly uint[] table;

            public static uint ComputeChecksum(byte[] bytes)
            {
                uint crc = 0xffffffff;
                for (int i = 0; i < bytes.Length; ++i)
                {
                    byte index = (byte)(((crc) & 0xff) ^ bytes[i]);
                    crc = (uint)((crc >> 8) ^ table[index]);
                }
                return ~crc;
            }

            public static uint ComputeChecksum(string str)
            {
                return ComputeChecksum(Encoding.ASCII.GetBytes(str));
            }

            public static byte[] ComputeChecksumBytes(byte[] bytes)
            {
                return BitConverter.GetBytes(ComputeChecksum(bytes));
            }

            static Crc32()
            {
                uint poly = 0xedb88320;
                table = new uint[256];
                uint temp = 0;
                for (uint i = 0; i < table.Length; ++i)
                {
                    temp = i;
                    for (int j = 8; j > 0; --j)
                    {
                        if ((temp & 1) == 1)
                        {
                            temp = (uint)((temp >> 1) ^ poly);
                        }
                        else
                        {
                            temp >>= 1;
                        }
                    }
                    table[i] = temp;
                }
            }
        }
    }
}
