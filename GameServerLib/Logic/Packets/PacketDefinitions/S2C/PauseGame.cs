using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class PauseGame : BasePacket
    {
        public PauseGame(int seconds, bool showWindow) 
            : base(PacketCmd.PKT_PAUSE_GAME)
        {
            // The following structure might be incomplete or wrong
            _buffer.Write(0);
            _buffer.Write(seconds);
            _buffer.Write(showWindow);
        }
    }
}