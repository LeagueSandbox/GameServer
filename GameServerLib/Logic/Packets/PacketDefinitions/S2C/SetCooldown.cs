using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SetCooldown : BasePacket
    {
        public SetCooldown(uint netId, byte slotId, float currentCd, float totalCd = 0.0f)
            : base(PacketCmd.PKT_S2C_SetCooldown, netId)
        {
            buffer.Write(slotId);
            buffer.Write((byte)0xF8); // 4.18
            buffer.Write(currentCd);
            buffer.Write(totalCd);
        }
    }
}