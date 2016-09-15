using InibinSharp;
using LeagueSandbox.GameServer.Core.Logic.RAF;
using LeagueSandbox.GameServer.Logic.Enet;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class AzirTurret : BaseTurret
    {
        private RAFManager _rafManager = Program.ResolveDependency<RAFManager>();

        private const float TURRET_RANGE = 905.0f;
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
            this.Owner = owner;

            BuildAzirTurret();

            SetTeam(team);
        }

        public void BuildAzirTurret()
        {
            Inibin inibin;
            if (!_rafManager.readInibin("DATA/Characters/" + Model + "/" + Model + ".inibin", out inibin))
            {
                _logger.LogCoreError("couldn't find turret stats for " + Model);
                return;
            }

            stats.HealthPoints.BaseValue = inibin.getFloatValue("Data", "BaseHP");
            stats.CurrentHealth = stats.HealthPoints.Total;
            stats.ManaPoints.BaseValue = inibin.getFloatValue("Data", "BaseMP");
            stats.CurrentMana = stats.ManaPoints.Total;
            stats.AttackDamage.BaseValue = inibin.getFloatValue("DATA", "BaseDamage");
            stats.Range.BaseValue = TURRET_RANGE;
            stats.MoveSpeed.BaseValue = inibin.getFloatValue("DATA", "MoveSpeed");
            stats.Armor.BaseValue = inibin.getFloatValue("DATA", "Armor");
            stats.MagicResist.BaseValue = inibin.getFloatValue("DATA", "SpellBlock");
            stats.HealthRegeneration.BaseValue = inibin.getFloatValue("DATA", "BaseStaticHPRegen");
            stats.ManaRegeneration.BaseValue = inibin.getFloatValue("DATA", "BaseStaticMPRegen");
            stats.AttackSpeedFlat = 0.625f / (1 + inibin.getFloatValue("DATA", "AttackDelayOffsetPercent"));

            stats.HealthPerLevel = inibin.getFloatValue("DATA", "HPPerLevel");
            stats.ManaPerLevel = inibin.getFloatValue("DATA", "MPPerLevel");
            stats.AdPerLevel = inibin.getFloatValue("DATA", "DamagePerLevel");
            stats.ArmorPerLevel = inibin.getFloatValue("DATA", "ArmorPerLevel");
            stats.MagicResistPerLevel = inibin.getFloatValue("DATA", "SpellBlockPerLevel");
            stats.HealthRegenerationPerLevel = inibin.getFloatValue("DATA", "HPRegenPerLevel");
            stats.ManaRegenerationPerLevel = inibin.getFloatValue("DATA", "MPRegenPerLevel");
            stats.GrowthAttackSpeed = inibin.getFloatValue("DATA", "AttackSpeedPerLevel");

            IsMelee = inibin.getBoolValue("DATA", "IsMelee");
            CollisionRadius = inibin.getIntValue("DATA", "PathfindingCollisionRadius");

            Inibin autoAttack = _rafManager.GetAutoAttackData(Model);

            if (autoAttack != null)
            {
                AutoAttackDelay = autoAttack.getFloatValue("SpellData", "castFrame") / 30.0f;
                AutoAttackProjectileSpeed = autoAttack.getFloatValue("SpellData", "MissileSpeed");
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
