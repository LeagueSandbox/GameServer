using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class DamageDone : BasePacket
    {
        public DamageDone(IAttackableUnit source, IAttackableUnit target, float amount, DamageType type, DamageText damageText)
            : base(PacketCmd.PKT_S2C_DAMAGE_DONE, target.NetId)
        {
            Write((byte)damageText);
            Write((short)((short)type << 8));
            Write(amount);
            WriteNetId(target);
            WriteNetId(source);
        }
    }
}