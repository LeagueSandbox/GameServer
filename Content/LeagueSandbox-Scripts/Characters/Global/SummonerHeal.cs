using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;

namespace Spells
{
    public class SummonerHeal : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            // TODO
        };

        public void OnSpellPreCast(ObjAIBase owner, Spell spell, AttackableUnit target, Vector2 start, Vector2 end)
        {
            Champion mostWoundedAlliedIChampion = null;

            if (target != null
                && target is Champion ch
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

        private void PerformHeal(ObjAIBase owner, Spell spell, AttackableUnit target)
        {
            float healthGain = 75 + (target.Stats.Level * 15);
            if (target.HasBuff("HealCheck"))
            {
                healthGain *= 0.5f;
            }
            target.TakeHeal(owner, healthGain, spell);
            AddBuff("HealSpeed", 1.0f, 1, spell, target, owner);
            AddBuff("HealCheck", 35.0f, 1, spell, target, owner);
        }
    }
}

