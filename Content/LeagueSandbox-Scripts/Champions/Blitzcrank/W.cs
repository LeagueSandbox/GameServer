using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Spells
{
    public class Overdrive : ISpellScript
    {
        public void OnActivate(IChampion owner)
        {
        }

        public void OnDeactivate(IChampion owner)
        {
        }

        public void OnStartCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
            var p = AddParticleTarget(owner, "Overdrive_buf.troy", target, 1);
            ((ObjAiBase)target).AddBuffGameScript("Overdrive", "Overdrive", spell, 8.0f);
            CreateTimer(8.0f, () =>
            {
                RemoveParticle(p);
            });
        }

        public void OnFinishCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
        }

        public void ApplyEffects(IChampion owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {
        }

        public void OnUpdate(double diff)
        {
        }

        public void CooldownStarted(IChampion owner, ISpell spell)
        {
            
        }

        public void CooldownEnded(IChampion owner, ISpell spell)
        {
            
        }
    }
}
