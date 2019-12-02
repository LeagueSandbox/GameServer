namespace LeagueSandbox.GameServer.Chatbox.Commands
{
    public class ReloadScriptsCommand : ChatCommandBase
    {
        public override string Command => "reloadscripts";
        public override string Syntax => $"{Command}";

        public ReloadScriptsCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.INFO,
                Game.LoadScripts() ? "Scripts reloaded." : "Scripts failed to reload.");
        }
    }
}
