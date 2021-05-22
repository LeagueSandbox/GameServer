using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using LeagueSandbox.GameServer.GameObjects.Stats;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Scripting.CSharp;

namespace LuluWBuff
{
    internal class LuluWBuff : IBuffGameScript
    {
        public BuffType BuffType => BuffType.COMBAT_ENCHANCER;
        public BuffAddType BuffAddType => BuffAddType.REPLACE_EXISTING;
        public int MaxStacks => 1;
        public bool IsHidden => false;

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        IParticle buff1;
        IParticle buff2;

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            var caster = ownerSpell.CastInfo.Owner;
            buff1 = AddParticleTarget(caster, unit, "Lulu_W_buf_01.troy", unit);
            buff2 = AddParticleTarget(caster, unit, "Lulu_W_buf_02.troy", unit);

            var ap = ownerSpell.CastInfo.Owner.Stats.AbilityPower.Total * 0.001;
            StatsModifier.MoveSpeed.PercentBonus = StatsModifier.MoveSpeed.PercentBonus + 0.3f + (float)ap;
            unit.AddStatModifier(StatsModifier);
            var time = 2.5f + 0.5f * ownerSpell.CastInfo.SpellLevel;
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            RemoveParticle(buff1);
            RemoveParticle(buff2);
        }

        public void OnUpdate(float diff)
        {

        }
    }
}
