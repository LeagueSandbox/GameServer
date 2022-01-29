using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.Stats;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
    internal class LuluR : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER,
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        private float _healthBefore;
        private float _meantimeDamage;
        private float _healthNow;
        private float _healthBonus;

        ISpell OwnerSpell;
        IParticle cast;

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            var owner = ownerSpell.CastInfo.Owner;
            OwnerSpell = ownerSpell;
            cast = AddParticleTarget(owner, unit, "Lulu_R_cas", unit);

            StatsModifier.Size.PercentBonus = StatsModifier.Size.PercentBonus + 1;
            _healthBefore = unit.Stats.CurrentHealth;
            _healthBonus = 150 + 150 * ownerSpell.CastInfo.SpellLevel;
            StatsModifier.HealthPoints.BaseBonus = StatsModifier.HealthPoints.BaseBonus + 150 + 150 * ownerSpell.CastInfo.SpellLevel;
            unit.Stats.CurrentHealth = unit.Stats.CurrentHealth + 150 + 150 * ownerSpell.CastInfo.SpellLevel;
            unit.AddStatModifier(StatsModifier);
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            RemoveParticle(cast);
            AddParticleTarget(OwnerSpell.CastInfo.Owner, unit, "Lulu_R_expire", unit);
            _healthNow = unit.Stats.CurrentHealth - _healthBonus;
            _meantimeDamage = _healthBefore - _healthNow;
            var bonusDamage = _healthBonus - _meantimeDamage;
            if (unit.Stats.CurrentHealth > unit.Stats.HealthPoints.Total)
            {
                unit.Stats.CurrentHealth = unit.Stats.CurrentHealth - bonusDamage;
            }
        }

        public void OnUpdate(float diff)
        {

        }
    }
}

