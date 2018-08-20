using System.Linq;
using ENet;
using GameServerCore;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Interfaces;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class MobsCommand : ChatCommandBase
    {
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public override string Command => "mobs";
        public override string Syntax => $"{Command} teamNumber";

        public MobsCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _game = game;
            _playerManager = game.PlayerManager;
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

            if (!int.TryParse(split[1], out var team))
            {
                return;
            }

            var units = Game.ObjectManager.GetObjects()
                .Where(xx => xx.Value.Team == team.ToTeamId())
                .Where(xx => xx.Value is Minion || xx.Value is Monster);

            var client = _playerManager.GetPeerInfo(peer);
            foreach (var unit in units)
            {
                 _game.PacketNotifier.NotifyPing(client, unit.Value.X, unit.Value.Y, 0, Pings.PING_DANGER);
            }
        }
    }
}
