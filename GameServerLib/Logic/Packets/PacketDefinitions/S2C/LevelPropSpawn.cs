using System.Text;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class LevelPropSpawn : BasePacket
    {
        public LevelPropSpawn(LevelProp lp)
            : base(PacketCmd.PKT_S2C_LEVEL_PROP_SPAWN)
        {
            Write((int)lp.NetId);
            Write((byte)0x40); // unk
            Write(lp.SkinId);
            Write((byte)0);
            Write((byte)0);
            Write((byte)0); // Unk
            Write(lp.X);
            Write(lp.Z);
            Write(lp.Y);
            Write(0.0f); // Rotation Y

            Write(lp.DirX);
            Write(lp.DirZ);
            Write(lp.DirY);
            Write(lp.Unk1);
            Write(lp.Unk2);

            Write(1.0f);
            Write(1.0f);
            Write(1.0f); // Scaling
            Write((int)lp.Team); // Probably a short
            Write(2); // nPropType [size 1 . 4] (4.18) -- if is a prop, become unselectable and use direction params

            foreach (var b in Encoding.Default.GetBytes(lp.Name))
                Write(b);
            Fill(0, 64 - lp.Name.Length);
            foreach (var b in Encoding.Default.GetBytes(lp.Model))
                Write(b);
            Fill(0, 64 - lp.Model.Length);
        }
    }
}