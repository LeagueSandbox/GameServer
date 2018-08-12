using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class ModifyShield : BasePacket
    {
        public ModifyShield(AttackableUnit unit, float amount, ShieldType type)
            : base(PacketCmd.PKT_S2C_MODIFY_SHIELD, unit.NetId)
        {
            Write((byte)type);
            Write(amount);
        }
    }
}