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
                    return;
                }

                SpawnMinionsForTeam(team, userId);
            }
        }

        public void SpawnMinionsForTeam(TeamId team, int userId)
        {
            var championPos = _playerManager.GetPeerInfo(userId).Champion.Position;
            var random = new Random();

            var casterModel = Game.Map.MapProperties.GetMinionModel(team, MinionSpawnType.MINION_TYPE_CASTER);
            var cannonModel = Game.Map.MapProperties.GetMinionModel(team, MinionSpawnType.MINION_TYPE_CANNON);
            var meleeModel = Game.Map.MapProperties.GetMinionModel(team, MinionSpawnType.MINION_TYPE_MELEE);
            var superModel = Game.Map.MapProperties.GetMinionModel(team, MinionSpawnType.MINION_TYPE_SUPER);

            var minions = new[]
            {
                new Minion(Game, null, championPos, casterModel, casterModel, 0, team),
                new Minion(Game, null, championPos, cannonModel, cannonModel, 0, team),
                new Minion(Game, null, championPos, meleeModel, meleeModel, 0, team),
                new Minion(Game, null, championPos, superModel, superModel, 0, team)
            };

            const int X = 400;
            foreach (var minion in minions)
            {
                minion.SetPosition(championPos.X + random.Next(-X, X), championPos.Y + random.Next(-X, X));
                minion.PauseAi(true);
                minion.StopMovement();
                minion.UpdateMoveOrder(OrderType.Hold);
                Game.ObjectManager.AddObject(minion);
            }
        }
    }
}
