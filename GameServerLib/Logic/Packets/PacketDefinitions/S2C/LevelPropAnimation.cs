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
            : base(PacketCmd.PKT_S2_C_LEVEL_PROP_ANIMATION)
        {
            _buffer.Write(Encoding.Default.GetBytes(animationName));
            _buffer.Fill(0, 64 - animationName.Length);

            _buffer.Write((float)unk1);
            _buffer.Write((float)animationTime);

            _buffer.Write((uint)lp.NetId);

            _buffer.Write((int)unk2);
            _buffer.Write((int)unk3);

            byte delete = 0x00;
            if (deletePropAfterAnimationFinishes)
            {
                delete = 0x01;
            }
            _buffer.Write((byte)delete); // Most likely deletes prop after animation ends when set to 1
            _buffer.Write((byte)0x00);
            _buffer.Write((byte)0x00);
        }
    }
}