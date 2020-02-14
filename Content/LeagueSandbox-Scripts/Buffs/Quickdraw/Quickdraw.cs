using GameServerCore.Enums;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Quickdraw
{
    internal class Quickdraw : IBuffGameScript
    {
        public BuffType BuffType { get; } = BuffType.COMBAT_ENCHANCER;
        public BuffAddType BuffAddType { get; } = BuffAddType.REPLACE_EXISTING;
        public int MaxStacks { get; } = 1;
        public bool IsHidden { get; } = false;
        public bool IsUnique { get; } = true;

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        public void OnUpdate(double diff)
        {

        }

        public void OnActivate(IObjAiBase unit, IBuff buff, ISpell ownerSpell)
        {
            StatsModifier.AttackSpeed.PercentBonus = 0.2f + (0.1f * ownerSpell.Level);
            unit.AddStatModifier(StatsModifier);
        }

        public void OnDeactivate(IObjAiBase unit)
        {
            unit.RemoveStatModifier(StatsModifier);
        }
    }
}
