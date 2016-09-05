using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using LeagueSandbox.GameServer.Logic.Packets;
using static LeagueSandbox.GameServer.Logic.Chatbox.ChatboxManager;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class HelpCommand : ChatCommand
    {
        public HelpCommand(string command, string syntax, ChatboxManager owner) : base(command, syntax, owner) { }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            ChatboxManager _chatboxManager = Program.ResolveDependency<ChatboxManager>();

            string commands = "";
            int count = 0;
            foreach (var command in _owner.GetCommandsStrings())
            {
                count += 1;
                commands = commands
                           + "<font color =\"#E175FF\"><b>"
                           + _chatboxManager.CommandStarterCharacter + command
                           + "</b><font color =\"#FFB145\">, ";
            }
            _owner.SendDebugMsgFormatted(DebugMsgType.INFO, "List of available commands: ");
            _owner.SendDebugMsgFormatted(DebugMsgType.INFO, commands);
            _owner.SendDebugMsgFormatted(DebugMsgType.INFO, "There are " + count.ToString() + " commands");

            _owner.AddCommand(new NewCommand("newcommand", "", _owner));
        }
    }
}
