using System.Text;
using LeagueSandbox.GameServer.Logic.GameObjects.Other;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SpawnPlaceable : Packet
    {
        public SpawnPlaceable(Placeable p)
            : base(PacketCmd.PKT_S2C_OBJECT_SPAWN)
        {

            _buffer.Write(p.NetId);

            _buffer.Write((byte)0xB5);
            _buffer.Write((byte)0x00);
            _buffer.Write((byte)0xB3);
            _buffer.Write((byte)0x00);
            _buffer.Write((byte)0x7C);

            _buffer.Write(p.NetId);
            _buffer.Write(p.NetId);
            _buffer.Write(p.Owner.NetId);

            _buffer.Write((byte)0x40);

            _buffer.Write(p.X); //x
            _buffer.Write(p.GetZ()); //z
            _buffer.Write(p.Y); //y

            _buffer.Fill(0, 8);

            _buffer.Write((short)p.Team);
            _buffer.Write((byte)0x92);
            _buffer.Write((byte)0x00);

            _buffer.Write((byte)0x00);
            _buffer.Write((byte)0x00);
            _buffer.Write((byte)0x02);
            _buffer.Write((byte)0x00);
            _buffer.Write((byte)0x00);
            _buffer.Write((byte)0x00);
            _buffer.Write((byte)0x00);

            _buffer.Write(Encoding.Default.GetBytes(p.Name));
            _buffer.Fill(0, 64 - p.Name.Length);

            _buffer.Write(Encoding.Default.GetBytes(p.Model));
            _buffer.Fill(0, 64 - p.Model.Length);

            _buffer.Write((byte)0x01);

            _buffer.Fill(0, 16);

            _buffer.Write(1.0f); // Unk

            _buffer.Fill(0, 13);

            _buffer.Write((byte)0x03);

            _buffer.Write((byte)0xB1); // <--|
            _buffer.Write((byte)0x20); //    | Unknown, changes between packets
            _buffer.Write((byte)0x18); //    |
            _buffer.Write((byte)0x00); // <--|

            _buffer.Write(p.X);
            _buffer.Write(p.Y);

            _buffer.Write((byte)0x00); // 0.0f
            _buffer.Write((byte)0x00); // Probably a float, see SpawnMonster
            _buffer.Write((byte)0x00);
            _buffer.Write((byte)0x00);

            _buffer.Write((byte)0xFF); // 1.0f
            _buffer.Write((byte)0xFF); // Probably a float, see SpawnMonster
            _buffer.Write((byte)0x7F);
            _buffer.Write((byte)0x3F);
        }
    }
}