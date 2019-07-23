using System.Numerics;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Spells
{
    public class LuxMaliceCannon : IGameScript
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
            var range = to * 3340;
            var trueCoords = current + range;

            spell.AddLaser("LuxMaliceCannon", trueCoords.X, trueCoords.Y);
            AddParticle(owner, "LuxMaliceCannon_beam.troy", trueCoords.X, trueCoords.Y);
            FaceDirection(owner, trueCoords, false);
            spell.SpellAnimation("SPELL4", owner);
            AddParticleTarget(owner, "LuxMaliceCannon_cas.troy", owner);
        }

        public void OnFinishCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
        }

        public void ApplyEffects(IChampion owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {
            target.TakeDamage(owner, 200f + spell.Level * 100f + owner.Stats.AbilityPower.Total * 0.75f,
                DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
        }

        public void OnUpdate(double diff)
        {
        }
    }
}
