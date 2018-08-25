using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class HeroSpawn2 : BasePacket
    {
        public HeroSpawn2(IChampion p) : base(PacketCmd.PKT_S2C_OBJECT_SPAWN, p.NetId)
        {
            Fill(0, 15);
            Write((byte)0x80); // unk
            Write((byte)0x3F); // unk
            Fill(0, 13);
            Write((byte)3); // unk
            Write((uint)1); // unk
            Write(p.X);
            Write(p.Y);
            Write((float)0x3F441B7D); // z ?
            Write((float)0x3F248DBB); // Rotation ?
        }
    }
}