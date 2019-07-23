using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Spells
{
    class PrimalSurge : IGameScript
    {
        public void OnStartCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {

        }

        public void OnFinishCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
            var championSpellAp = owner.Stats.AbilityPower.Total * 0.5f;
            var healAmount = (45 * spell.Level) + championSpellAp;

            target.Stats.CurrentHealth += healAmount;

            ((ObjAiBase) target).AddBuffGameScript("NidaleeEBuff", "NidaleeEBuff", spell, 7f, true);
        }

        public void ApplyEffects(IChampion owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {
            
        }

        public void OnActivate(IChampion owner)
        {
            
        }

        public void OnDeactivate(IChampion owner)
        {
            
        }
    }
}
