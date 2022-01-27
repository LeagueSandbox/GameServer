using System;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Scripting.CSharp;
using GameServerCore.Enums;

namespace Spells
{
    public class SummonerHeal : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            // TODO
        };

        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnSpellPreCast(IObjAiBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {
            IChampion mostWoundedAlliedIChampion = null;

            if (target != null
                && target is IChampion ch
                && IsUnitInRange(ch, owner.Position, spell.SpellData.CastRangeDisplayOverride, true))
            {
                mostWoundedAlliedIChampion = ch;
            }

            if (mostWoundedAlliedIChampion == null)
            {
                var units = GetChampionsInRange(owner.Position, spell.SpellData.CastRangeDisplayOverride, true);
                float lowestHealthPercentage = 100;
                float maxHealth;
                foreach (var value in units)
                {
                    if (value != owner && owner.Team == value.Team)
                    {
                        var currentHealth = value.Stats.CurrentHealth;
                        maxHealth = value.Stats.HealthPoints.Total;
                        if (currentHealth * 100 / maxHealth < lowestHealthPercentage && owner != value)
                        {
                            lowestHealthPercentage = currentHealth * 100 / maxHealth;
                            mostWoundedAlliedIChampion = value;
                        }
                    }
                }
            }

            if (mostWoundedAlliedIChampion != null)
            {
                PerformHeal(owner, spell, mostWoundedAlliedIChampion);
            }

            PerformHeal(owner, spell, owner);
        }

        private void PerformHeal(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            float healthGain = 75 + (target.Stats.Level * 15);
            if (target.HasBuff("HealCheck"))
            {
                healthGain *= 0.5f;
            }
            var newHealth = target.Stats.CurrentHealth + healthGain;
            target.Stats.CurrentHealth = Math.Min(newHealth, target.Stats.HealthPoints.Total);
            AddBuff("HealSpeed", 1.0f, 1, spell, target, owner);
            AddBuff("HealCheck", 35.0f, 1, spell, target, owner);
        }

        public void OnSpellCast(ISpell spell)
        {
        }

        public void OnSpellPostCast(ISpell spell)
        {
        }

        public void OnSpellChannel(ISpell spell)
        { 
        }

        public void OnSpellChannelCancel(ISpell spell, ChannelingStopSource reason)
        {
        }

        public void OnSpellPostChannel(ISpell spell)
        {
        }

        public void OnUpdate(float diff)
        {
        }
    }
}

