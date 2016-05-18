using ENet;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using System;
using System.Text;
using static LeagueSandbox.GameServer.Logic.Chatbox.ChatboxManager;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    class NewCommand : ChatCommand
    {
        public NewCommand(string command, string syntax, ChatboxManager owner) : base(command, syntax, owner) { }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            _owner.SendDebugMsgFormatted(DebugMsgType.INFO, "The new command added by " + _owner.CommandStarterCharacter + "help has been executed");
            _owner.RemoveCommand(Command);
        }
    }
}
