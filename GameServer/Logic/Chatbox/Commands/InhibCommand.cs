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
            Game game = Program.ResolveDependency<Game>();
            PlayerManager playerManager = Program.ResolveDependency<PlayerManager>();
            NetworkIdManager networkIdManager = Program.ResolveDependency<NetworkIdManager>();

            var sender = playerManager.GetPeerInfo(peer);
            var min = new Monster(
                sender.Champion.X,
                sender.Champion.Y,
                sender.Champion.X,
                sender.Champion.Y,
                "Worm",
                "Worm"
                );
            game.Map.AddObject(min);
        }
    }
}
