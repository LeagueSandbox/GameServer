using System.Text;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SpawnMonster : Packet
    {
        public SpawnMonster(Monster m)
            : base(PacketCmd.PKT_S2_C_OBJECT_SPAWN)
        {
            _buffer.Write(m.NetId);
            _buffer.Write((short)345);
            _buffer.Write((short)343);

            _buffer.Write((byte)0x63); // 0x63 (99) for jungle monster, 3 for minion
            _buffer.Write(m.NetId);
            _buffer.Write(m.NetId);
            _buffer.Write((byte)0x40);
            _buffer.Write((float)m.X); //x
            _buffer.Write((float)m.GetZ()); //z
            _buffer.Write((float)m.Y); //y
            _buffer.Write((float)m.X); //x
            _buffer.Write((float)m.GetZ()); //z
            _buffer.Write((float)m.Y); //y
            _buffer.Write((float)m.Facing.X); //facing x
            _buffer.Write((float)Game.Map.NavGrid.GetHeightAtLocation(m.Facing.X, m.Facing.Y)); //facing z
            _buffer.Write((float)m.Facing.Y); //facing y

            _buffer.Write(Encoding.Default.GetBytes(m.Name));
            _buffer.Fill(0, 64 - m.Name.Length);

            _buffer.Write(Encoding.Default.GetBytes(m.Model));
            _buffer.Fill(0, 64 - m.Model.Length);

            _buffer.Write(Encoding.Default.GetBytes(m.Name));
            _buffer.Fill(0, 64 - m.Name.Length);

            _buffer.Fill(0, 64); // empty

            _buffer.Write((int)m.Team); // Probably a short
            _buffer.Fill(0, 12);
            _buffer.Write((int)1); //campId 1
            _buffer.Write((int)100);
            _buffer.Write((int)74);
            _buffer.Write((long)1);
            _buffer.Write((float)115.0066f);
            _buffer.Write((byte)0);

            _buffer.Fill(0, 11);
            _buffer.Write((float)1.0f); // Unk
            _buffer.Fill(0, 13);
            _buffer.Write((byte)3); //type 3=champ/jungle; 2=minion
            _buffer.Write((int)13337);
            _buffer.Write((float)m.X); //x
            _buffer.Write((float)m.Y); //y
            _buffer.Write((float)-0.8589599f); // rotation1 from -1 to 1
            _buffer.Write((float)0.5120428f); //rotation2 from -1 to 1
        }
    }
}