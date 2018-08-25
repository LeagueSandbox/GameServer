using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.PacketDefinitions;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class SpawnPlaceable : Packet
    {
        public SpawnPlaceable(IPlaceable p)
            : base(PacketCmd.PKT_S2C_OBJECT_SPAWN)
        {

            WriteNetId(p);

            Write((byte)0xB5);
            Write((byte)0x00);
            Write((byte)0xB3);
            Write((byte)0x00);
            Write((byte)0x7C);

            WriteNetId(p);
            WriteNetId(p);
            WriteNetId(p.Owner);

            Write((byte)0x40);

            Write(p.X); //x
            Write(p.GetZ()); //z
            Write(p.Y); //y

            Fill(0, 8);

            Write((short)p.Team);
            Write((byte)0x92);
            Write((byte)0x00);

            Write((byte)0x00);
            Write((byte)0x00);
            Write((byte)0x02);
            Write((byte)0x00);
            Write((byte)0x00);
            Write((byte)0x00);
            Write((byte)0x00);

			WriteConstLengthString(p.Name, 64);

			WriteConstLengthString(p.Model, 64);

            Write((byte)0x01);

            Fill(0, 16);

            Write(1.0f); // Unk

            Fill(0, 13);

            Write((byte)0x03);

            Write((byte)0xB1); // <--|
            Write((byte)0x20); //    | Unknown, changes between packets
            Write((byte)0x18); //    |
            Write((byte)0x00); // <--|

            Write(p.X);
            Write(p.Y);

            Write((byte)0x00); // 0.0f
            Write((byte)0x00); // Probably a float, see SpawnMonster
            Write((byte)0x00);
            Write((byte)0x00);

            Write((byte)0xFF); // 1.0f
            Write((byte)0xFF); // Probably a float, see SpawnMonster
            Write((byte)0x7F);
            Write((byte)0x3F);
        }
    }
}