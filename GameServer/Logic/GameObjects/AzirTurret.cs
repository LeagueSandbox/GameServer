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
            if (!RAFManager.ReadUnitStats(Model, out data))
            {
                _logger.LogCoreError("couldn't find turret stats for " + Model);
                return;
            }

            stats.HealthPoints.BaseValue = RAFManager.GetFloatValue(data, "Values", "Data", "BaseHP");
            stats.CurrentHealth = stats.HealthPoints.Total;
            stats.ManaPoints.BaseValue = RAFManager.GetFloatValue(data, "Values", "Data", "BaseMP");
            stats.CurrentMana = stats.ManaPoints.Total;
            stats.AttackDamage.BaseValue = RAFManager.GetFloatValue(data, "Values", "Data", "BaseDamage");
            stats.Range.BaseValue = 905.0f;
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
            if (!RAFManager.ReadAutoAttackData(Model, out autoAttack))
            {
                AutoAttackDelay = RAFManager.GetFloatValue(autoAttack, "SpellData", "CastFrame") / 30.0f;
                AutoAttackProjectileSpeed = RAFManager.GetFloatValue(autoAttack, "SpellData", "MissileSpeed");
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
