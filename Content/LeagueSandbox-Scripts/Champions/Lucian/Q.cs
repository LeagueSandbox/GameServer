using System.Numerics;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Spells
{
    public class LucianQ : IGameScript
    {
        public void OnActivate(IChampion owner)
        {
        }

        public void OnDeactivate(IChampion owner)
        {
        }

        public void OnStartCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
            var current = new Vector2(owner.X, owner.Y);
            var to = Vector2.Normalize(new Vector2(spell.X, spell.Y) - current);
            var range = to * 1100;
            var trueCoords = current + range;

            spell.AddLaser("LucianQ", trueCoords.X, trueCoords.Y);
            spell.SpellAnimation("SPELL1", owner);
            AddParticle(owner, "Lucian_Q_laser.troy", trueCoords.X, trueCoords.Y);
            AddParticleTarget(owner, "Lucian_Q_cas.troy", owner);
        }

        public void OnFinishCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
        }

        public void ApplyEffects(IChampion owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {
            var damage = owner.Stats.AttackDamage.Total * (0.45f + spell.Level * 0.15f) + (50 + spell.Level * 30);
            target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_SPELL,
                false);
        }

        public void OnUpdate(double diff)
        {
        }
    }
}
