using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects;
using            GameServerLib.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using GameServerCore.Scripting.CSharp;
using System;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
    internal class Meditate : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.HEAL
        };

        public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

        float[] healthTick =
        {
            15f,
            25f,
            35f,
            45f,
            55f
        };

        float[] flatArmorMod =
        {
            100.0f,
            150.0f,
            200.0f,
            250.0f,
            300.0f
        };

        float[] flatSpellBlockMod =
        {
            100.0f,
            150.0f,
            200.0f,
            250.0f,
            300.0f
        };

        ObjAIBase owner;
        float tickTime;
        float trueHeal;
        int spellLevel;
        Particle buffParticle;

        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            owner = ownerSpell.CastInfo.Owner;
            spellLevel = ownerSpell.CastInfo.SpellLevel - 1;

            StatsModifier.Armor.FlatBonus = flatArmorMod[spellLevel];
            StatsModifier.MagicResist.FlatBonus = flatSpellBlockMod[spellLevel];

            owner.AddStatModifier(StatsModifier);

            ApiEventManager.OnTakeDamage.AddListener(this, unit, TakeDamage, false);

            buffParticle = AddParticleTarget(unit, unit, "masteryi_base_w_buf", unit, 4.0f, flags: 0);
        }

        public void TakeDamage(DamageData dmg)
        {
            var unit = dmg.Target;
            AddParticleTarget(unit, unit, "masteryi_base_w_dmg", unit, flags: 0);
        }

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            var missingHealthBonus = healthTick[spellLevel] * ((owner.Stats.HealthPoints.Total - owner.Stats.CurrentHealth) / owner.Stats.HealthPoints.Total);
            var apBonus = owner.Stats.AbilityPower.Total * 0.3f;
            trueHeal = healthTick[spellLevel] + missingHealthBonus + apBonus;

            var newHealth = owner.Stats.CurrentHealth + trueHeal;
            owner.Stats.CurrentHealth = Math.Min(newHealth, owner.Stats.HealthPoints.Total);

            ApiEventManager.RemoveAllListenersForOwner(this);

            if (buffParticle != null)
            {
                buffParticle.SetToRemove();
            }
        }

        public void OnUpdate(float diff)
        {
            if (tickTime >= 500.0f)
            {
                var missingHealthBonus = healthTick[spellLevel] * ((owner.Stats.HealthPoints.Total - owner.Stats.CurrentHealth) / owner.Stats.HealthPoints.Total);
                var apBonus = owner.Stats.AbilityPower.Total * 0.3f;
                trueHeal = healthTick[spellLevel] + missingHealthBonus + apBonus;

                var newHealth = owner.Stats.CurrentHealth + trueHeal;
                owner.Stats.CurrentHealth = Math.Min(newHealth, owner.Stats.HealthPoints.Total);
                tickTime = 0;
            }

            tickTime += diff;
        }
    }
}
