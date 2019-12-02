using System.Numerics;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Spells
{
    public class GravesMove : IGameScript
    {
        public void OnActivate(IChampion owner)
        {
        }

        public void OnDeactivate(IChampion owner)
        {
        }

        public void OnStartCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
        }

        public void OnFinishCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
            var current = new Vector2(owner.X, owner.Y);
            var to = Vector2.Normalize(new Vector2(spell.X, spell.Y) - current);
            var range = to * 425;
            var trueCoords = current + range;

            DashToLocation(owner, trueCoords.X, trueCoords.Y, 1200, false, "Spell3");
            owner.AddBuffGameScript("Quickdraw", "Quickdraw", spell, 4.0f, true);
            var p = AddParticleTarget(owner, "Graves_Move_OnBuffActivate.troy", owner);
            CreateTimer(4.0f, () =>
            {
                RemoveParticle(p);
            });
        }

        public void ApplyEffects(IChampion owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {
        }

        public void OnUpdate(double diff)
        {
        }
    }
}
