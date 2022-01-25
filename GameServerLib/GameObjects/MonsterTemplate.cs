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
    public class MonsterTemplate : IMonsterTemplate
    {
        public string Name { get; set; }
        public string Model { get; set; }
        public Vector2 Position { get; set; }
        public Vector3 FaceDirection { get; set; }
        public IMonsterCamp Camp { get; set; }
        public TeamId Team { get; set; }
        public string SpawnAnimation { get; set; }
        public uint NetId { get; set; }
        public bool IsTargetable { get; set; }
        public bool IgnoresCollision { get; set; }
        public string AiScript { get; set; }
        public int DamageBonus { get; set; }
        public int HealthBonus { get; set; }
        public int InitialLevel { get; set; }

        public MonsterTemplate(
            string name,
            string model,
            Vector2 position,
            Vector3 faceDirection,
            IMonsterCamp monsterCamp,
            TeamId team = TeamId.TEAM_NEUTRAL,
            string spawnAnimation = "",
            uint netId = 0,
            bool isTargetable = true,
            bool ignoresCollision = false,
            string aiScript = "",
            int damageBonus = 0,
            int healthBonus = 0,
            int initialLevel = 1
        )
        {
            Name = name;
            Model = model;
            FaceDirection = faceDirection;
            Camp = monsterCamp;
            Team = team;
            SpawnAnimation = spawnAnimation;
            Position = position;
            NetId = netId;
            IsTargetable = isTargetable;
            IgnoresCollision = ignoresCollision;
            AiScript = aiScript;
            DamageBonus = damageBonus;
            HealthBonus = healthBonus;
            InitialLevel = initialLevel;
        }
    }
}
