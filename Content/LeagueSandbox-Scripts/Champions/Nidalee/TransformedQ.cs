using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Spells
{
    class Takedown : IGameScript
    {
        public void OnStartCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
            owner.AddBuffGameScript("NidaleeTransformedQBuff", "NidaleeTransformedQBuff", spell, 5f, true);

            owner.ChangeAutoAttackSpellData("NidaleeCougarTakedownAttack");
        }

        public void OnFinishCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
            
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
