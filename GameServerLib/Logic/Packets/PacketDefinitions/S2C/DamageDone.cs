using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class DamageDone : BasePacket
    {
        public DamageDone(AttackableUnit source, AttackableUnit target, float amount, DamageType type, DamageText damageText)
            : base(PacketCmd.PKT_S2C_DamageDone, target.NetId)
        {
            buffer.Write((byte)damageText);
            buffer.Write((short)((short)type << 8));
            buffer.Write((float)amount);
            buffer.Write((int)target.NetId);
            buffer.Write((int)source.NetId);
        }
    }
}