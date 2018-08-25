using GameServerCore.Enums;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class SetScreenTint : BasePacket
    {
        public SetScreenTint(TeamId team, bool enable, float transitionTime, byte red, byte green, byte blue, float alpha)
            : base(PacketCmd.PKT_S2C_SET_SCREEN_TINT)
        {
            Write(enable);
            Write(transitionTime); // Transition time in seconds
            Write((int)team);
            Write(blue);
            Write(green);
            Write(red);
            Write((byte)0xFF); // Unk
            Write(alpha);
        }
    }
}