using System;
using System.Numerics;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using LeagueSandbox.GameServer.API;
using System.Collections.Generic;
using GameServerCore.Scripting.CSharp;
using GameServerCore.Domain.GameObjects.Spell.Sector;

namespace Spells
{
    public class EzrealTrueshotBarrage : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true,
            MissileParameters = new MissileParameters
            {
                Type = MissileType.Circle
            }
            // TODO
        };

        private IObjAIBase _owner;
        private ISpell _spell;
        private float _bonusAd = 0;

        public void OnActivate(IObjAIBase owner, ISpell spell)
        {
            _owner = owner;
            _spell = spell;
            ApiEventManager.OnSpellHit.AddListener(this, spell, TargetExecute, false);
            ApiEventManager.OnUpdateStats.AddListener(this, owner, OnStatsUpdate, false);
        }

        public void OnSpellCast(ISpell spell)
        {
            var owner = spell.CastInfo.Owner;
            AddParticleTarget(owner, owner, "Ezreal_bow_huge", owner, bone: "L_HAND");
        }

        public void TargetExecute(ISpell spell, IAttackableUnit target, ISpellMissile missile, ISpellSector sector)
        {
            var owner = spell.CastInfo.Owner;
            if (missile is ISpellCircleMissile skillshot)
            {
                var reduc = Math.Min(skillshot.ObjectsHit.Count, 7);
                var bonusAd = owner.Stats.AttackDamage.Total - owner.Stats.AttackDamage.BaseValue;
                var ap = owner.Stats.AbilityPower.Total * 0.9f;
                var damage = 200f + (150f * (spell.CastInfo.SpellLevel - 1)) + bonusAd + ap;
                target.TakeDamage(owner, damage * (1f - (reduc - 1f) / 10f), DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
            }
        }

        private void OnStatsUpdate(IAttackableUnit _unit, float diff)
        {
            float bonusAd = _owner.Stats.AttackDamage.Total - _owner.Stats.AttackDamage.BaseValue;
            if(_bonusAd != bonusAd)
            {
                _bonusAd = bonusAd; 
                SetSpellToolTipVar(_owner, 0, bonusAd, SpellbookType.SPELLBOOK_CHAMPION, 3, SpellSlotType.SpellSlots);
            }
        }
    }
}