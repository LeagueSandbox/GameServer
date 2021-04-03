using System.Numerics;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;

namespace Spells
{
    public class BlindMonkQOne : IGameScript
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
            var to = Vector2.Normalize(new Vector2(spell.X, spell.Y) - current);
            var range = to * 1150;
            var trueCoords = current + range;
            spell.AddProjectile("BlindMonkQOne", current, trueCoords);
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, ISpellMissile projectile)
        {
            var ad = owner.Stats.AttackDamage.Total * 0.9f;
            var damage = 50 + (spell.Level * 30) + ad;
            target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_ATTACK, false);
            AddParticleTarget(owner, "blindMonk_Q_resonatingStrike_tar.troy", target, 1, "C_BuffBone_Glb_Center_Loc");
            AddParticleTarget(owner, "blindMonk_Q_tar.troy", target, 1, "C_BuffBone_Glb_Center_Loc");
            if (target is IObjAiBase u)
            {
                AddBuff("BlindMonkSonicWave", 3f, 1, spell, u, owner);
            }

            projectile.SetToRemove();
            if (Vector2.DistanceSquared(owner.Position, target.Position) <= 800 * 800 && !target.IsDead)
            {
                ForceMovement(owner, target, "Spell1b", 2200, 0, 0, 0, 20000);
            }
        }

        public void OnUpdate(float diff)
        {
        }
    }
}
