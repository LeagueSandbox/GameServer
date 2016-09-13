using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Players;
using static LeagueSandbox.GameServer.Logic.Chatbox.ChatboxManager;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    class HealthCommand : ChatCommand
    {
        public HealthCommand(string command, string syntax, ChatboxManager owner) : base(command, syntax, owner) { }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            PlayerManager _playerManager = Program.ResolveDependency<PlayerManager>();
            var split = arguments.ToLower().Split(' ');
            float hp;
            if (split.Length < 2)
            {
                _owner.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
            }
            else if (float.TryParse(split[1], out hp))
            {
                _playerManager.GetPeerInfo(peer).Champion.GetStats().HealthPoints.FlatBonus = hp;
                _playerManager.GetPeerInfo(peer).Champion.GetStats().CurrentHealth = hp;
            }
        }
    }
}
