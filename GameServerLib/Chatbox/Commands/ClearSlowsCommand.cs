using LeagueSandbox.GameServer.Players;

namespace LeagueSandbox.GameServer.Chatbox.Commands
{
    public class ClearSlowCommand : ChatCommandBase
    {
        private readonly PlayerManager _playerManager;

        public override string Command => "clearslows";
        public override string Syntax => $"{Command}";

        public ClearSlowCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _playerManager = game.PlayerManager;
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            _playerManager.GetPeerInfo(userId).Champion.Stats.ClearSlows();
            ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.INFO, "Your slows have been cleared!");
        }
    }
}
