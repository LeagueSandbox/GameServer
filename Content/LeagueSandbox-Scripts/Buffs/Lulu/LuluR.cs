﻿using GameServerCore.Enums;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Domain.GameObjects.Spell;

namespace LuluR
{
    internal class LuluR : IBuffGameScript
    {
        public BuffType BuffType => BuffType.COMBAT_ENCHANCER;
        public BuffAddType BuffAddType => BuffAddType.REPLACE_EXISTING;
        public int MaxStacks => 1;
        public bool IsHidden => false;

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        private float _healthBefore;
        private float _meantimeDamage;
        private float _healthNow;
        private float _healthBonus;

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            StatsModifier.Size.PercentBonus = StatsModifier.Size.PercentBonus + 1;
            _healthBefore = unit.Stats.CurrentHealth;
            _healthBonus = 150 + 150 * ownerSpell.CastInfo.SpellLevel;
            StatsModifier.HealthPoints.BaseBonus = StatsModifier.HealthPoints.BaseBonus + 150 + 150 * ownerSpell.CastInfo.SpellLevel;
            unit.Stats.CurrentHealth = unit.Stats.CurrentHealth + 150 + 150 * ownerSpell.CastInfo.SpellLevel;
            unit.AddStatModifier(StatsModifier);
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            _healthNow = unit.Stats.CurrentHealth - _healthBonus;
            _meantimeDamage = _healthBefore - _healthNow;
            var bonusDamage = _healthBonus - _meantimeDamage;
            if (unit.Stats.CurrentHealth > unit.Stats.HealthPoints.Total)
            {
                unit.Stats.CurrentHealth = unit.Stats.CurrentHealth - bonusDamage;
            }
        }

        public void OnUpdate(float diff)
        {

        }
    }
}

