using System.Numerics;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Domain.GameObjects.Spell.Missile;

namespace Spells
{
    public class EzrealArcaneShift : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata => new SpellScriptMetadata()
        {
            TriggersSpellCasts = true
            // TODO
        };

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
            var current = new Vector2(owner.Position.X, owner.Position.Y);
            var spellPos = new Vector2(spell.CastInfo.TargetPosition.X, spell.CastInfo.TargetPosition.Z);
            var to = spellPos - current;
            Vector2 trueCoords;
            if (to.Length() > 475)
            {
                to = Vector2.Normalize(to);
                var range = to * 475;
                trueCoords = current + range;
            }
            else
            {
                trueCoords = spellPos;
            }

            AddParticle(owner, "Ezreal_arcaneshift_cas.troy", owner.Position);
            TeleportTo(owner, trueCoords.X, trueCoords.Y);
            AddParticleTarget(owner, "Ezreal_arcaneshift_flash.troy", owner);
            IAttackableUnit target2 = null;
            var units = GetUnitsInRange(current, 700, true);
            float sqrDistance = 700 * 700;
            foreach (var value in units)
            {
                if (owner.Team != value.Team && value is IObjAiBase)
                {
                    if (Vector2.DistanceSquared(trueCoords, value.Position) <=
                        sqrDistance)
                    {
                        target2 = value;
                        sqrDistance = Vector2.DistanceSquared(trueCoords, value.Position);
                    }
                }
            }

            if (target2 != null)
            {
                if (!(target2 is IBaseTurret))
                {
                    //spell.AddProjectileTarget("EzrealArcaneShiftMissile", new Vector3(trueCoords.X, owner.GetHeight() + 150.0f, trueCoords.Y), target2, overrideCastPosition: true);
                }
            }
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, ISpellMissile missile)
        {
            target.TakeDamage(owner, 25f + spell.CastInfo.SpellLevel * 50f + owner.Stats.AbilityPower.Total * 0.75f,
                DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
            missile.SetToRemove();
        }

        public void OnSpellChannel(ISpell spell)
        {
        }

        public void OnSpellChannelCancel(ISpell spell)
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
