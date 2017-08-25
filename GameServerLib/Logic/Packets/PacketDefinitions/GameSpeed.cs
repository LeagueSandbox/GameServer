using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets
{
    public class GameSpeed : BasePacket
    {
        public GameSpeed(float gameSpeed) : base(PacketCmd.PKT_S2C_GameSpeed)
        {
            buffer.Write((float)gameSpeed);
        }
    }
}