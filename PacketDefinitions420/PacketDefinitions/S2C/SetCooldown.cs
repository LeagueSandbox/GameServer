using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class SetCooldown : BasePacket
    {
        public SetCooldown(uint netId, byte slotId, float currentCd, float totalCd = 0.0f)
            : base(PacketCmd.PKT_S2C_SET_COOLDOWN, netId)
        {
            Write(slotId);
            Write((byte)0xF8); // 4.18
            Write(currentCd);
            Write(totalCd);
        }
    }
}