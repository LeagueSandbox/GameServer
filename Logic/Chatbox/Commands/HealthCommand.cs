using ENet;
using static LeagueSandbox.GameServer.Logic.Chatbox.ChatboxManager;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    class HealthCommand : ChatCommand
    {
        public HealthCommand(string command, string syntax, ChatboxManager owner) : base(command, syntax, owner) { }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            float hp;
            if (split.Length < 2)
            {
                _owner.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
            }
            else if (float.TryParse(split[1], out hp))
            {
                _owner.GetGame().GetPeerInfo(peer).GetChampion().GetStats().HealthPoints.FlatBonus = hp;
                _owner.GetGame().GetPeerInfo(peer).GetChampion().GetStats().CurrentHealth = hp;
            }
        }
    }
}
