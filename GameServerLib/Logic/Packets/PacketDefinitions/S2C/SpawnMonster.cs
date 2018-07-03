using System.Text;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SpawnMonster : Packet
    {
        public SpawnMonster(Monster m)
            : base(PacketCmd.PKT_S2C_OBJECT_SPAWN)
        {
            Write(m.NetId);
            Write((short)345);
            Write((short)343);

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

            Fill(0, 64); // empty

            Write((int)m.Team); // Probably a short
            Fill(0, 12);
            Write(1); //campId 1
            Write(100);
            Write(74);
            Write((long)1);
            Write(115.0066f);
            Write((byte)0);

            Fill(0, 11);
            Write(1.0f); // Unk
            Fill(0, 13);
            Write((byte)3); //type 3=champ/jungle; 2=minion
            Write(13337);
            Write(m.X); //x
            Write(m.Y); //y
            Write(-0.8589599f); // rotation1 from -1 to 1
            Write(0.5120428f); //rotation2 from -1 to 1
        }
    }
}