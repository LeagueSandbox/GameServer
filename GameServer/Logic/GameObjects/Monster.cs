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

            if (!_rafManager.ReadUnitStats(model, out data))
            {
                _logger.LogCoreError("Couldn't find monster stats for " + model);
                return;
            }

            stats.HealthPoints.BaseValue = _rafManager.GetFloatValue(data, "Data", "BaseHP");
            stats.CurrentHealth = stats.HealthPoints.Total;
            stats.ManaPoints.BaseValue = _rafManager.GetFloatValue(data, "Data", "BaseMP");
            stats.CurrentMana = stats.ManaPoints.Total;
            stats.AttackDamage.BaseValue = _rafManager.GetFloatValue(data, "Data", "BaseDamage");
            stats.Range.BaseValue = _rafManager.GetFloatValue(data, "Data", "AttackRange");
            stats.MoveSpeed.BaseValue = _rafManager.GetFloatValue(data, "Data", "MoveSpeed");
            stats.Armor.BaseValue = _rafManager.GetFloatValue(data, "Data", "Armor");
            stats.MagicResist.BaseValue = _rafManager.GetFloatValue(data, "Data", "SpellBlock");
            stats.HealthRegeneration.BaseValue = _rafManager.GetFloatValue(data, "Data", "BaseStaticHPRegen");
            stats.ManaRegeneration.BaseValue = _rafManager.GetFloatValue(data, "Data", "BaseStaticMPRegen");
            stats.AttackSpeedFlat = 0.625f / (1 + _rafManager.GetFloatValue(data, "Data", "AttackDelayOffsetPercent"));

            stats.HealthPerLevel = _rafManager.GetFloatValue(data, "Data", "HPPerLevel");
            stats.ManaPerLevel = _rafManager.GetFloatValue(data, "Data", "MPPerLevel");
            stats.AdPerLevel = _rafManager.GetFloatValue(data, "Data", "DamagePerLevel");
            stats.ArmorPerLevel = _rafManager.GetFloatValue(data, "Data", "ArmorPerLevel");
            stats.MagicResistPerLevel = _rafManager.GetFloatValue(data, "Data", "SpellBlockPerLevel");
            stats.HealthRegenerationPerLevel = _rafManager.GetFloatValue(data, "Data", "HPRegenPerLevel");
            stats.ManaRegenerationPerLevel = _rafManager.GetFloatValue(data, "Data", "MPRegenPerLevel");
            stats.GrowthAttackSpeed = _rafManager.GetFloatValue(data, "Data", "AttackSpeedPerLevel");

            IsMelee = _rafManager.GetBoolValue(data, "Data", "IsMelee");
            CollisionRadius = _rafManager.GetIntValue(data, "Data", "PathfindingCollisionRadius");

            if (!_rafManager.ReadAutoAttackData(model, out data))
            {
                _logger.LogCoreError("Couldn't find monster auto-attack data for " + model);
                return;
            }

            AutoAttackDelay = _rafManager.GetFloatValue(data, "SpellData", "CastFrame") / 30.0f;
            AutoAttackProjectileSpeed = _rafManager.GetFloatValue(data, "SpellData", "MissileSpeed");
        }

        public override bool isInDistress()
        {
            return DistressCause != null;
        }
    }
}
