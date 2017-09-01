using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Logic.Packets.PacketArgs.DTO;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketArgs
{
    public struct DestroyObjectArgs
    {
        public PacketObject Destroyer { get; }
        public PacketObject Destroyed { get; }

        public DestroyObjectArgs(PacketObject destroyer, PacketObject destroyed)
        {
            Destroyer = destroyer;
            Destroyed = destroyed;
        }
    }
}
