using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.GameObjects.Stats;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
    internal class BlessingoftheLizardElder : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER
        };

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        IBuff thisBuff;
        IObjAiBase owner;
        IParticle particle;
        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            thisBuff  = buff;

            if (unit is IChampion champ)
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

        public void OnDeath(IDeathData deathData)
        {
            var unit = deathData.Unit as IObjAiBase;
            var killer = deathData.Killer as IChampion;

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
                if (deathData.Killer is IPet pet)
                {
                    var petOwner = pet.Owner;

                    if (petOwner != null && petOwner is IChampion petChamp && !petChamp.IsDead)
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

        public void OnPreDealDamage(IDamageData data)
        {
            if (data.Attacker is IObjAiBase ai && data.Target is IObjAiBase && !(data.Target is IBaseTurret || data.Target is IObjBuilding))
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

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            ApiEventManager.OnDeath.RemoveListener(this);
            ApiEventManager.OnHitUnit.RemoveListener(this);
            RemoveParticle(particle);
        }

        public void OnUpdate(float diff)
        {

        }
    }
}
