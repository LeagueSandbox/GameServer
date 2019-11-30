using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Spells
{
    public class AkaliTwinDisciplines : ISpellScript
    {
        public void OnActivate(IChampion owner)
        {
            var bonusAd = owner.Stats.AttackDamage.Total - owner.Stats.AttackDamage.BaseValue;
            owner.Stats.SpellVamp.PercentBonus = 6 + bonusAd % 6;
        }

        public void OnDeactivate(IChampion owner)
        {
        }

        public void OnStartCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
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

        public void CooldownStarted(IChampion owner, ISpell spell)        {            //Executed once spell cooldown started        }

        public void CooldownEnded(IChampion owner, ISpell spell)
        {
            //Executed when cooldown is over
        }
    }
}

