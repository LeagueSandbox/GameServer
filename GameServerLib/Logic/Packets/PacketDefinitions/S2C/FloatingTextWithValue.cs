using System.Text;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class FloatingTextWithValue : BasePacket
    {
        public FloatingTextWithValue(uint unitNetId, int value, string text)
            : base(PacketCmd.PKT_S2C_FloatingTextWithValue)
        {
            buffer.Write(unitNetId);
            buffer.Write((int)15); // Unk
            buffer.Write(value); // Example -3
            buffer.Write(Encoding.Default.GetBytes(text));
            buffer.Write((byte)0x00);
        }
    }
}