using LeagueSandbox.GameServer.API;
using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.Missiles;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Spells
{
    public class EvelynnPassive : ISpellScript
    {
        public void OnActivate(IChampion owner)
        {
            ApiEventManager.OnChampionDamageTaken.AddListener(this, owner, SelfWasDamaged);
        }

        private void SelfWasDamaged()
        {
        }

        public void OnDeactivate(IChampion owner)
        {
            //Listeners are automatically removed when GameScripts deactivate
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

