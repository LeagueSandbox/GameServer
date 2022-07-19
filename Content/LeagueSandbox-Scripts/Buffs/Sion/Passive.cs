using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects;
using            GameServerLib.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;

namespace Buffs
{
    internal class SionPassive : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER,
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };

        public StatsModifier StatsModifier { get; private set; }

        float timer = 0f;
        int tickCount = 0;
        Champion champion;
        Buff thisBuff;
        Particle p;
        Particle p2;
        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            ApiEventManager.OnDeath.AddListener(this, unit, OnDeath, true);
            champion = unit as Champion;
            thisBuff = buff;

            //These Particles aren't working
            //p = AddParticleTarget(unit, null, "Sion_Skin01_Passive_Skin.troy", unit, buff.Duration);
            //p2 = AddParticleTarget(unit, null, "Sion_Skin01_Passive_Ax.troy", unit, buff.Duration);

            for (byte i = 0; i < 4; i++)
            {
                if (champion != null)
                {
                    SetSpell(champion, "SionPassiveSpeed", SpellSlotType.SpellSlots, i, true);
                }
            };
        }

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {            
            string[] originalAbilities = new string[] {"SionQ", "SionW", "SionE", "SionR"};
            for (byte i = 0; i < 4; i++)
            {
                if (champion != null)
                {
                    SetSpell(champion, originalAbilities[i], SpellSlotType.SpellSlots, i, true);
                }
            }

            unit.TakeDamage(unit, 100000f, DamageType.DAMAGE_TYPE_TRUE, DamageSource.DAMAGE_SOURCE_INTERNALRAW, false);

            RemoveBuff(unit, "SionPassiveDelay");
            RemoveBuff(unit, "SionPassive");
            //RemoveParticle(p);
            //RemoveParticle(p2);
        }

        public void OnDeath(DeathData deathData)
        {
            if (thisBuff != null && !thisBuff.Elapsed())
            {
                thisBuff.DeactivateBuff();
            }
        }
        public void OnUpdate(float diff)
        {
            timer += diff;
            if (timer > 250f && champion != null)
            {
                champion.TakeDamage(champion, 1 + champion.Stats.Level + tickCount * (0.7f * (champion.Stats.Level * 0.7f)), DamageType.DAMAGE_TYPE_TRUE, DamageSource.DAMAGE_SOURCE_INTERNALRAW, false);
                timer = 0;
                tickCount++;
            }
        }
    }
}
