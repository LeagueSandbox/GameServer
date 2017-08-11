using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using static LeagueSandbox.GameServer.Logic.Chatbox.ChatCommandManager;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class JunglespawnCommand : ChatCommandBase
    {
        private readonly Logger _logger;

        public override string Command => "junglespawn";
        public override string Syntax => "";

        public JunglespawnCommand(ChatCommandManager chatCommandManager, Logger logger) : base(chatCommandManager)
        {
            _logger = logger;
        }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            _logger.LogCoreInfo($"{ChatCommandManager.CommandStarterCharacter}{Command} command not implemented");
            ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.INFO, "Command not implemented");
        }
    }
}
