using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class CreateMonsterCamp : BasePacket
    {
        public CreateMonsterCamp(float x, float y, float z, string iconName, byte campId, byte campUnk, float unk)
            : base(PacketCmd.PKT_S2C_CREATE_MONSTER_CAMP)
        {
            Write(x);
            Write(z);
            Write(y);
			WriteConstLengthString(iconName, 64);
            Write(campId);
            Write(campUnk);

            /*buffer.Write((byte)0x64); // <-|
            buffer.Write((byte)0x15); //   |
            buffer.Write((byte)0xFB); //   |-> Unk
            buffer.Write((byte)0x41); //   |
            buffer.Write((byte)0x0C); // <-|*/
            Fill(0, 5);
            Write(unk);
        }
    }
}