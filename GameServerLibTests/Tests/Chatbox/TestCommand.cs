using LeagueSandbox.GameServer;
using LeagueSandbox.GameServer.Chatbox;

namespace LeagueSandbox.GameServerTests.Tests.Chatbox
{
    public class TestCommand : ChatCommandBase
    {
        public override string Command { get; }
        public override string Syntax { get; }

        public TestCommand(Game game, ChatCommandManager chatCommandManager, string command, string syntax)
            : base(chatCommandManager, game)
        {
            Command = command;
            Syntax = syntax;
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {

        }
    }
}
