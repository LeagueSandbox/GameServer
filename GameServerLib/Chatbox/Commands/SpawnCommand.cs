﻿using System;
using System.Collections.Generic;
using System.Numerics;
using ENet;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Players;

namespace LeagueSandbox.GameServer.Chatbox.Commands
{
    public class SpawnCommand : ChatCommandBase
    {
        private readonly PlayerManager _playerManager;

        public override string Command => "spawn";
        public override string Syntax => $"{Command} minionsblue minionspurple";

        public SpawnCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _playerManager = game.PlayerManager;
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
                new Minion(Game, MinionSpawnType.MINION_TYPE_CASTER, spawnPositions[team]),
                new Minion(Game, MinionSpawnType.MINION_TYPE_CANNON, spawnPositions[team]),
                new Minion(Game, MinionSpawnType.MINION_TYPE_MELEE, spawnPositions[team]),
                new Minion(Game, MinionSpawnType.MINION_TYPE_SUPER, spawnPositions[team])
            };

            const int X = 400;
            foreach (var minion in minions)
            {
                minion.SetPosition(champion.Position + new Vector2(random.Next(-X, X), random.Next(-X, X)));
                minion.PauseAi(true);
                minion.SetWaypoints(
                    new List<Vector2> { minion.Position, minion.Position});
                minion.SetVisibleByTeam(team, true);
                Game.ObjectManager.AddObject(minion);
            }
        }
    }
}
