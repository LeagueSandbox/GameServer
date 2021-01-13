using System.Numerics;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.Scripting.CSharp;

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

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
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
                DashToTarget(owner, target, 2200, "Spell1b", 0, false, 20000, 0, 0);
            }
        }

        public void OnUpdate(float diff)
        {
        }
    }
}
