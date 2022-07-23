using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.GameObjects;
using            GameServerLib.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.Buildings;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
    internal class BlessingoftheLizardElder : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER
        };

        public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

        Buff thisBuff;
        ObjAIBase owner;
        Particle particle;
        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            thisBuff  = buff;

            if (unit is Champion champ)
            {
                particle = AddParticleTarget(unit, unit, "NeutralMonster_buf_red_offense", unit, buff.Duration);
            }
            else
            {
                particle = AddParticleTarget(unit, unit, "NeutralMonster_buf_red_offense_big", unit, buff.Duration);
            }

            ApiEventManager.OnDeath.AddListener(this, unit, OnDeath, false);
            ApiEventManager.OnPreDealDamage.AddListener(this, unit, OnPreDealDamage, false);
        }

        public void OnDeath(DeathData deathData)
        {
            var unit = deathData.Unit as ObjAIBase;
            var killer = deathData.Killer as Champion;

            if (unit != null && killer != null && !killer.IsDead)
            {
                thisBuff.DeactivateBuff();

                // Talent ID 4332 (Runic Affinity)
                var duration = 150f;
                if (HasBuff(deathData.Killer, "MonsterBuffs"))
                {
                    duration *= 1.2f;
                }

                AddBuff("BlessingoftheLizardElder", duration, 1, null, killer, unit);
            }
            else if (killer == null)
            {
                if (deathData.Killer is Pet pet)
                {
                    var petOwner = pet.Owner;

                    if (petOwner != null && petOwner is Champion petChamp && !petChamp.IsDead)
                    {
                        var duration = 150f;
                        if (HasBuff(deathData.Killer, "MonsterBuffs"))
                        {
                            duration *= 1.2f;
                        }

                        AddBuff("BlessingoftheLizardElder", duration, 1, null, killer, unit);
                    }
                }
            }
        }

        public void OnPreDealDamage(DamageData data)
        {
            if (data.Attacker is ObjAIBase ai && data.Target is ObjAIBase && !(data.Target is BaseTurret || data.Target is ObjBuilding))
            {
                if (data.DamageSource == DamageSource.DAMAGE_SOURCE_ATTACK)
                {
                    AddBuff("Burning", 3.0f, 1, null, data.Target, ai);
                    // TODO: Find out how we should handle dynamic buff stats (League transfers data from the parent buff to the basic slow buff).
                    var slowBuffScript = AddBuff("Slow", 3.0f, 1, null, data.Target, ai).BuffScript as Slow;
                    
                    float slow = 0.08f;
                    if (!ai.CharData.IsMelee || HasBuff(ai, "JudicatorRighteousFury"))
                    {
                        slow = 0.05f;
                    }

                    if (ai.Stats.Level > 5 && ai.Stats.Level < 11)
                    {
                        slow *= 2;
                    }
                    else if (ai.Stats.Level > 10)
                    {
                        slow *= 3;
                    }

                    // TODO: Find a better way to transfer data between scripts.
                    slowBuffScript.SetSlowMod(slow);
                }
            }
        }

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            ApiEventManager.OnDeath.RemoveListener(this);
            ApiEventManager.OnHitUnit.RemoveListener(this);
            RemoveParticle(particle);
        }
    }
}
