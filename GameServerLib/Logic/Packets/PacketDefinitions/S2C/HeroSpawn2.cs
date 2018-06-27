using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class HeroSpawn2 : BasePacket
    {
        public HeroSpawn2(Champion p) : base(PacketCmd.PKT_S2_C_OBJECT_SPAWN, p.NetId)
        {
            _buffer.Fill(0, 15);
            _buffer.Write((byte)0x80); // unk
            _buffer.Write((byte)0x3F); // unk
            _buffer.Fill(0, 13);
            _buffer.Write((byte)3); // unk
            _buffer.Write((uint)1); // unk
            _buffer.Write(p.X);
            _buffer.Write(p.Y);
            _buffer.Write((float)0x3F441B7D); // z ?
            _buffer.Write((float)0x3F248DBB); // Rotation ?
        }
    }
}