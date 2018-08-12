using System.Text;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SpawnPlaceable : Packet
    {
        public SpawnPlaceable(Placeable p)
            : base(PacketCmd.PKT_S2C_ObjectSpawn)
        {

            buffer.Write(p.NetId);

            buffer.Write((byte)0xB5);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0xB3);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x7C);

            buffer.Write(p.NetId);
            buffer.Write(p.NetId);
            buffer.Write(p.Owner.NetId);

            buffer.Write((byte)0x40);

            buffer.Write((float)p.X); //x
            buffer.Write((float)p.GetZ()); //z
            buffer.Write((float)p.Y); //y

            buffer.fill(0, 8);

            buffer.Write((short)p.Team);
            buffer.Write((byte)0x92);
            buffer.Write((byte)0x00);

            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x02);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);

            buffer.Write(Encoding.Default.GetBytes(p.Name));
            buffer.fill(0, 64 - p.Name.Length);

            buffer.Write(Encoding.Default.GetBytes(p.Model));
            buffer.fill(0, 64 - p.Model.Length);

            buffer.Write((byte)0x01);

            buffer.fill(0, 16);

            buffer.Write((float)1.0f); // Unk

            buffer.fill(0, 13);

            buffer.Write((byte)0x03);

            buffer.Write((byte)0xB1); // <--|
            buffer.Write((byte)0x20); //    | Unknown, changes between packets
            buffer.Write((byte)0x18); //    |
            buffer.Write((byte)0x00); // <--|

            buffer.Write((float)p.X);
            buffer.Write((float)p.Y);

            buffer.Write((byte)0x00); // 0.0f
            buffer.Write((byte)0x00); // Probably a float, see SpawnMonster
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);

            buffer.Write((byte)0xFF); // 1.0f
            buffer.Write((byte)0xFF); // Probably a float, see SpawnMonster
            buffer.Write((byte)0x7F);
            buffer.Write((byte)0x3F);
        }
    }
}