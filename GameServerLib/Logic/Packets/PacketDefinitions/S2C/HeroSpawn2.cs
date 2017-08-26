using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class HeroSpawn2 : BasePacket
    {
        public HeroSpawn2(Champion p) : base(PacketCmd.PKT_S2C_ObjectSpawn, p.NetId)
        {
            buffer.fill(0, 15);
            buffer.Write((byte)0x80); // unk
            buffer.Write((byte)0x3F); // unk
            buffer.fill(0, 13);
            buffer.Write((byte)3); // unk
            buffer.Write((uint)1); // unk
            buffer.Write(p.X);
            buffer.Write(p.Y);
            buffer.Write((float)0x3F441B7D); // z ?
            buffer.Write((float)0x3F248DBB); // Rotation ?
        }
    }
}