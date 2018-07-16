using ENet;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class InhibCommand : ChatCommandBase
    {
        private readonly PlayerManager _playerManager;

        public override string Command => "inhib";
        public override string Syntax => $"{Command}";

        public InhibCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _playerManager = game.GetPlayerManager();
        }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            var sender = _playerManager.GetPeerInfo(peer);
            var min = new Monster(
                Game,
                sender.Champion.X,
                sender.Champion.Y,
                sender.Champion.X,
                sender.Champion.Y,
                "Worm",
                "Worm"
                );
            Game.ObjectManager.AddObject(min);
        }
    }
}
