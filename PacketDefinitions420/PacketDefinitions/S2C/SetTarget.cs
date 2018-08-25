using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class SetTarget : BasePacket
    {
        public SetTarget(IAttackableUnit attacker, IAttackableUnit attacked)
            : base(PacketCmd.PKT_S2C_SET_TARGET, attacker.NetId)
        {
            WriteNetId(attacked);
        }
    }
}