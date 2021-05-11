using GameServerCore.Domain;

namespace LeagueSandbox.GameServer.Chatbox
{
    public interface IChatCommand : IUpdate
    {
        string Command { get; }
        string Syntax { get; }
        void Execute(int userId, bool hasReceivedArguments, string arguments = "");
        void ShowSyntax();
    }
}
