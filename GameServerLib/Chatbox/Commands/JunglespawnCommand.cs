using LeagueSandbox.GameServer.Logging;
using log4net;

namespace LeagueSandbox.GameServer.Chatbox.Commands
{
    public class JunglespawnCommand : ChatCommandBase
    {
        private readonly ILog _logger;
        private readonly Game _game;

        public override string Command => "junglespawn";
        public override string Syntax => $"{Command}";

        public JunglespawnCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _logger = LoggerProvider.GetLogger();
            _game = game;
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            _game.Map.MapScript.SpawnAllCamps();
            _logger.Info($"{ChatCommandManager.CommandStarterCharacter}{Command} Jungle Spawned!");
            ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.NORMAL, "Jungle Spawned!");
        }
    }
}
