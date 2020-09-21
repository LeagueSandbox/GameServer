using GameServerCore;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Enums;
using GameServerCore.Domain;
using System;

namespace Spells
{
    public class RemoveScurvy : IGameScript
    {
        public void OnActivate(IObjAiBase owner)
        {

        }

        private void SelfWasDamaged()
        {
        }

        public void OnDeactivate(IObjAiBase owner)
        {

        }

        public void OnStartCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {	
        }

        public void OnFinishCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            float ap = owner.Stats.AbilityPower.Total; //100% AP Ratio
            float newHealth = target.Stats.CurrentHealth + 80 + ap;
            target.Stats.CurrentHealth = Math.Min(newHealth, target.Stats.HealthPoints.Total);
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {
        }

        public void OnUpdate(double diff)
        {
        }
    }
}
