using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic.Players;
using System.Linq;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;
using static LeagueSandbox.GameServer.Logic.Chatbox.ChatCommandManager;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class MobsCommand : ChatCommandBase
    {
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public override string Command => "mobs";
        public override string Syntax => $"{Command} teamNumber";

        public MobsCommand(ChatCommandManager chatCommandManager, Game game, PlayerManager playerManager)
            : base(chatCommandManager)
        {
            _game = game;
            _playerManager = playerManager;
        }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            if (split.Length < 2)
            {
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
                return;
            }
            int team;
            if (!int.TryParse(split[1], out team))
                return;

            var units = _game.ObjectManager.GetObjects()
                .Where(xx => xx.Value.Team == CustomConvert.ToTeamId(team))
                .Where(xx => xx.Value is Minion || xx.Value is Monster);

            foreach (var unit in units)
            {
                var ping = new AttentionPingRequest(unit.Value.X, unit.Value.Y, 0, Pings.PING_DANGER);
                var client = _playerManager.GetPeerInfo(peer);
                var response = new AttentionPingResponse(client, ping);
                _game.PacketHandlerManager.BroadcastPacketTeam(client.Team, response, Channel.CHL_S2_C);
            }
        }
    }
}
