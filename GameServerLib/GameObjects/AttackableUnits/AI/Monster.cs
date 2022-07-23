using System.Numerics;
using GameServerCore.Enums;
using GameServerLib.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.StatsNS;

namespace LeagueSandbox.GameServer.GameObjects
{
    public class Monster : Minion
    {
        public MonsterCamp Camp { get; private set; }
        public string SpawnAnimation { get; private set; }

        public Monster(
            Game game,
            string name,
            string model,
            Vector2 position,
            Vector3 faceDirection,
            MonsterCamp monsterCamp,
            TeamId team = TeamId.TEAM_NEUTRAL,
            uint netId = 0,
            string spawnAnimation = "",
            bool isTargetable = true,
            bool ignoresCollision = false,
            Stats stats = null,
            string aiScript = "",
            int damageBonus = 0,
            int healthBonus = 0,
            int initialLevel = 1
        ) : base
            (
                game, null, position, model, name,
                netId, team, 0, ignoresCollision, isTargetable,
                false, null, stats, aiScript, damageBonus, healthBonus, initialLevel
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

