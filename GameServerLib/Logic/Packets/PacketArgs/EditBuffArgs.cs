using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Logic.Packets.PacketArgs.DTO;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketArgs
{
    public struct EditBuffArgs
    {
        public PacketObject Unit { get; }
        public byte Slot { get; }
        public byte Stacks { get; }

        public EditBuffArgs(PacketObject unit, byte slot, byte stacks)
        {
            Unit = unit;
            Slot = slot;
            Stacks = stacks;
        }
    }
}
