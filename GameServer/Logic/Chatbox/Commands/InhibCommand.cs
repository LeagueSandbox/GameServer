using ENet;
using LeagueSandbox.GameServer.Logic.GameObjects;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    class InhibCommand : ChatCommand
    {
        public InhibCommand(string command, string syntax, ChatboxManager owner) : base(command, syntax, owner) { }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            var sender = _owner.GetGame().GetPeerInfo(peer);
            var min = new Monster(_owner.GetGame(), _owner.GetGame().GetNewNetID(), sender.GetChampion().getX(), sender.GetChampion().getY(), sender.GetChampion().getX(), sender.GetChampion().getY(), "Worm", "Worm");//"AncientGolem", "AncientGolem1.1.1");
            _owner.GetGame().GetMap().AddObject(min);
        }
    }
}
