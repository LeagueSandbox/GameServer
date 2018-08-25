using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class FloatingTextWithValue : BasePacket
    {
        public FloatingTextWithValue(IAttackableUnit u, int value, string text)
            : base(PacketCmd.PKT_S2C_FLOATING_TEXT_WITH_VALUE)
        {
            WriteNetId(u);
            Write(15); // Unk
            Write(value); // Example -3
            Write(text);
            Write((byte)0x00);
        }
    }
}