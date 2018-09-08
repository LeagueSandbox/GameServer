namespace LeagueSandbox.GameServer.Chatbox
{
    public interface IChatCommand
    {
        string Command { get; }
        string Syntax { get; }
        void Execute(int userId, bool hasReceivedArguments, string arguments = "");
        void ShowSyntax();
    }
}
