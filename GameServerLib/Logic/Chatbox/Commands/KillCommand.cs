using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Players;
using static LeagueSandbox.GameServer.Logic.Chatbox.ChatCommandManager;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class KillCommand : ChatCommandBase
    {
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public override string Command => "kill";
        public override string Syntax => $"{Command} minions";

        public KillCommand(ChatCommandManager chatCommandManager, Game game, PlayerManager playerManager) 
            : base(chatCommandManager)
        {
            _game = game;
            _playerManager = playerManager;
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
                var objects = _game.ObjectManager.GetObjects();
                foreach (var o in objects)
                {
                    if (o.Value is Minion)
                    {
                        (o.Value as Minion).Die(_playerManager.GetPeerInfo(peer).Champion); // :(
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
