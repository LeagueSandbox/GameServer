using LeagueSandbox.GameServer.Logging;
using log4net;
using System;

namespace LeagueSandbox.GameServer.Chatbox.Commands
{
    public class JunglespawnCommand : ChatCommandBase
    {
        private static ILog _logger = LoggerProvider.GetLogger();
        private readonly Game _game;

        public override string Command => "junglespawn";
        public override string Syntax => $"{Command}";

        public JunglespawnCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _game = game;
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            try
            {
                _game.Map.MapScript.SpawnAllCamps();
            }
            catch(Exception e)
            {
                _logger.Error(null, e);
            }
            _logger.Info($"{ChatCommandManager.CommandStarterCharacter}{Command} Jungle Spawned!");
            ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.NORMAL, "Jungle Spawned!");
        }
    }
}
