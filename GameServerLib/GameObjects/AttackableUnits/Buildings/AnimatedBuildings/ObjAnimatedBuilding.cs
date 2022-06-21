﻿using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects.Stats;
using System.Linq;
using System.Numerics;

namespace LeagueSandbox.GameServer.GameObjects.AttackableUnits.Buildings.AnimatedBuildings
{
    public class ObjAnimatedBuilding : ObjBuilding, IObjAnimatedBuilding
    {
        public ObjAnimatedBuilding(Game game, string model, int collisionRadius = 40,
            Vector2 position = new Vector2(), int visionRadius = 0, uint netId = 0, TeamId team = TeamId.TEAM_BLUE, IStats stats = null) :
            base(game, model, collisionRadius, position, visionRadius, netId, team, stats)
        {
            Replication = new ReplicationAnimatedBuilding(this);
        }

        public override void Update(float diff)
        {
            base.Update(diff);
        }
    }
}
