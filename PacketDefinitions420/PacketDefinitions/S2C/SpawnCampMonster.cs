using GameServerCore.Content;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class SpawnCampMonster : BasePacket
    {
        public SpawnCampMonster(INavGrid navGrid, IMonster m)
            : base(PacketCmd.PKT_S2C_OBJECT_SPAWN, m.NetId)
        {
            Write((byte)0x79);
            Write((byte)0x01);
            Write((byte)0x77);
            Write((byte)0x01);

            Write((byte)0x63); // 0x63 (99) for jungle monster, 3 for minion
            WriteNetId(m);
            WriteNetId(m);
            Write((byte)0x40);
            Write(m.X); //x
            Write(m.GetZ()); //z
            Write(m.Y); //y
            Write(m.X); //x
            Write(m.GetZ()); //z
            Write(m.Y); //y
            Write(m.Facing.X); //facing x
            Write(navGrid.GetHeightAtLocation(m.Facing.X, m.Facing.Y)); //facing z
            Write(m.Facing.Y); //facing y

			WriteConstLengthString(m.Name, 64);

			WriteConstLengthString(m.Model, 64);

			WriteConstLengthString(m.Name, 64);

			WriteConstLengthString(m.SpawnAnimation, 64);

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