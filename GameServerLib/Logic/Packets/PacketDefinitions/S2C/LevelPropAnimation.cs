using System.Text;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class LevelPropAnimation : BasePacket
    {
        public LevelPropAnimation(LevelProp lp,
            string animationName,
            float unk1 = 0.0f,
            float animationTime = 0.0f,
            int unk2 = 1,
            int unk3 = 1,
            bool deletePropAfterAnimationFinishes = false)
            : base(PacketCmd.PKT_S2C_LevelPropAnimation)
        {
            buffer.Write(Encoding.Default.GetBytes(animationName));
            buffer.fill(0, 64 - animationName.Length);

            buffer.Write((float)unk1);
            buffer.Write((float)animationTime);

            buffer.Write((uint)lp.NetId);

            buffer.Write((int)unk2);
            buffer.Write((int)unk3);

            byte delete = 0x00;
            if (deletePropAfterAnimationFinishes)
            {
                delete = 0x01;
            }
            buffer.Write((byte)delete); // Most likely deletes prop after animation ends when set to 1
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
        }
    }
}