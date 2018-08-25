using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class ModifyShield : BasePacket
    {
        public ModifyShield(IAttackableUnit unit, float amount, ShieldType type)
            : base(PacketCmd.PKT_S2C_MODIFY_SHIELD, unit.NetId)
        {
            Write((byte)type);
            Write(amount);
        }
    }
}