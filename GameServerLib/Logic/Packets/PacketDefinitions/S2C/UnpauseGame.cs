using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class UnpauseGame : BasePacket
    {
        public UnpauseGame(Game game, uint unpauserNetId, bool showWindow)
            : base(game, PacketCmd.PKT_UNPAUSE_GAME)
        {
            Write(unpauserNetId);
            Write(showWindow);
        }
    }
}