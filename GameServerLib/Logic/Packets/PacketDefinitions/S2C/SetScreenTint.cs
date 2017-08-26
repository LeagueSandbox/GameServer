using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SetScreenTint : BasePacket
    {
        public SetScreenTint(TeamId team, bool enable, float transitionTime, byte red, byte green, byte blue, float alpha)
            : base(PacketCmd.PKT_S2C_SetScreenTint)
        {
            buffer.Write(enable);
            buffer.Write((float)transitionTime); // Transition time in seconds
            buffer.Write((int)team);
            buffer.Write((byte)blue);
            buffer.Write((byte)green);
            buffer.Write((byte)red);
            buffer.Write((byte)0xFF); // Unk
            buffer.Write((float)alpha);
        }
    }
}