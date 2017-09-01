using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class ModifyShield : BasePacket
    {
        public ModifyShield(Unit unit, float amount, ShieldType type)
            : base(PacketCmd.PKT_S2C_ModifyShield, unit.NetId)
        {
            buffer.Write((byte)type);
            buffer.Write((float)amount);
        }
    }
}