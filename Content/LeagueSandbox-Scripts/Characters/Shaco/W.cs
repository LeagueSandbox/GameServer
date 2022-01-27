using GameServerCore;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Enums;
using System.Numerics;
using GameServerCore.Scripting.CSharp;

namespace Spells
{
    public class JackInTheBox : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true
            // TODO
        };

        public float petTimeAlive = 0.00f;

        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnSpellPreCast(IObjAiBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {
        }

        public void OnSpellCast(ISpell spell)
        {
        }

        public void OnSpellPostCast(ISpell spell)
        {
            var owner = spell.CastInfo.Owner;
            var castrange = spell.GetCurrentCastRange();
            var apbonus = owner.Stats.AbilityPower.Total * 0.2f;
            var damage = 35 + ((15 * (spell.CastInfo.SpellLevel - 1)) + apbonus); //TODO: Should replace minion AA damage
            var jackduration = 5.0f; //TODO: Split into Active duration and Hidden duration when Invisibility is implemented
            var attspeed = 1 / 1.8f; // 1.8 attacks a second = ~.56 seconds per attack, could not extrapolate from minion stats
            //TODO: Implement Fear buff and ShacoBoxSpell
            //var fearrange = 300;
            var fearduration = 0.5f + (0.25 * (spell.CastInfo.SpellLevel - 1));
            var ownerPos = owner.Position;
            var spellPos = new Vector2(spell.CastInfo.TargetPosition.X, spell.CastInfo.TargetPosition.Z);

            if (Extensions.IsVectorWithinRange(ownerPos, spellPos, castrange))
            {
                IMinion m = AddMinion((IChampion)owner, "ShacoBox", "ShacoBox", spellPos, owner.Team);
                AddParticle(owner, null, "JackintheboxPoof", spellPos);

                var attackrange = m.Stats.Range.Total;

                if (m.IsVisibleByTeam(owner.Team))
                {
                    if (!m.IsDead)
                    {
                        var units = GetUnitsInRange(m.Position, attackrange, true);
                        foreach (var value in units)
                        {
                            if (owner.Team != value.Team && value is IAttackableUnit && !(value is IBaseTurret) && !(value is IObjAnimatedBuilding))
                            {
                                //TODO: Change TakeDamage to activate on Jack AutoAttackHit, not use CreateTimer, and make Pets use owner spell stats
                                m.SetTargetUnit(value);
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
                                //TODO: Fix targeting issues
                                m.TakeDamage(m.Owner, 1000f, DamageType.DAMAGE_TYPE_TRUE, DamageSource.DAMAGE_SOURCE_INTERNALRAW, DamageResultType.RESULT_NORMAL);
                            }
                        });
                    }
                }
            }
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
