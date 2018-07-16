using ENet;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class KillCommand : ChatCommandBase
    {
        private readonly PlayerManager _playerManager;

        public override string Command => "kill";
        public override string Syntax => $"{Command} minions";

        public KillCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _playerManager = game.GetPlayerManager();
        }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');

            if (split.Length < 2)
            {
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
            }
            else if (split[1] == "minions")
            {
                var objects = Game.ObjectManager.GetObjects();
                foreach (var o in objects)
                {
                    if (o.Value is Minion minion)
                    {
                        minion.Die(_playerManager.GetPeerInfo(peer).Champion); // :(
                    }
                }
            }
            else
            {
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
            }
        }
    }
}
