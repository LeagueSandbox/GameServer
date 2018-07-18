using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class CloseGame : BasePacket
    {
        public CloseGame(Game game)
            : base(game, PacketCmd.PKT_S2C_CLOSE_GAME)
        {
        }
    }
}