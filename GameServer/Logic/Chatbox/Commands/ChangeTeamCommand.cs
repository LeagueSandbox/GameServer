using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic.Players;
using System.Linq;
using static LeagueSandbox.GameServer.Logic.Chatbox.ChatboxManager;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    class ChangeTeamCommand : ChatCommand
    {
        public ChangeTeamCommand(string command, string syntax, ChatboxManager owner) : base(command, syntax, owner) { }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            Game game = Program.ResolveDependency<Game>();
            PlayerManager playerManager = Program.ResolveDependency<PlayerManager>();

            var split = arguments.ToLower().Split(' ');
            if (split.Length < 2)
            {
                _owner.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
                return;
            }
            int t;
            if (!int.TryParse(split[1], out t))
                return;
            var team = CustomConvert.ToTeamId(t);
            playerManager.GetPeerInfo(peer).Champion.SetTeam(team);
        }
    }
}
