using System.Text;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SpawnCampMonster : BasePacket
    {
        public SpawnCampMonster(Monster m)
            : base(PacketCmd.PKT_S2C_OBJECT_SPAWN, m.NetId)
        {
            Write((byte)0x79);
            Write((byte)0x01);
            Write((byte)0x77);
            Write((byte)0x01);

            Write((byte)0x63); // 0x63 (99) for jungle monster, 3 for minion
            Write(m.NetId);
            Write(m.NetId);
            Write((byte)0x40);
            Write(m.X); //x
            Write(m.GetZ()); //z
            Write(m.Y); //y
            Write(m.X); //x
            Write(m.GetZ()); //z
            Write(m.Y); //y
            Write(m.Facing.X); //facing x
            Write(Game.Map.NavGrid.GetHeightAtLocation(m.Facing.X, m.Facing.Y)); //facing z
            Write(m.Facing.Y); //facing y

            Write(Encoding.Default.GetBytes(m.Name));
            Fill(0, 64 - m.Name.Length);

            Write(Encoding.Default.GetBytes(m.Model));
            Fill(0, 64 - m.Model.Length);

            Write(Encoding.Default.GetBytes(m.Name));
            Fill(0, 64 - m.Name.Length);

            Write(Encoding.Default.GetBytes(m.SpawnAnimation));
            Fill(0, 64 - m.SpawnAnimation.Length);

            Write((int)m.Team); // Probably a short
            Fill(0, 12); // Unk
            Write((int)m.CampId); // Camp id. Camp needs to exist
            Write(0); // Unk
            Write((int)m.CampUnk);
            Write(1); // Unk
            Write(m.SpawnAnimationTime); // After this many seconds, the camp icon appears in the minimap
            Write(1191.533936f); // Unk
            Write(1); // Unk
            Fill(0, 40); // Unk
            Write(1.0f); // Unk
            Fill(0, 13); // Unk
            Write((byte)3); //type 3=champ/jungle; 2=minion
            Write((byte)0xF1); //<-|
            Write((byte)0xFB); //  |-> Unk
            Write((byte)0x27); //  |
            Write((byte)0x00); //<-|
            Write(m.X); //x
            Write(m.Y); //y
            Write(-0.8589599f); // rotation1 from -1 to 1
            Write(0.5120428f); // rotation2 from -1 to 1
        }
    }
}