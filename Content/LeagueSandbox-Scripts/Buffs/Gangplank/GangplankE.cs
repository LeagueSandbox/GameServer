using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Collections.Generic;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace Buffs
{
    internal class RaiseMorale : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER,
            BuffAddType = BuffAddType.REPLACE_EXISTING,
            MaxStacks = 1
        };

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        IBuff ThisBuff;

        private List<IParticle> Particles => new List<IParticle>();

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            var owner = ownerSpell.CastInfo.Owner;
            StatsModifier.AttackSpeed.PercentBonus = StatsModifier.AttackSpeed.PercentBonus + (10f + 20f * ownerSpell.CastInfo.SpellLevel) / 100f;
            StatsModifier.MoveSpeed.PercentBonus = StatsModifier.MoveSpeed.PercentBonus + (10f + 5f * ownerSpell.CastInfo.SpellLevel) / 100f;
            StatsModifier.AttackDamage.PercentBonus = StatsModifier.AttackDamage.PercentBonus + (10f + 10f * ownerSpell.CastInfo.SpellLevel) / 100f;
            unit.AddStatModifier(StatsModifier);

            Particles.Add(AddParticleTarget(owner, null, "pirate_raiseMorale_cas.troy", unit));
            Particles.Add(AddParticleTarget(owner, null, "pirate_raiseMorale_mis.troy", unit));
            Particles.Add(AddParticleTarget(owner, null, "pirate_raiseMorale_tar.troy", unit));
            Particles.Add(AddParticleTarget(owner, unit, "pirate_attack_buf_01.troy", unit, lifetime: 7f, bone: "L_HAND"));
            Particles.Add(AddParticleTarget(owner, unit, "pirate_attack_buf_01.troy", unit, lifetime: 7f, bone: "R_HAND"));
            Particles.Add(AddParticleTarget(owner, unit, "pirate_attack_cas.troy", unit, lifetime: 7f));
            
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            Particles.ForEach(particle => RemoveParticle(particle));
        }

        public void OnUpdate(float diff)
        {
        }
    }
}
