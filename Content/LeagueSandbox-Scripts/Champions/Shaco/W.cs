using GameServerCore;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Enums;
using System.Numerics;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;

namespace Spells
{
    public class JackInTheBox : IGameScript
    {
        public float petTimeAlive = 0.00f;

        public void OnActivate(IObjAiBase owner)
        {
        }

        private void OnUnitHit(IAttackableUnit target, bool isCrit)
        {
        }

        public void OnDeactivate(IObjAiBase owner)
        {
        }

        public void OnStartCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
        }

        public void OnFinishCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            var castrange = spell.SpellData.CastRange[0];
            var apbonus = owner.Stats.AbilityPower.Total * 0.2f;
            var damage = 35 + ((15 * (spell.Level - 1)) + apbonus); //TODO: Should replace minion AA damage
            var jackduration = 5.0f; //TODO: Split into Active duration and Hidden duration when Invisibility is implemented
            var attspeed = 1 / 1.8f; // 1.8 attacks a second = ~.56 seconds per attack, could not extrapolate from minion stats
            //TODO: Implement Fear buff and ShacoBoxSpell
            //var fearrange = 300;
            var fearduration = 0.5f + (0.25 * (spell.Level - 1));
            var ownerPos = new Vector2(owner.X, owner.Y);
            var spellPos = new Vector2(spell.X, spell.Y);

            if (owner.WithinRange(ownerPos, spellPos, castrange))
            {
                IMinion m = AddMinion((IChampion)owner, "ShacoBox", "ShacoBox", spell.X, spell.Y);
                AddParticle(owner, "JackintheboxPoof.troy", spell.X, spell.Y);

                var attackrange = m.Stats.Range.Total;

                if (m.IsVisibleByTeam(owner.Team))
                {
                    if (!m.IsDead)
                    {
                        var units = GetUnitsInRange(m, attackrange, true);
                        foreach (var value in units)
                        {
                            if (owner.Team != value.Team && value is IAttackableUnit && !(value is IBaseTurret) && !(value is IObjAnimatedBuilding))
                            {
                                //TODO: Change TakeDamage to activate on Jack AutoAttackHit, not use CreateTimer, and make Pets use owner spell stats
                                m.SetTargetUnit(value);
                                m.AutoAttackTarget = value;
                                m.AutoAttackProjectileSpeed = 1450;
                                m.AutoAttackHit(value);
                                for (petTimeAlive = 0.0f; petTimeAlive < jackduration; petTimeAlive += attspeed)
                                {
                                    CreateTimer(petTimeAlive, () => {
                                        if (!value.IsDead && !m.IsDead)
                                        {
                                            value.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
                                        }
                                    });
                                }
                            }
                        }
                        CreateTimer(jackduration, () =>
                        {
                            if (!m.IsDead)
                            {
                                m.Die(m); //TODO: Fix targeting issues
                            }
                        });
                    }
                }
            }
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {
        }

        public void OnUpdate(double diff)
        {
        }
    }
}
