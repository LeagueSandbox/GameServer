using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public enum ItemOwnerType : byte
    {
        Champion = 0x29,
        Turret = 0x01
    }
}
