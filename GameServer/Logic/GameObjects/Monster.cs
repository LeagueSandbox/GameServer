using LeagueSandbox.GameServer.Core.Logic.RAF;
using LeagueSandbox.GameServer.Logic.Enet;
using System;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json.Linq;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class Monster : Unit
    {
        public Vector2 Facing { get; private set; }
        public string Name { get; private set; }
        public string SpawnAnimation { get; private set; }
        public byte CampId { get; private set; }
        public byte CampUnk { get; private set; }
        public float SpawnAnimationTime { get; private set; }

        public Monster(
            float x,
            float y,
            float facingX,
            float facingY,
            string model,
            string name,
            string spawnAnimation = "",
            byte campId = 0x01,
            byte campUnk = 0x2A,
            float spawnAnimationTime = 0.0f,
            uint netId = 0
        ) : base(model, new Stats(), 40, x, y, 0, netId)
        {
            SetTeam(TeamId.TEAM_NEUTRAL);

            var teams = Enum.GetValues(typeof(TeamId)).Cast<TeamId>();
            foreach (var team in teams)
                SetVisibleByTeam(team, true);

            MoveOrder = MoveOrder.MOVE_ORDER_MOVE;
            Facing = new Vector2(facingX, facingY);
            Name = name;
            SpawnAnimation = spawnAnimation;
            CampId = campId;
            CampUnk = campUnk;
            SpawnAnimationTime = spawnAnimationTime;

            JObject data;

            if (!RAFManager.ReadUnitStats(model, out data))
            {
                _logger.LogCoreError("Couldn't find monster stats for " + model);
                return;
            }

            stats.HealthPoints.BaseValue = RAFManager.GetFloatValue(data, "Values", "Data", "BaseHP");
            stats.CurrentHealth = stats.HealthPoints.Total;
            stats.ManaPoints.BaseValue = RAFManager.GetFloatValue(data, "Values", "Data", "BaseMP");
            stats.CurrentMana = stats.ManaPoints.Total;
            stats.AttackDamage.BaseValue = RAFManager.GetFloatValue(data, "Values", "Data", "BaseDamage");
            stats.Range.BaseValue = RAFManager.GetFloatValue(data, "Values", "Data", "AttackRange");
            stats.MoveSpeed.BaseValue = RAFManager.GetFloatValue(data, "Values", "Data", "MoveSpeed");
            stats.Armor.BaseValue = RAFManager.GetFloatValue(data, "Values", "Data", "Armor");
            stats.MagicResist.BaseValue = RAFManager.GetFloatValue(data, "Values", "Data", "SpellBlock");
            stats.HealthRegeneration.BaseValue = RAFManager.GetFloatValue(data, "Values", "Data", "BaseStaticHPRegen");
            stats.ManaRegeneration.BaseValue = RAFManager.GetFloatValue(data, "Values", "Data", "BaseStaticMPRegen");
            stats.AttackSpeedFlat = 0.625f / (1 + RAFManager.GetFloatValue(data, "Values", "Data", "AttackDelayOffsetPercent"));

            stats.HealthPerLevel = RAFManager.GetFloatValue(data, "Values", "Data", "HPPerLevel");
            stats.ManaPerLevel = RAFManager.GetFloatValue(data, "Values", "Data", "MPPerLevel");
            stats.AdPerLevel = RAFManager.GetFloatValue(data, "Values", "Data", "DamagePerLevel");
            stats.ArmorPerLevel = RAFManager.GetFloatValue(data, "Values", "Data", "ArmorPerLevel");
            stats.MagicResistPerLevel = RAFManager.GetFloatValue(data, "Values", "Data", "SpellBlockPerLevel");
            stats.HealthRegenerationPerLevel = RAFManager.GetFloatValue(data, "Values", "Data", "HPRegenPerLevel");
            stats.ManaRegenerationPerLevel = RAFManager.GetFloatValue(data, "Values", "Data", "MPRegenPerLevel");
            stats.GrowthAttackSpeed = RAFManager.GetFloatValue(data, "Values", "Data", "AttackSpeedPerLevel");

            IsMelee = RAFManager.GetBoolValue(data, "Values", "Data", "IsMelee");
            CollisionRadius = RAFManager.GetIntValue(data, "Values", "Data", "PathfindingCollisionRadius");

            if (!RAFManager.ReadAutoAttackData(model, out data))
            {
                _logger.LogCoreError("Couldn't find monster auto-attack data for " + model);
                return;
            }

            AutoAttackDelay = RAFManager.GetFloatValue(data, "SpellData", "castFrame") / 30.0f;
            AutoAttackProjectileSpeed = RAFManager.GetFloatValue(data, "SpellData", "MissileSpeed");
        }

        public override bool isInDistress()
        {
            return DistressCause != null;
        }
    }
}
