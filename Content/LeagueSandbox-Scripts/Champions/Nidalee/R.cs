using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.Content;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace Spells
{
    class AspectOfTheCougar : IGameScript
    {
        private bool _transformed;

        public void OnStartCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
            
        }

        public void OnFinishCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
            
        }

        private void Transform(IChampion owner)
        {
            if (!_transformed)
            {
                owner.SetSpell("Takedown", 0, true);
                owner.SetSpell("Pounce", 1, true);
                owner.SetSpell("Swipe", 2, true);
            }
            else
            {
                owner.SetSpell("JavelinToss", 0, true);
                owner.SetSpell("Bushwhack", 1, true);
                owner.SetSpell("PrimalSurge", 2, true);
            }
        }

        public void ApplyEffects(IChampion owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {
            if (!_transformed)
            {
                owner.ChangeModel("Nidalee_Cougar");
                owner.ResetAutoAttackSpellData();
                owner.Stats.Range.BaseValue = 125;

                Transform(owner);

                _transformed = true;
            }
            else
            {
                owner.ChangeModel("Nidalee");
                owner.ResetAutoAttackSpellData();
                owner.Stats.Range.BaseValue = 525;

                Transform(owner);

                _transformed = false;
            }
        }

        public void OnActivate(IChampion owner)
        {
            
        }

        public void OnDeactivate(IChampion owner)
        {
            
        }
    }
}
