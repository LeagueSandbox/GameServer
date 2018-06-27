using System;
using System.Collections.Generic;
using System.Numerics;
using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class SpawnCommand : ChatCommandBase
    {
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public override string Command => "spawn";
        public override string Syntax => $"{Command} minionsblue minionspurple";

        public SpawnCommand(ChatCommandManager chatCommandManager, Game game, PlayerManager playerManager)
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
            }
            else if (split[1].StartsWith("minions"))
            {
                split[1] = split[1].Replace("minions", "team_").ToUpper();
                if (!Enum.TryParse(split[1], out TeamId team) || team == TeamId.TEAM_NEUTRAL)
                {
                    ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                    ShowSyntax();
                }

                SpawnMinionsForTeam(team, peer);
            }
        }

        public void SpawnMinionsForTeam(TeamId team, Peer peer)
        {
            var spawnPositions = new Dictionary<TeamId, MinionSpawnPosition>
            {
                [TeamId.TEAM_BLUE] = MinionSpawnPosition.SPAWN_BLUE_BOT,
                [TeamId.TEAM_PURPLE] = MinionSpawnPosition.SPAWN_RED_BOT
            };

            var champion = _playerManager.GetPeerInfo(peer).Champion;
            var random = new Random();

            var minions = new[]
            {
                new Minion(MinionSpawnType.MINION_TYPE_CASTER, spawnPositions[team]),
                new Minion(MinionSpawnType.MINION_TYPE_CANNON, spawnPositions[team]),
                new Minion(MinionSpawnType.MINION_TYPE_MELEE, spawnPositions[team]),
                new Minion(MinionSpawnType.MINION_TYPE_SUPER, spawnPositions[team])
            };

            const int X = 400;
            foreach (var minion in minions)
            {
                minion.SetPosition(champion.X + random.Next(-X, X), champion.Y + random.Next(-X, X));
                minion.PauseAi(true);
                minion.SetWaypoints(
                    new List<Vector2> {new Vector2(minion.X, minion.Y), new Vector2(minion.X, minion.Y)});
                minion.SetVisibleByTeam(team, true);
                _game.ObjectManager.AddObject(minion);
            }
        }
    }
}
