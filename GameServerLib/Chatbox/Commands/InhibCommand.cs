using ENet;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.Players;

namespace LeagueSandbox.GameServer.Chatbox.Commands
{
    public class InhibCommand : ChatCommandBase
    {
        private readonly PlayerManager _playerManager;

        public override string Command => "inhib";
        public override string Syntax => $"{Command}";

        public InhibCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _playerManager = game.PlayerManager;
        }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            var sender = _playerManager.GetPeerInfo(peer);
            var min = new Monster(
                Game,
                sender.Champion.Position,
                sender.Champion.Position,
                "Worm",
                "Worm"
                );
            Game.ObjectManager.AddObject(min);
        }
    }
}
