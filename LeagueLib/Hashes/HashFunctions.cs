using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueLib.Hashes
{
    public static class HashFunctions
    {
        public static uint LeagueHash(string value)
        {
            uint hash = 0;
            uint temp = 0;

            value = value.ToLower();

            for (int i = 0; i < value.Length; i++)
            {
                hash = (hash << 4) + value[i];
                temp = hash & 0xf0000000;
                if (temp != 0)
                {
                    hash = hash ^ (temp >> 24);
                    hash = hash ^ temp;
                }
            }

            return hash;
        }
    }
}
