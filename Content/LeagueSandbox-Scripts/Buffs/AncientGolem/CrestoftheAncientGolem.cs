using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using GameServerLib.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace Buffs
{
    internal class CrestoftheAncientGolem : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER
        };

        public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

        Buff thisBuff;
        Particle particle;
        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            thisBuff  = buff;

            if (unit is Champion champ)
            {
                particle = AddParticleTarget(unit, unit, "NeutralMonster_buf_blue_defense", unit, buff.Duration);
                // TODO: Separate Mana and PAR stat mods (energy is modified differently from Mana, same with other types)
                StatsModifier.ManaRegeneration.FlatBonus += 5 + unit.Stats.ManaPoints.Total * 0.05f;
                StatsModifier.CooldownReduction.FlatBonus += 0.1f;
                unit.AddStatModifier(StatsModifier);
            }
            else
            {
                particle = AddParticleTarget(unit, unit, "NeutralMonster_buf_blue_defense_big", unit, buff.Duration);
            }

            ApiEventManager.OnDeath.AddListener(this, unit, OnDeath, false);

            // TODO: CrestoftheAncientGolemLines?
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

                AddBuff("CrestoftheAncientGolem", duration, 1, null, killer, unit);
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

                        AddBuff("CrestoftheAncientGolem", duration, 1, null, killer, unit);
                    }
                }
            }
        }

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            ApiEventManager.OnDeath.RemoveListener(this);
            RemoveParticle(particle);
        }

        public void OnUpdate(float diff)
        {

        }
    }
}
