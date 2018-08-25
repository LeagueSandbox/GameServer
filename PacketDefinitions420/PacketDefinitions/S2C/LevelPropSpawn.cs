using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class LevelPropSpawn : BasePacket
    {
        public LevelPropSpawn(ILevelProp lp)
            : base(PacketCmd.PKT_S2C_LEVEL_PROP_SPAWN)
        {
            WriteNetId(lp);
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

			WriteConstLengthString(lp.Name, 64);
			WriteConstLengthString(lp.Model, 64);
        }
    }
}