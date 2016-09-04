using InibinSharp;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Core.Logic.RAF;
using LeagueSandbox.GameServer.Logic.Enet;
using Ninject;
using System;
using System.Linq;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class Placeable : Unit
    {
        private RAFManager _rafManager = Program.ResolveDependency<RAFManager>();

        private string _name;
        private Unit _owner; // We'll probably want to change this in the future

        public Placeable(
            Unit owner,
            uint id,
            float x,
            float y,
            string model,
            string name
        ) : base(id, model, new Stats(), 40, x, y)
        {
            setTeam(TeamId.TEAM_NEUTRAL);

            _owner = owner;

            var teams = Enum.GetValues(typeof(TeamId)).Cast<TeamId>();
            foreach (var team in teams)
            {
                setVisibleByTeam(team, true);
            }

            setMoveOrder(MoveOrder.MOVE_ORDER_MOVE);

            this._name = name;

            Inibin inibin;
            if (!_rafManager.readInibin("DATA/Characters/" + model + "/" + model + ".inibin", out inibin))
            {
                _logger.LogCoreError("Couldn't find placeable stats for " + model);
                return;
            }

            stats.HealthPoints.BaseValue = inibin.getFloatValue("Data", "BaseHP");
            stats.CurrentHealth = stats.HealthPoints.Total;
            stats.ManaPoints.BaseValue = inibin.getFloatValue("Data", "BaseMP");
            stats.CurrentMana = stats.ManaPoints.Total;
            stats.AttackDamage.BaseValue = inibin.getFloatValue("DATA", "BaseDamage");
            stats.Range.BaseValue = inibin.getFloatValue("DATA", "AttackRange");
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

            setMelee(inibin.getBoolValue("DATA", "IsMelee"));
            setCollisionRadius(inibin.getIntValue("DATA", "PathfindingCollisionRadius"));

            var autoAttack = _rafManager.GetAutoAttackData(model);
            if (autoAttack == null)
            {
                _logger.LogCoreError("Couldn't find monster auto-attack data for {0}", model);
                return;
            }

            autoAttackDelay = autoAttack.getFloatValue("SpellData", "castFrame") / 30.0f;
            autoAttackProjectileSpeed = autoAttack.getFloatValue("SpellData", "MissileSpeed");
        }

        public override void update(long diff)
        {
            base.update(diff);
        }

        public string getName()
        {
            return _name;
        }

        public Unit GetOwner()
        {
            return _owner;
        }

        public override bool isInDistress()
        {
            return distressCause != null;
        }
    }
}
