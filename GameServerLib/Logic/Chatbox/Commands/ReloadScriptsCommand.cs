using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Scripting.CSharp;
using static LeagueSandbox.GameServer.Logic.Chatbox.ChatCommandManager;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class ReloadScriptsCommand : ChatCommand
    {
        public ReloadScriptsCommand(string command, string syntax, ChatCommandManager owner) : base(command, syntax, owner) { }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            var game = Program.ResolveDependency<Game>();
            if (game.LoadScripts())
            {
                _owner.SendDebugMsgFormatted(DebugMsgType.INFO, "Scripts reloaded.");
            } else
            {
                _owner.SendDebugMsgFormatted(DebugMsgType.INFO, "Scripts failed to reload.");
            }
        }
    }
}
