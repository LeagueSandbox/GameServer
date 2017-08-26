using System.Text;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SpawnCampMonster : BasePacket
    {
        public SpawnCampMonster(Monster m)
            : base(PacketCmd.PKT_S2C_ObjectSpawn, m.NetId)
        {
            buffer.Write((byte)0x79);
            buffer.Write((byte)0x01);
            buffer.Write((byte)0x77);
            buffer.Write((byte)0x01);

            buffer.Write((byte)0x63); // 0x63 (99) for jungle monster, 3 for minion
            buffer.Write(m.NetId);
            buffer.Write(m.NetId);
            buffer.Write((byte)0x40);
            buffer.Write((float)m.X); //x
            buffer.Write((float)m.GetZ()); //z
            buffer.Write((float)m.Y); //y
            buffer.Write((float)m.X); //x
            buffer.Write((float)m.GetZ()); //z
            buffer.Write((float)m.Y); //y
            buffer.Write((float)m.Facing.X); //facing x
            buffer.Write((float)Game.Map.NavGrid.GetHeightAtLocation(m.Facing.X, m.Facing.Y)); //facing z
            buffer.Write((float)m.Facing.Y); //facing y

            buffer.Write(Encoding.Default.GetBytes(m.Name));
            buffer.fill(0, 64 - m.Name.Length);

            buffer.Write(Encoding.Default.GetBytes(m.Model));
            buffer.fill(0, 64 - m.Model.Length);

            buffer.Write(Encoding.Default.GetBytes(m.Name));
            buffer.fill(0, 64 - m.Name.Length);

            buffer.Write(Encoding.Default.GetBytes(m.SpawnAnimation));
            buffer.fill(0, 64 - m.SpawnAnimation.Length);

            buffer.Write((int)m.Team); // Probably a short
            buffer.fill(0, 12); // Unk
            buffer.Write((int)m.CampId); // Camp id. Camp needs to exist
            buffer.Write((int)0); // Unk
            buffer.Write((int)m.CampUnk);
            buffer.Write((int)1); // Unk
            buffer.Write((float)m.SpawnAnimationTime); // After this many seconds, the camp icon appears in the minimap
            buffer.Write((float)1191.533936f); // Unk
            buffer.Write((int)1); // Unk
            buffer.fill(0, 40); // Unk
            buffer.Write((float)1.0f); // Unk
            buffer.fill(0, 13); // Unk
            buffer.Write((byte)3); //type 3=champ/jungle; 2=minion
            buffer.Write((byte)0xF1); //<-|
            buffer.Write((byte)0xFB); //  |-> Unk
            buffer.Write((byte)0x27); //  |
            buffer.Write((byte)0x00); //<-|
            buffer.Write((float)m.X); //x
            buffer.Write((float)m.Y); //y
            buffer.Write((float)-0.8589599f); // rotation1 from -1 to 1
            buffer.Write((float)0.5120428f); // rotation2 from -1 to 1
        }
    }
}