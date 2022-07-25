using GameServerCore;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Enums;
using System.Numerics;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.Buildings.AnimatedBuildings;
using LeagueSandbox.GameServer.GameObjects.SpellNS;

namespace Spells
{
    public class JackInTheBox : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true
            // TODO
        };

        public float petTimeAlive = 0.00f;

        public void OnSpellPostCast(Spell spell)
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
                Minion m = AddMinion((Champion)owner, "ShacoBox", "ShacoBox", spellPos, owner.Team);
                AddParticle(owner, null, "JackintheboxPoof", spellPos);

                var attackrange = m.Stats.Range.Total;

                if (m.IsVisibleByTeam(owner.Team))
                {
                    if (!m.IsDead)
                    {
                        var units = GetUnitsInRange(m.Position, attackrange, true);
                        foreach (var value in units)
                        {
                            if (owner.Team != value.Team && value is AttackableUnit && !(value is BaseTurret) && !(value is ObjAnimatedBuilding))
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
    }
}
