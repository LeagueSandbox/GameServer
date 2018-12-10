using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.PacketDefinitions;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class SpawnMinion : Packet
    {
        public SpawnMinion(IMinion m)
            : base(PacketCmd.PKT_S2C_OBJECT_SPAWN)
        {

            WriteNetId(m);

            Write((byte)0xB5);
            Write((byte)0x00);
            Write((byte)0xB3);
            Write((byte)0x00);
            Write((byte)0x7C);

            WriteNetId(m);
            WriteNetId(m);
            WriteNetId(m.Owner);

            Write((byte)0x40);

            Write(m.X); //x
            Write(m.GetZ()); //z
            Write(m.Y); //y

            Fill(0, 8);

            Write((short)m.Team);
            Write((byte)0x92);
            Write((byte)0x00);

            Write((byte)0x00);
            Write((byte)0x00);
            Write((byte)0x02);
            Write((byte)0x00);
            Write((byte)0x00);
            Write((byte)0x00);
            Write((byte)0x00);

			WriteConstLengthString(m.Name, 64);

			WriteConstLengthString(m.Model, 64);

            Write((byte)0x01);

            Fill(0, 16);

            Write(1.0f); // Unk

            Fill(0, 13);

            Write((byte)0x03);

            Write((byte)0xB1); // <--|
            Write((byte)0x20); //    | Unknown, changes between packets
            Write((byte)0x18); //    |
            Write((byte)0x00); // <--|

            Write(m.X);
            Write(m.Y);

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