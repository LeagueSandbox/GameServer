using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
    internal class LuluWBuff : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER,
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };

        public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

        Particle buff1;
        Particle buff2;

        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            var caster = ownerSpell.CastInfo.Owner;
            buff1 = AddParticleTarget(caster, unit, "Lulu_W_buf_01", unit);
            buff2 = AddParticleTarget(caster, unit, "Lulu_W_buf_02", unit);

            var ap = ownerSpell.CastInfo.Owner.Stats.AbilityPower.Total * 0.001;
            StatsModifier.MoveSpeed.PercentBonus = StatsModifier.MoveSpeed.PercentBonus + 0.3f + (float)ap;
            unit.AddStatModifier(StatsModifier);
            var time = 2.5f + 0.5f * ownerSpell.CastInfo.SpellLevel;
        }

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            RemoveParticle(buff1);
            RemoveParticle(buff2);
        }
    }
}
