using System.Numerics;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Spells
{
    public class RocketGrab : IGameScript
    {
        public void OnActivate(IObjAiBase owner)
        {
        }

        public void OnDeactivate(IObjAiBase owner)
        {
        }

        public void OnStartCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            spell.SpellAnimation("SPELL1", owner);
        }

        public void OnFinishCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            var current = new Vector2(owner.X, owner.Y);
            var to = Vector2.Normalize(new Vector2(spell.X, spell.Y) - current);
            var range = to * 925;
            var trueCoords = current + range;
            spell.AddProjectile("RocketGrabMissile", owner.X, owner.Y, trueCoords.X, trueCoords.Y);
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {
            var ap = owner.Stats.AbilityPower.Total;
            var damage = 25 + spell.Level * 55 + ap;
            target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
            if (!target.IsDead)
            {
                AddParticleTarget(owner, "Blitzcrank_Grapplin_tar.troy", target, 1, "L_HAND");
                var current = new Vector2(owner.X, owner.Y);
                var to = Vector2.Normalize(new Vector2(spell.X, spell.Y) - current);
                var range = to * 50;
                var trueCoords = current + range;
                DashToLocation((ObjAiBase)target, trueCoords.X, trueCoords.Y,
                    spell.SpellData.MissileSpeed, true);
            }

            projectile.SetToRemove();
        }

        public void OnUpdate(double diff)
        {
        }
    }
}

