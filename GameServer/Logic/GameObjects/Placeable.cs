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

            if (!RAFManager.ReadUnitStats(model, out data))
            {
                _logger.LogCoreError("Couldn't find placeable stats for " + model);
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

            JObject autoAttack;
            if (!RAFManager.ReadAutoAttackData(model, out autoAttack))
            {
                _logger.LogCoreError("Couldn't find monster auto-attack data for {0}", model);
                return;
            }

            AutoAttackDelay = RAFManager.GetFloatValue(autoAttack, "SpellData", "castFrame") / 30.0f;
            AutoAttackProjectileSpeed = RAFManager.GetFloatValue(autoAttack, "SpellData", "MissileSpeed");
        }

        public override bool isInDistress()
        {
            return DistressCause != null;
        }
    }
}
