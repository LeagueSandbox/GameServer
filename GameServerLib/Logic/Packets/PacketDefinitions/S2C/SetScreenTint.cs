using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SetScreenTint : BasePacket
    {
        public SetScreenTint(TeamId team, bool enable, float transitionTime, byte red, byte green, byte blue, float alpha)
            : base(PacketCmd.PKT_S2_C_SET_SCREEN_TINT)
        {
            _buffer.Write(enable);
            _buffer.Write(transitionTime); // Transition time in seconds
            _buffer.Write((int)team);
            _buffer.Write(blue);
            _buffer.Write(green);
            _buffer.Write(red);
            _buffer.Write((byte)0xFF); // Unk
            _buffer.Write(alpha);
        }
    }
}