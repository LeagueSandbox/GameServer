using System.Numerics;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace Buffs
{
    internal class CrestoftheAncientGolem : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER
        };

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        IBuff thisBuff;
        IParticle particle;
        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            thisBuff  = buff;

            if (unit is IChampion champ)
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

                AddBuff("CrestoftheAncientGolem", duration, 1, null, killer, unit);
            }
            else if (killer == null)
            {
                if (killer is IPet pet)
                {
                    var petOwner = pet.Owner;

                    if (petOwner != null && petOwner is IChampion petChamp && !petChamp.IsDead)
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

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            ApiEventManager.OnDeath.RemoveListener(this);
            RemoveParticle(particle);
        }

        public void OnUpdate(float diff)
        {

        }
    }
}
