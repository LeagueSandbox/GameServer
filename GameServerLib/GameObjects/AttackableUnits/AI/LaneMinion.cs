﻿using System.Collections.Generic;
using System.Numerics;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects.Stats;
using static GameServerLib.API.APIMapFunctionManager;

namespace LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI
{
    public class LaneMinion : Minion, ILaneMinion
    {
        /// <summary>
        /// Const waypoints that define the minion's route
        /// </summary>
        public List<Vector2> PathingWaypoints { get; }
        /// <summary>
        /// Name of the Barracks that spawned this lane minion.
        /// </summary>
        public string BarracksName { get; }
        public MinionSpawnType MinionSpawnType { get; }

        public LaneMinion(
            Game game,
            MinionSpawnType spawnType,
            string barracksName,
            List<Vector2> mainWaypoints,
            string model,
            uint netId = 0,
            TeamId team = TeamId.TEAM_BLUE,
            string AiScript = ""
        ) : base(game, null, new Vector2(), model, model, netId, team, aiScript: AiScript)
        {
            IsLaneMinion = true;
            MinionSpawnType = spawnType;
            BarracksName = barracksName;
            PathingWaypoints = mainWaypoints;
            _aiPaused = false;

            var spawnSpecifics = GetMinionSpawnPosition(BarracksName);
            SetPosition(spawnSpecifics.Item2.X, spawnSpecifics.Item2.Y);

            StopMovement();

            MoveOrder = OrderType.Hold;
            Replication = new ReplicationLaneMinion(this);
        }
    }
}