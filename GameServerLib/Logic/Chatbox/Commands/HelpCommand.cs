using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using static LeagueSandbox.GameServer.Logic.Chatbox.ChatCommandManager;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class HelpCommand : ChatCommandBase
    {
        private readonly Game _game;

        public override string Command => "help";
        public override string Syntax => "";

        public HelpCommand(ChatCommandManager chatCommandManager, Game game) : base(chatCommandManager)
        {
            _game = game;
        }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            if (!_game.Config.ChatCheatsEnabled)
            {
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.INFO, "Chat commands are disabled in this game.");
                return;
            }

            var commands = "";
            var count = 0;
            foreach (var command in ChatCommandManager.GetCommandsStrings())
            {
                count++;
                commands = commands
                           + "<font color =\"#E175FF\"><b>"
                           + ChatCommandManager.CommandStarterCharacter + command
                           + "</b><font color =\"#FFB145\">, ";
            }

            ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.INFO, "List of available commands: ");
            ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.INFO, commands);
            ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.INFO, "There are " + count + " commands");
        }
    }
}
