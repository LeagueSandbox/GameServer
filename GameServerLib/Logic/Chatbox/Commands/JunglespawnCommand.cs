using ENet;
using LeagueSandbox.GameServer.Logic.Logging;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class JunglespawnCommand : ChatCommandBase
    {
        private readonly ILogger _logger;

        public override string Command => "junglespawn";
        public override string Syntax => $"{Command}";

        public JunglespawnCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _logger = LoggerProvider.GetLogger();
        }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            _logger.Info($"{ChatCommandManager.CommandStarterCharacter}{Command} command not implemented");
            ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.INFO, "Command not implemented");
        }
    }
}
