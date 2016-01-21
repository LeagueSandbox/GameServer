using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace freedompeace.RiotArchive
{
    internal static class RiotArchiveHash
    {
        public static uint GetHash(string s)
        {
            uint hash = 0;
            var bytes = Encoding.ASCII.GetBytes(s.ToLowerInvariant());

            foreach (byte b in bytes)
            {
                hash = (hash << 4) + b;

                var temp = hash & 0xF0000000;
                if (temp == 0)
                    continue;

                hash = hash ^ (temp >> 24);
                hash = hash ^ temp;
            }
            return hash;
        }
    }
}
