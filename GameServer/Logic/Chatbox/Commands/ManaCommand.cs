using ENet;
using static LeagueSandbox.GameServer.Logic.Chatbox.ChatboxManager;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    class ManaCommand : ChatCommand
    {
        public ManaCommand(string command, string syntax, ChatboxManager owner) : base(command, syntax, owner) { }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            float mp;
            if (split.Length < 2)
            {
                _owner.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
            }
            else if (float.TryParse(split[1], out mp))
            {
                _owner.GetGame().GetPeerInfo(peer).GetChampion().GetStats().ManaPoints.FlatBonus = mp;
                _owner.GetGame().GetPeerInfo(peer).GetChampion().GetStats().CurrentMana = mp;
            }
        }
    }
}
