using GameServerCore.Enums;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace HealSpeed
{
    internal class HealSpeed : IBuffGameScript
    {
        private StatsModifier _statMod;
        private IBuff _healBuff;

        public void OnActivate(IObjAiBase unit, ISpell ownerSpell)
        {
            _statMod = new StatsModifier();
            _statMod.MoveSpeed.PercentBonus = 0.3f;
            unit.AddStatModifier(_statMod);
            _healBuff = AddBuffHudVisual("SummonerHeal", 1.0f, 1, BuffType.COMBAT_ENCHANCER, unit);
        }

        public void OnDeactivate(IObjAiBase unit)
        {
            unit.RemoveStatModifier(_statMod);
            RemoveBuffHudVisual(_healBuff);
        }

        public void OnUpdate(double diff)
        {

        }
    }
}
