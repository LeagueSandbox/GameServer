using System.Text;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class CreateMonsterCamp : BasePacket
    {
        public CreateMonsterCamp(float x, float y, float z, string iconName, byte campId, byte campUnk, float unk)
            : base(PacketCmd.PKT_S2C_CreateMonsterCamp)
        {
            buffer.Write((float)x);
            buffer.Write((float)z);
            buffer.Write((float)y);
            buffer.Write(Encoding.Default.GetBytes(iconName));
            buffer.fill(0, 64 - iconName.Length);
            buffer.Write((byte)campId);
            buffer.Write((byte)campUnk);

            /*buffer.Write((byte)0x64); // <-|
            buffer.Write((byte)0x15); //   |
            buffer.Write((byte)0xFB); //   |-> Unk
            buffer.Write((byte)0x41); //   |
            buffer.Write((byte)0x0C); // <-|*/
            buffer.fill(0, 5);
            buffer.Write((float)unk);
        }
    }
}