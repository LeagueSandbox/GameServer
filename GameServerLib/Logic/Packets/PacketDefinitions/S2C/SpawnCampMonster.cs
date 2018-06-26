using System.Text;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SpawnCampMonster : BasePacket
    {
        public SpawnCampMonster(Monster m)
            : base(PacketCmd.PKT_S2_C_OBJECT_SPAWN, m.NetId)
        {
            _buffer.Write((byte)0x79);
            _buffer.Write((byte)0x01);
            _buffer.Write((byte)0x77);
            _buffer.Write((byte)0x01);

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

            _buffer.Write(Encoding.Default.GetBytes(m.SpawnAnimation));
            _buffer.Fill(0, 64 - m.SpawnAnimation.Length);

            _buffer.Write((int)m.Team); // Probably a short
            _buffer.Fill(0, 12); // Unk
            _buffer.Write((int)m.CampId); // Camp id. Camp needs to exist
            _buffer.Write((int)0); // Unk
            _buffer.Write((int)m.CampUnk);
            _buffer.Write((int)1); // Unk
            _buffer.Write((float)m.SpawnAnimationTime); // After this many seconds, the camp icon appears in the minimap
            _buffer.Write((float)1191.533936f); // Unk
            _buffer.Write((int)1); // Unk
            _buffer.Fill(0, 40); // Unk
            _buffer.Write((float)1.0f); // Unk
            _buffer.Fill(0, 13); // Unk
            _buffer.Write((byte)3); //type 3=champ/jungle; 2=minion
            _buffer.Write((byte)0xF1); //<-|
            _buffer.Write((byte)0xFB); //  |-> Unk
            _buffer.Write((byte)0x27); //  |
            _buffer.Write((byte)0x00); //<-|
            _buffer.Write((float)m.X); //x
            _buffer.Write((float)m.Y); //y
            _buffer.Write((float)-0.8589599f); // rotation1 from -1 to 1
            _buffer.Write((float)0.5120428f); // rotation2 from -1 to 1
        }
    }
}