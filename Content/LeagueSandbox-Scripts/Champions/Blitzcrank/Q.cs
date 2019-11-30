using System.Numerics;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameMaths;

namespace Spells
{
    public class RocketGrab : ISpellScript
    {
        public void OnActivate(IChampion owner)
        {
        }

        public void OnDeactivate(IChampion owner)
        {
        }

        public void OnStartCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
            spell.SpellAnimation("SPELL1", owner);
        }

        public void OnFinishCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
            var current = new Vector2(owner.X, owner.Y);
            var to = Vector2.Normalize(new Vector2(spell.X, spell.Y) - current);
            var range = to * 1050;
            var trueCoords = current + range;

            spell.AddProjectile("RocketGrabMissile", owner.X, owner.Y, trueCoords.X, trueCoords.Y);
            FaceDirection(owner, trueCoords, false);
        }

        public void ApplyEffects(IChampion owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {
            if (!(target is IChampion)) return;

            var ap = owner.Stats.AbilityPower.Total;
            var damage = 25 + spell.Level * 55 + ap;
            target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);

            if (!target.IsDead)
            {
                AddParticleTarget(owner, "Blitzcrank_Grapplin_tar.troy", target, 1, "L_HAND");

                var trueCoords = owner.GetPosition().ExtendInDirection(target.GetPosition(), 50);
                DashToLocation((ObjAiBase) target, trueCoords.X, trueCoords.Y, spell.SpellData.MissileSpeed, true);
            }

            projectile.SetToRemove();
        }

        public void OnUpdate(double diff)
        {
        }

        public void CooldownStarted(IChampion owner, ISpell spell)
        {
            //Executed once spell cooldown started
        }

        public void CooldownEnded(IChampion owner, ISpell spell)
        {
            //Executed when cooldown is over
        }
    }
}

