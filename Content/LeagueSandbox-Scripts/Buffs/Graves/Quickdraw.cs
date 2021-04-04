using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Domain.GameObjects.Spell;

namespace Quickdraw
{
    internal class Quickdraw : IBuffGameScript
    {
        public BuffType BuffType => BuffType.COMBAT_ENCHANCER;
        public BuffAddType BuffAddType => BuffAddType.REPLACE_EXISTING;
        public int MaxStacks => 1;
        public bool IsHidden => false;

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        IParticle activate;

        public void OnUpdate(float diff)
        {
        }

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            activate = AddParticleTarget(ownerSpell.CastInfo.Owner, "Graves_Move_OnBuffActivate.troy", unit);
            StatsModifier.AttackSpeed.PercentBonus = 0.2f + (0.1f * ownerSpell.CastInfo.SpellLevel);
            unit.AddStatModifier(StatsModifier);
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            RemoveParticle(activate);
        }
    }
}
