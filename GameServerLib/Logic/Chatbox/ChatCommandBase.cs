using ENet;
using LeagueSandbox.GameServer.Logic.Interfaces;

namespace LeagueSandbox.GameServer.Logic.Chatbox
{
    public abstract class ChatCommandBase : IChatCommand
    {
        protected readonly ChatCommandManager ChatCommandManager;

        public abstract string Command { get; }
        public abstract string Syntax { get; }
        public bool IsHidden { get; set; }
        public bool IsDisabled { get; set; }

        protected ChatCommandBase(ChatCommandManager chatCommandManager)
        {
            ChatCommandManager = chatCommandManager;
        }

        public abstract void Execute(Peer peer, bool hasReceivedArguments, string arguments = "");

        public void ShowSyntax()
        {
            ChatCommandManager.SendDebugMsgFormatted(ChatCommandManager.DebugMsgType.SYNTAX, ChatCommandManager.CommandStarterCharacter + Syntax);
        }
    }
}
