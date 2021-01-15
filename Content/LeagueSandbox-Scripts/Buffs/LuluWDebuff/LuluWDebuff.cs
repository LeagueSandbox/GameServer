using GameServerCore.Enums;
using LeagueSandbox.GameServer.API;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace LuluWDebuff
{
    internal class LuluWDebuff : IBuffGameScript
    {
        public BuffType BuffType => BuffType.COMBAT_DEHANCER;
        public BuffAddType BuffAddType => BuffAddType.REPLACE_EXISTING;
        public int MaxStacks => 1;
        public bool IsHidden => false;

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            StatsModifier.MoveSpeed.BaseBonus = StatsModifier.MoveSpeed.BaseBonus - 60;
            unit.AddStatModifier(StatsModifier);
            var time = 1 + 0.25f * ownerSpell.Level;
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
