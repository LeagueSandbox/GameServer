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
            _buffer.Write((float)transitionTime); // Transition time in seconds
            _buffer.Write((int)team);
            _buffer.Write((byte)blue);
            _buffer.Write((byte)green);
            _buffer.Write((byte)red);
            _buffer.Write((byte)0xFF); // Unk
            _buffer.Write((float)alpha);
        }
    }
}