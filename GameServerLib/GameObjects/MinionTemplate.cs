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
    public class MinionTemplate : IMinionTemplate
    {
        public IObjAiBase Owner { get; set; }
        public string Name { get; set; }
        public string Model { get; set; }
        public Vector2 Position { get; set; }
        public int SkinId { get; set; }
        public TeamId Team { get; set; }
        public uint NetId { get; set; }
        public bool IsTargetable { get; set; }
        public bool IgnoresCollision { get; set; }
        public string AiScript { get; set; }
        public int DamageBonus { get; set; }
        public int HealthBonus { get; set; }
        public int InitialLevel { get; set; }
        public IObjAiBase VisibilityOwner { get; set; }

        public MinionTemplate(
            IObjAiBase owner,
            string model,
            string name,
            Vector2 position,
            uint netId = 0,
            TeamId team = TeamId.TEAM_NEUTRAL,
            int skinId = 0,
            bool ignoreCollision = false,
            bool targetable = true,
            IObjAiBase visibilityOwner = null,
            string aiScript = "",
            int damageBonus = 0,
            int healthBonus = 0,
            int initialLevel = 1
        )
        {
            Owner = owner;
            Name = name;
            Model = model;
            Team = team;
            Position = position;
            NetId = netId;
            IsTargetable = targetable;
            IgnoresCollision = ignoreCollision;
            AiScript = aiScript;
            DamageBonus = damageBonus;
            HealthBonus = healthBonus;
            InitialLevel = initialLevel;
            VisibilityOwner = visibilityOwner;
            SkinId = skinId;
        }
    }
}
