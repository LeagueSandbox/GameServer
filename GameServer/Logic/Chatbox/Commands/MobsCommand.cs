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
    class MobsCommand : ChatCommand
    {
        public MobsCommand(string command, string syntax, ChatboxManager owner) : base(command, syntax, owner) { }

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
            int team;
            if (!int.TryParse(split[1], out team))
                return;
            var units = game.Map.GetObjects()
                .Where(xx => xx.Value.Team == CustomConvert.ToTeamId(team))
                .Where(xx => xx.Value is Minion || xx.Value is Monster);
            foreach (var unit in units)
            {
                var response = new AttentionPingAns(
                    playerManager.GetPeerInfo(peer),
                    new AttentionPing {
                        x = unit.Value.X,
                        y = unit.Value.Y,
                        targetNetId = 0,
                        type = Pings.Ping_Danger
                    });
                game.PacketHandlerManager.broadcastPacketTeam(
                    playerManager.GetPeerInfo(peer).Team,
                    response,
                    Channel.CHL_S2C
                );
            }
        }
    }
}
