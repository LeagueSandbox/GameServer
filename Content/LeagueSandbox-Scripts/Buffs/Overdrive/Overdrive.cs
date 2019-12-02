using GameServerCore.Enums;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Overdrive
{
    internal class Overdrive : IBuffGameScript
    {
        private StatsModifier _statMod;
        private IBuff _visualBuff;

        public void OnActivate(IObjAiBase unit, ISpell ownerSpell)
        {
            _statMod = new StatsModifier();
            _statMod.MoveSpeed.PercentBonus = _statMod.MoveSpeed.PercentBonus + (12f + ownerSpell.Level * 4) / 100f;
            _statMod.AttackSpeed.PercentBonus = _statMod.AttackSpeed.PercentBonus + (22f + 8f * ownerSpell.Level) / 100f;
            unit.AddStatModifier(_statMod);
            _visualBuff = AddBuffHudVisual("Overdrive", 8.0f, 1, BuffType.COMBAT_ENCHANCER, unit);
        }

        public void OnDeactivate(IObjAiBase unit)
        {
            RemoveBuffHudVisual(_visualBuff);
            unit.RemoveStatModifier(_statMod);
        }

        public void OnUpdate(double diff)
        {

        }
    }
}

