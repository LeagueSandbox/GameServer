using System.Collections.Generic;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class UnitAnnounce : BasePacket
    {
        public UnitAnnounce(UnitAnnounces id, IAttackableUnit target, IGameObject killer = null, List<IChampion> assists = null)
            : base(PacketCmd.PKT_S2C_ANNOUNCE2, target.NetId)
        {
            if (assists == null)
                assists = new List<IChampion>();

            Write((byte)id);
            if (killer != null)
            {
                Write((long)killer.NetId);
                Write(assists.Count);
                foreach (var a in assists)
                {
                    WriteNetId(a);
                }

                for (var i = 0; i < 12 - assists.Count; i++)
                {
                    Write(0);
                }
            }
        }
    }
}