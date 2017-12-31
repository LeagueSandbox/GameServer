using System.Text;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SpawnMonster : Packet
    {
        public SpawnMonster(Monster m)
            : base(PacketCmd.PKT_S2C_ObjectSpawn)
        {
            buffer.Write(m.NetId);
            buffer.Write((short)345);
            buffer.Write((short)343);

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

            buffer.fill(0, 64); // empty

            buffer.Write((int)m.Team); // Probably a short
            buffer.fill(0, 12);
            buffer.Write((int)1); //campId 1
            buffer.Write((int)100);
            buffer.Write((int)74);
            buffer.Write((long)1);
            buffer.Write((float)115.0066f);
            buffer.Write((byte)0);

            buffer.fill(0, 11);
            buffer.Write((float)1.0f); // Unk
            buffer.fill(0, 13);
            buffer.Write((byte)3); //type 3=champ/jungle; 2=minion
            buffer.Write((int)13337);
            buffer.Write((float)m.X); //x
            buffer.Write((float)m.Y); //y
            buffer.Write((float)-0.8589599f); // rotation1 from -1 to 1
            buffer.Write((float)0.5120428f); //rotation2 from -1 to 1
        }
    }
}