using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Spells
{
    public class MasterYiHighlander : IGameScript
    {
        public void OnActivate(IObjAiBase owner)
        {
        }

        private void ReduceCooldown(IAttackableUnit unit, bool isCrit)
        {
            //No Cooldown reduction on the other skills yet
        }

        public void OnDeactivate(IObjAiBase owner)
        {
        }

        public void OnStartCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            var p = AddParticleTarget(owner, "Highlander_buf.troy", target, 1);
            AddBuff("Highlander", 10.0f, 1, spell, (IObjAiBase)target, owner);
            CreateTimer(10.0f, () =>
            {
                RemoveParticle(p);
            });
            //No increased durations on kills and assists yet
        }

        public void OnFinishCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {
        }

        public void OnUpdate(double diff)
        {
        }
    }
}
