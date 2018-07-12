using ENet;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class InhibCommand : ChatCommandBase
    {

        public override string Command => "inhib";
        public override string Syntax => $"{Command}";

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            var sender = PlayerManager.GetPeerInfo(peer);
            var min = new Monster(
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
