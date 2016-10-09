using LeagueSandbox.GameServer.Core.Logic.RAF;
using Newtonsoft.Json.Linq;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class Placeable : Unit
    {
        public string Name { get; private set; }
        public Unit Owner { get; private set; } // We'll probably want to change this in the future

        public Placeable(
            Unit owner,
            float x,
            float y,
            string model,
            string name,
            uint netId = 0
        ) : base(model, new Stats(), 40, x, y, 0, netId)
        {
            SetTeam(owner.Team);

            Owner = owner;

            SetVisibleByTeam(Team, true);

            MoveOrder = MoveOrder.MOVE_ORDER_MOVE;

            Name = name;

            JObject data;

            if (!_rafManager.ReadUnitStats(model, out data))
            {
                _logger.LogCoreError("Couldn't find placeable stats for " + model);
                return;
            }

            stats.HealthPoints.BaseValue = _rafManager.GetFloatValue(data, "Values", "Data", "BaseHP");
            stats.CurrentHealth = stats.HealthPoints.Total;
            stats.ManaPoints.BaseValue = _rafManager.GetFloatValue(data, "Values", "Data", "BaseMP");
            stats.CurrentMana = stats.ManaPoints.Total;
            stats.AttackDamage.BaseValue = _rafManager.GetFloatValue(data, "Values", "Data", "BaseDamage");
            stats.Range.BaseValue = _rafManager.GetFloatValue(data, "Values", "Data", "AttackRange");
            stats.MoveSpeed.BaseValue = _rafManager.GetFloatValue(data, "Values", "Data", "MoveSpeed");
            stats.Armor.BaseValue = _rafManager.GetFloatValue(data, "Values", "Data", "Armor");
            stats.MagicResist.BaseValue = _rafManager.GetFloatValue(data, "Values", "Data", "SpellBlock");
            stats.HealthRegeneration.BaseValue = _rafManager.GetFloatValue(data, "Values", "Data", "BaseStaticHPRegen");
            stats.ManaRegeneration.BaseValue = _rafManager.GetFloatValue(data, "Values", "Data", "BaseStaticMPRegen");
            stats.AttackSpeedFlat = 0.625f / (1 + _rafManager.GetFloatValue(data, "Values", "Data", "AttackDelayOffsetPercent"));

            stats.HealthPerLevel = _rafManager.GetFloatValue(data, "Values", "Data", "HPPerLevel");
            stats.ManaPerLevel = _rafManager.GetFloatValue(data, "Values", "Data", "MPPerLevel");
            stats.AdPerLevel = _rafManager.GetFloatValue(data, "Values", "Data", "DamagePerLevel");
            stats.ArmorPerLevel = _rafManager.GetFloatValue(data, "Values", "Data", "ArmorPerLevel");
            stats.MagicResistPerLevel = _rafManager.GetFloatValue(data, "Values", "Data", "SpellBlockPerLevel");
            stats.HealthRegenerationPerLevel = _rafManager.GetFloatValue(data, "Values", "Data", "HPRegenPerLevel");
            stats.ManaRegenerationPerLevel = _rafManager.GetFloatValue(data, "Values", "Data", "MPRegenPerLevel");
            stats.GrowthAttackSpeed = _rafManager.GetFloatValue(data, "Values", "Data", "AttackSpeedPerLevel");

            IsMelee = _rafManager.GetBoolValue(data, "Values", "Data", "IsMelee");
            CollisionRadius = _rafManager.GetIntValue(data, "Values", "Data", "PathfindingCollisionRadius");

            JObject autoAttack;
            if (!_rafManager.ReadAutoAttackData(model, out autoAttack))
            {
                _logger.LogCoreError("Couldn't find monster auto-attack data for {0}", model);
                return;
            }

            AutoAttackDelay = _rafManager.GetFloatValue(autoAttack, "SpellData", "castFrame") / 30.0f;
            AutoAttackProjectileSpeed = _rafManager.GetFloatValue(autoAttack, "SpellData", "MissileSpeed");
        }

        public override bool isInDistress()
        {
            return DistressCause != null;
        }
    }
}
