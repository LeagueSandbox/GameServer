using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntWarsSharp.Logic
{
    public class SnifferPacket
    {
        public int len { get; set; }
        public bool s2c { get; set; }
        public bool broadcast { get; set; }
        public byte[] data { get; set; }
    }
}
