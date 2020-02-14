using GameServerCore.Enums;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace SummonerExhaustDebuff
{
    internal class SummonerExhaustDebuff : IBuffGameScript
    {
        public BuffType BuffType { get; } = BuffType.COMBAT_DEHANCER;
        public BuffAddType BuffAddType { get; } = BuffAddType.REPLACE_EXISTING;
        public int MaxStacks { get; } = 1;
        public bool IsHidden { get; } = false;
        public bool IsUnique { get; } = true;

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        public void OnActivate(IObjAiBase unit, IBuff buff, ISpell ownerSpell)
        {
            StatsModifier.MoveSpeed.PercentBonus = -0.3f;
            StatsModifier.AttackSpeed.PercentBonus = -0.3f;
            StatsModifier.Armor.BaseBonus = -10;
            StatsModifier.MagicResist.BaseBonus = -10;
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
