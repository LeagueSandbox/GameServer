namespace LeagueSandbox.GameServer.Chatbox.Commands
{
    public class NewCommand : ChatCommandBase
    {
        public override string Command => "newcommand";
        public override string Syntax => $"{Command}";

        public NewCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            var msg = $"The new command added by {ChatCommandManager.CommandStarterCharacter}help has been executed";
            ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.INFO, msg);
            ChatCommandManager.RemoveCommand(Command);
        }
    }
}
