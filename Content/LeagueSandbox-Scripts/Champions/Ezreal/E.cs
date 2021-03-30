using System.Numerics;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;

namespace Spells
{
    public class EzrealArcaneShift : IGameScript
    {
        public void OnActivate(IObjAiBase owner)
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
            var current = new Vector2(owner.Position.X, owner.Position.Y);
            var to = new Vector2(spell.X, spell.Y) - current;
            Vector2 trueCoords;
            if (to.Length() > 475)
            {
                to = Vector2.Normalize(to);
                var range = to * 475;
                trueCoords = current + range;
            }
            else
            {
                trueCoords = new Vector2(spell.X, spell.Y);
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
                    spell.AddProjectileTarget("EzrealArcaneShiftMissile", target2);
                }
            }
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, ISpellMissile projectile)
        {
            target.TakeDamage(owner, 25f + spell.Level * 50f + owner.Stats.AbilityPower.Total * 0.75f,
                DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
            projectile.SetToRemove();
        }

        public void OnUpdate(double diff)
        {
        }
    }
}
