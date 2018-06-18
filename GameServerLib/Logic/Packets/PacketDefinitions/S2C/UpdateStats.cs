using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class UpdateStats : BasePacket
    {
        public UpdateStats(AttackableUnit u, bool partial = true)
            : base(PacketCmd.PKT_S2C_CharStats)
        {
            // TODO: replication
        }
    }
}