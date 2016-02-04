using IntWarsSharp.Core.Logic.PacketHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SnifferApp.Logic
{
    public class Packet
    {
        public int len { get; set; }
        public bool s2c { get; set; }
        public bool broadcast { get; set; }
        public byte[] data { get; set; }
        public byte header
        {
            get
            {
                return data[0];
            }
            private set
            {
            }
        }
        public string Name
        {
            get
            {
                return s2c ? ((PacketCmdS2C)header).ToString().Replace("PKT_S2C_", "") : ((PacketCmdC2S)header).ToString().Replace("PKT_C2S_", "");
            }
            private set
            {
            }
        }
    }
}
