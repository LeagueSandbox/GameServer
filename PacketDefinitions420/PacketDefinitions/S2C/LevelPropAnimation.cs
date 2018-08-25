using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class LevelPropAnimation : BasePacket
    {
        public LevelPropAnimation(
            ILevelProp lp,
            string animationName,
            float unk1 = 0.0f,
            float animationTime = 0.0f,
            int unk2 = 1,
            int unk3 = 1,
            bool deletePropAfterAnimationFinishes = false)
            : base(PacketCmd.PKT_S2C_LEVEL_PROP_ANIMATION)
        {
			WriteConstLengthString(animationName, 64);

            Write(unk1);
            Write(animationTime);

            WriteNetId(lp);

            Write(unk2);
            Write(unk3);

            byte delete = 0x00;
            if (deletePropAfterAnimationFinishes)
            {
                delete = 0x01;
            }
            Write(delete); // Most likely deletes prop after animation ends when set to 1
            Write((byte)0x00);
            Write((byte)0x00);
        }
    }
}