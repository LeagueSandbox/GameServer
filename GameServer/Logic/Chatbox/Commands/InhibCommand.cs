using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    class InhibCommand : ChatCommand
    {
        public InhibCommand(string command, string syntax, ChatboxManager owner) : base(command, syntax, owner) { }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            Game _game = Program.ResolveDependency<Game>();
            PlayerManager _playerManager = Program.ResolveDependency<PlayerManager>();
            NetworkIdManager _networkIdManager = Program.ResolveDependency<NetworkIdManager>();

            var sender = _playerManager.GetPeerInfo(peer);
            var min = new Monster(_networkIdManager.GetNewNetID(), sender.GetChampion().getX(), sender.GetChampion().getY(), sender.GetChampion().getX(), sender.GetChampion().getY(), "Worm", "Worm");//"AncientGolem", "AncientGolem1.1.1");
            _game.GetMap().AddObject(min);
        }
    }
}
