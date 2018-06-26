using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Content
{
    public class HashFunctions
    {
        static public uint HashString(string path)
        {
            uint hash = 0;
            var mask = 0xF0000000;
            for (var i = 0; i < path.Length; i++)
            {
                hash = char.ToLower(path[i]) + 0x10 * hash;
                if ((hash & mask) > 0)
                {
                    hash ^= hash & mask ^ ((hash & mask) >> 24);
                }
            }
            return hash;
        }

        static public UInt32 HashStringSdbm(string section, string name)
        {
            UInt32 hash = 0;
            foreach (var c in section)
                hash = char.ToLower(c) + 65599 * hash;
            hash = char.ToLower('*') + 65599 * hash;
            foreach (var c in name)
                hash = char.ToLower(c) + 65599 * hash;
            return hash;
        }
    }


}
