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
        private StatsModifier _statMod;
        private IBuff _visualBuff;

        public void OnActivate(IObjAiBase unit, ISpell ownerSpell)
        {
            _statMod = new StatsModifier();
            _statMod.MoveSpeed.PercentBonus = 27 / 100.0f;
            unit.AddStatModifier(_statMod);
            _visualBuff = AddBuffHudVisual("SummonerHaste", 10.0f, 1, BuffType.COMBAT_ENCHANCER, unit);
        }

        public void OnDeactivate(IObjAiBase unit)
        {
            unit.RemoveStatModifier(_statMod);
            RemoveBuffHudVisual(_visualBuff);
        }

        public void OnUpdate(double diff)
        {

        }
    }
}