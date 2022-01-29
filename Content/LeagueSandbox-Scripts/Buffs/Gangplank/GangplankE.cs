using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using System.Collections.Generic;
using LeagueSandbox.GameServer.GameObjects.Stats;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
    internal class GangplankE : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_DEHANCER
        };

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        private List<IParticle> Particles => new List<IParticle>();

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            var owner = ownerSpell.CastInfo.Owner;
            StatsModifier.AttackSpeed.PercentBonus = StatsModifier.AttackSpeed.PercentBonus + (10f + 20f * ownerSpell.CastInfo.SpellLevel) / 100f;
            StatsModifier.MoveSpeed.PercentBonus = StatsModifier.MoveSpeed.PercentBonus + (10f + 5f * ownerSpell.CastInfo.SpellLevel) / 100f;
            StatsModifier.AttackDamage.PercentBonus = StatsModifier.AttackDamage.PercentBonus + (10f + 10f * ownerSpell.CastInfo.SpellLevel) / 100f;
            unit.AddStatModifier(StatsModifier);

            //_hudvisual = AddBuffHUDVisual("RaiseMorale", time, 1, unit);

            Particles.Add(AddParticleTarget(owner, null, "pirate_raiseMorale_cas", unit));
            Particles.Add(AddParticleTarget(owner, null, "pirate_raiseMorale_mis", unit));
            Particles.Add(AddParticleTarget(owner, null, "pirate_raiseMorale_tar", unit));
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            //RemoveBuffHudVisual(_hudvisual);
            Particles.ForEach(particle => RemoveParticle(particle));
        }

        public void OnUpdate(float diff)
        {

        }
    }
}
