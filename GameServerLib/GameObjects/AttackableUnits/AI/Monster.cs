using System;
using System.Linq;
using System.Numerics;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.Stats;

namespace LeagueSandbox.GameServer.GameObjects
{
    public class Monster : Minion, IMonster
    {
        public IMonsterCamp Camp { get; private set; }
        public string SpawnAnimation { get; private set; }

        public Monster(
            Game game,
            string name,
            string model,
            Vector2 position,
            Vector3 faceDirection,
            IMonsterCamp monsterCamp,
            TeamId team = TeamId.TEAM_NEUTRAL,
            uint netId = 0,
            string spawnAnimation = "",
            bool isTargetable = true,
            bool ignoresCollision = false,
            string aiScript = "",
            int damageBonus = 0,
            int healthBonus = 0,
            int initialLevel = 1
        ) : base
            (
                game, null, position, model, name,
                netId, team, 0, ignoresCollision, isTargetable,
                false ,null, aiScript, damageBonus, healthBonus, initialLevel
            )
        {
            Camp = monsterCamp;
            Team = team;
            SpawnAnimation = spawnAnimation;
            FaceDirection(faceDirection);
            IsTargetable = isTargetable;
            IgnoresCollision = ignoresCollision;
        }

        public void UpdateInitialLevel(int level)
        {
            InitialLevel = level;
        }
    }
}

