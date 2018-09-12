using LeagueSandbox.GameServer.Logging;
using log4net;

namespace LeagueSandbox.GameServer.Chatbox.Commands
{
    public class JunglespawnCommand : ChatCommandBase
    {
        private readonly ILog _logger;

        public override string Command => "junglespawn";
        public override string Syntax => $"{Command}";

        public JunglespawnCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _logger = LoggerProvider.GetLogger();
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            _logger.Warn($"{ChatCommandManager.CommandStarterCharacter}{Command} command not implemented");
            ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.INFO, "Command not implemented");
        }
    }
}
