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
            return HashStringNorm(section + '*' + name);
        }

        public static uint HashStringNorm(string str)
        {
            uint hash = 0;

            for (var i = 0; i < str.Length; i++)
            {
                hash = char.ToLower(str[i]) + 65599 * hash;
            }

            return hash;
        }
    }
}
