using ENet;

namespace LeagueSandbox.GameServer.Logic.Chatbox
{
    public abstract class ChatCommand
    {
        internal ChatCommandManager _owner;

        public string Command { get; set; }
        public string Syntax { get; set; }
        public bool IsHidden { get; set; }
        public bool IsDisabled { get; set; }

        public ChatCommand(string command, string syntax, ChatCommandManager owner)
        {
            Command = command;
            Syntax = syntax;
            _owner = owner;
        }

        public abstract void Execute(Peer peer, bool hasReceivedArguments, string arguments = "");

        public void ShowSyntax()
        {
            _owner.SendDebugMsgFormatted(ChatCommandManager.DebugMsgType.SYNTAX, _owner.CommandStarterCharacter + Syntax);
        }
    }
}
