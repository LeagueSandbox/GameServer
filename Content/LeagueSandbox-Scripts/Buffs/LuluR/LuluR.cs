using GameServerCore.Enums;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace LuluR
{
    internal class LuluR : IBuffGameScript
    {
        private StatsModifier _statMod;
        private float _healthBefore;
        private float _meantimeDamage;
        private float _healthNow;
        private float _healthBonus;
        private IBuff _visualBuff;

        public void OnActivate(IObjAiBase unit, ISpell ownerSpell)
        {
            _statMod = new StatsModifier();
            _statMod.Size.PercentBonus = _statMod.Size.PercentBonus + 1;
            _healthBefore = unit.Stats.CurrentHealth;
            _healthBonus = 150 + 150 * ownerSpell.Level;
            _statMod.HealthPoints.BaseBonus = _statMod.HealthPoints.BaseBonus + 150 + 150 * ownerSpell.Level;
            unit.Stats.CurrentHealth = unit.Stats.CurrentHealth + 150 + 150 * ownerSpell.Level;
            _visualBuff = AddBuffHudVisual("LuluR", 7.0f, 1, BuffType.COMBAT_ENCHANCER,
                unit);
            unit.AddStatModifier(_statMod);
        }

        public void OnDeactivate(IObjAiBase unit)
        {
            _healthNow = unit.Stats.CurrentHealth - _healthBonus;
            _meantimeDamage = _healthBefore - _healthNow;
            var bonusDamage = _healthBonus - _meantimeDamage;
            unit.RemoveStatModifier(_statMod);
            if (unit.Stats.CurrentHealth > unit.Stats.HealthPoints.Total)
            {
                unit.Stats.CurrentHealth = unit.Stats.CurrentHealth - bonusDamage;
            }
            RemoveBuffHudVisual(_visualBuff);
        }

        public void OnUpdate(double diff)
        {

        }
    }
}

