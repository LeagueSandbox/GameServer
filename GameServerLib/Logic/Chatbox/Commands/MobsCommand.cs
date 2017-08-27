using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic.Players;
using System.Linq;
using LeagueSandbox.GameServer.Logic.Packets.PacketArgs;
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
        private readonly IPacketArgsTranslationService _translationService;

        public override string Command => "mobs";
        public override string Syntax => $"{Command} teamNumber";

        public MobsCommand(ChatCommandManager chatCommandManager, Game game, PlayerManager playerManager,
            IPacketArgsTranslationService translationService)
            : base(chatCommandManager)
        {
            _game = game;
            _playerManager = playerManager;
            _translationService = translationService;
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
                var client = _playerManager.GetPeerInfo(peer);
                var args = _translationService.TranslateAttentionPingResponse(client, unit.Value.X, unit.Value.Y, 0, Pings.Ping_Danger);
                var response = new AttentionPingResponse(args);
                _game.PacketHandlerManager.broadcastPacketTeam(client.Team, response, Channel.CHL_S2C);
            }
        }
    }
}
