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


        // Adapted from http://sanity-free.org/12/crc32_implementation_in_csharp.html
        public static class CRC32
        {
            static readonly uint[] _table = new uint[256];

            static CRC32()
            {
                uint entry;
                for (uint i = 0; i < 256; i++)
                {
                    entry = i;
                    for (var j = 0; j < 8; j++)
                    {
                        if ((entry & 1) == 1)
                        {
                            entry = (entry >> 1) ^ 0xedb88320;
                        }
                        else
                        {
                            entry >>= 1;
                        }
                    }
                    _table[i] = entry;
                }
            }

            public static uint ComputeChecksum(string str, Encoding enc)
            {
                return ComputeChecksum(enc.GetBytes(str));
            }

            public static uint ComputeChecksum(string str)
            {
                return ComputeChecksum(str, Encoding.ASCII);
            }

            public static uint ComputeChecksum(byte[] bytes)
            {
                uint hash = 0xffffffff;
                for (var i = 0; i < bytes.Length; i++)
                {
                    hash = (hash >> 8) ^ _table[bytes[i] ^ hash & 0xff];
                }
                return ~hash;
            }
        }
    }
}
