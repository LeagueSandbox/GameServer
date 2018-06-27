using System.Collections.Generic;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class UnitAnnounce : BasePacket
    {
        public UnitAnnounce(UnitAnnounces id, AttackableUnit target, GameObject killer = null, List<Champion> assists = null)
            : base(PacketCmd.PKT_S2C_ANNOUNCE2, target.NetId)
        {
            if (assists == null)
                assists = new List<Champion>();

            _buffer.Write((byte)id);
            if (killer != null)
            {
                _buffer.Write((long)killer.NetId);
                _buffer.Write(assists.Count);
                foreach (var a in assists)
                    _buffer.Write(a.NetId);
                for (var i = 0; i < 12 - assists.Count; i++)
                    _buffer.Write(0);
            }
        }
    }
}