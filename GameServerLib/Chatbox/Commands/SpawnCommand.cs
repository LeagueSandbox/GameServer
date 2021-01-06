using GameServerCore;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace LeagueSandbox.GameServer.Chatbox.Commands
{
    public class SpawnCommand : ChatCommandBase
    {
        private readonly IPlayerManager _playerManager;

        public override string Command => "spawn";
        public override string Syntax => $"{Command} minionsblue minionspurple";

        public SpawnCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _playerManager = game.PlayerManager;
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
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

                SpawnMinionsForTeam(team, userId);
            }
        }

        public void SpawnMinionsForTeam(TeamId team, int userId)
        {
            var spawnPositions = new Dictionary<TeamId, string>
            {
                [TeamId.TEAM_BLUE] = Barracks.SPAWN_BLUE_BOT,
                [TeamId.TEAM_PURPLE] = Barracks.SPAWN_RED_BOT
            };

            var champion = _playerManager.GetPeerInfo((ulong)userId).Champion;
            var random = new Random();

            var minions = new[]
            {
                new LaneMinion(Game, MinionSpawnType.MINION_TYPE_CASTER, spawnPositions[team], Game.Map.MapProperties.GetMinionModel(Game.Map.MapProperties.GetMinionSpawnPosition(spawnPositions[team]).Item1, MinionSpawnType.MINION_TYPE_CASTER)),
                new LaneMinion(Game, MinionSpawnType.MINION_TYPE_CANNON, spawnPositions[team], Game.Map.MapProperties.GetMinionModel(Game.Map.MapProperties.GetMinionSpawnPosition(spawnPositions[team]).Item1, MinionSpawnType.MINION_TYPE_CANNON)),
                new LaneMinion(Game, MinionSpawnType.MINION_TYPE_MELEE, spawnPositions[team], Game.Map.MapProperties.GetMinionModel(Game.Map.MapProperties.GetMinionSpawnPosition(spawnPositions[team]).Item1, MinionSpawnType.MINION_TYPE_MELEE)),
                new LaneMinion(Game, MinionSpawnType.MINION_TYPE_SUPER, spawnPositions[team], Game.Map.MapProperties.GetMinionModel(Game.Map.MapProperties.GetMinionSpawnPosition(spawnPositions[team]).Item1, MinionSpawnType.MINION_TYPE_SUPER))
            };

            const int X = 400;
            foreach (var minion in minions)
            {
                minion.SetPosition(champion.X + random.Next(-X, X), champion.Y + random.Next(-X, X));
                minion.PauseAi(true);
                minion.SetVisibleByTeam(team, true);
                Game.ObjectManager.AddObject(minion);
            }
        }
    }
}
