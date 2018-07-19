using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class PauseGame : BasePacket
    {
        public PauseGame(Game game, int seconds, bool showWindow)
            : base(game, PacketCmd.PKT_PAUSE_GAME)
        {
            // The following structure might be incomplete or wrong
            Write(0);
            Write(seconds);
            Write(showWindow);
        }
    }
}