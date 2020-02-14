using GameServerCore.Enums;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace SummonerHasteBuff
{
    internal class SummonerHasteBuff : IBuffGameScript
    {
        public BuffType BuffType { get; } = BuffType.HASTE;
        public BuffAddType BuffAddType { get; } = BuffAddType.STACKS_AND_OVERLAPS;
        public int MaxStacks { get; } = 5;
        public bool IsHidden { get; } = false;
        public bool IsUnique { get; } = false;

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        public void OnActivate(IObjAiBase unit, IBuff buff, ISpell ownerSpell)
        {
            StatsModifier.MoveSpeed.PercentBonus = 27 / 100.0f;
            unit.AddStatModifier(StatsModifier);
        }

        public void OnDeactivate(IObjAiBase unit)
        {
            unit.RemoveStatModifier(StatsModifier);
        }

        public void OnUpdate(double diff)
        {

        }
    }
}