using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using static LeagueSandbox.GameServer.Logic.Chatbox.ChatCommandManager;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class HelpCommand : ChatCommand
    {
        public HelpCommand(string command, string syntax, ChatCommandManager owner) : base(command, syntax, owner) { }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            var _game = Program.ResolveDependency<Game>();
            if (!_game.Config.ChatCheatsEnabled)
            {
                _owner.SendDebugMsgFormatted(DebugMsgType.INFO, "Chat commands are disabled in this game.");
                return;
            }

            var _chatboxManager = Program.ResolveDependency<ChatCommandManager>();

            var commands = "";
            var count = 0;
            foreach (var command in _owner.GetCommandsStrings())
            {
                count++;
                commands = commands
                           + "<font color =\"#E175FF\"><b>"
                           + _chatboxManager.CommandStarterCharacter + command
                           + "</b><font color =\"#FFB145\">, ";
            }

            _owner.SendDebugMsgFormatted(DebugMsgType.INFO, "List of available commands: ");
            _owner.SendDebugMsgFormatted(DebugMsgType.INFO, commands);
            _owner.SendDebugMsgFormatted(DebugMsgType.INFO, "There are " + count + " commands");

            _owner.AddCommand(new NewCommand("newcommand", "", _owner));
        }
    }
}
