using ENet;

namespace LeagueSandbox.GameServer.Logic.Chatbox
{
    public interface IChatCommand
    {
        string Command { get; }
        string Syntax { get; }
        void Execute(Peer peer, bool hasReceivedArguments, string arguments = "");
        void ShowSyntax();
    }
}
