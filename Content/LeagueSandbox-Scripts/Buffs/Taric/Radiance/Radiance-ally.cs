using GameServerCore.Enums;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Radiance_ally
{
    internal class Radiance_ally : IBuffGameScript
    {
        public BuffType BuffType => BuffType.COMBAT_ENCHANCER;
        public BuffAddType BuffAddType => BuffAddType.REPLACE_EXISTING;
        public int MaxStacks => 1;
        public bool IsHidden => false;

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            StatsModifier.AttackDamage.FlatBonus += (10f + ownerSpell.Level * 20) / 2;
            StatsModifier.AbilityPower.FlatBonus += (10f + ownerSpell.Level * 20) / 2;
            unit.AddStatModifier(StatsModifier);
        }

        public void OnDeactivate(IAttackableUnit unit)
        {
            unit.RemoveStatModifier(StatsModifier);
        }

        public void OnUpdate(double diff)
        {

        }
    }
}
