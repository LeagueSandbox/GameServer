using ENet;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class CoordsCommand : ChatCommandBase
    {
        private readonly Logger _logger;
        private readonly PlayerManager _playerManager;

        public override string Command => "coords";
        public override string Syntax => $"{Command}";

        public CoordsCommand(ChatCommandManager chatCommandManager, Logger logger, PlayerManager playerManager)
            : base(chatCommandManager)
        {
            _logger = logger;
            _playerManager = playerManager;
        }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            var champion = _playerManager.GetPeerInfo(peer).Champion;
            _logger.LogCoreInfo($"At {champion.X}; {champion.Y}");
            var msg = $"At Coords - X: {champion.X} Y: {champion.Y} Z: {champion.GetZ()}";
            ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.NORMAL, msg);
        }
    }
}
