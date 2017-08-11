using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Scripting.CSharp;
using static LeagueSandbox.GameServer.Logic.Chatbox.ChatCommandManager;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class ReloadScriptsCommand : ChatCommandBase
    {
        private readonly Game _game;

        public override string Command => "reloadscripts";
        public override string Syntax => "";

        public ReloadScriptsCommand(ChatCommandManager chatCommandManager, Game game) : base(chatCommandManager)
        {
            _game = game;
        }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            if (_game.LoadScripts())
            {
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.INFO, "Scripts reloaded.");
            }
            else
            {
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.INFO, "Scripts failed to reload.");
            }
        }
    }
}
