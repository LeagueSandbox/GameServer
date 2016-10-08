using LeagueSandbox.GameServer.Core.Logic.RAF;
using LeagueSandbox.GameServer.Logic.Enet;
using Newtonsoft.Json.Linq;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class AzirTurret : BaseTurret
    {
        public Unit Owner { get; private set; }

        public AzirTurret(
            Unit owner,
            string name,
            string model,
            float x = 0,
            float y = 0,
            TeamId team = TeamId.TEAM_BLUE,
            uint netId = 0
        ) : base(name, model, x, y, team, netId)
        {
            Owner = owner;

            BuildAzirTurret();

            SetTeam(team);
        }

        public void BuildAzirTurret()
        {
            JObject data;
            if (!_rafManager.ReadUnitStats(Model, out data))
            {
                _logger.LogCoreError("couldn't find turret stats for " + Model);
                return;
            }

            stats.HealthPoints.BaseValue = _rafManager.GetFloatValue(data, "Values", "Data", "BaseHP");
            stats.CurrentHealth = stats.HealthPoints.Total;
            stats.ManaPoints.BaseValue = _rafManager.GetFloatValue(data, "Values", "Data", "BaseMP");
            stats.CurrentMana = stats.ManaPoints.Total;
            stats.AttackDamage.BaseValue = _rafManager.GetFloatValue(data, "Values", "Data", "BaseDamage");
            stats.Range.BaseValue = 905.0f;
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
            if (!_rafManager.ReadAutoAttackData(Model, out autoAttack))
            {
                AutoAttackDelay = _rafManager.GetFloatValue(autoAttack, "SpellData", "CastFrame") / 30.0f;
                AutoAttackProjectileSpeed = _rafManager.GetFloatValue(autoAttack, "SpellData", "MissileSpeed");
            }
        }

        public override void refreshWaypoints()
        {
        }

        public override float getMoveSpeed()
        {
            return 0;
        }
    }
}
