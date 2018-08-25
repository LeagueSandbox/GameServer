using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class StopAutoAttack : BasePacket
    {
        public StopAutoAttack(IAttackableUnit attacker)
            : base(PacketCmd.PKT_S2C_STOP_AUTO_ATTACK, attacker.NetId)
        {
            Write((byte)0); // Flag
            Write(0); // A netId
        }
    }
}