using ENet;
using static LeagueSandbox.GameServer.Logic.Chatbox.ChatboxManager;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    class LevelCommand : ChatCommand
    {
        public LevelCommand(string command, string syntax, ChatboxManager owner) : base(command, syntax, owner) { }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            byte lvl;
            if (split.Length < 2)
            {
                _owner.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
            }
            else if (byte.TryParse(split[1], out lvl))
            {
                if (lvl < 1 || lvl > 18)
                    return;

                var experienceToLevelUp = _owner.GetGame().GetMap().GetExperienceToLevelUp()[lvl-1];
                _owner.GetGame().GetPeerInfo(peer).GetChampion().GetStats().Experience = experienceToLevelUp;
            }
        }
    }
}
