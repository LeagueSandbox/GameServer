using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using static LeagueSandbox.GameServer.Logic.Chatbox.ChatboxManager;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    class SpawnStateCommand : ChatCommand
    {
        public SpawnStateCommand(string command, string syntax, ChatboxManager owner) : base(command, syntax, owner) { }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            Game _game = Program.ResolveDependency<Game>();

            var split = arguments.ToLower().Split(' ');

            if (split.Length < 2)
            {
                _owner.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
            }
            else if (split[1] == "1")
            {
                _game.Map.SetSpawnState(true);
            }
            else if (split[1] == "0")
            {
                _game.Map.SetSpawnState(false);
            }
            else
            {
                _owner.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
            }
        }
    }
}
