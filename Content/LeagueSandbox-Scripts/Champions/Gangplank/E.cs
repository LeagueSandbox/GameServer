using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Domain;
using GameServerCore.Enums;
using GameServerCore;
using System.Linq;

namespace Spells
{
    public class RaiseMorale : IGameScript
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
            var hasbuff = owner.HasBuff("GangplankE");
            
            if (hasbuff == false)
            {
                AddBuff("GangplankE", 7.0f, 1, spell, owner, owner);
            }
            
            foreach (var allyTarget in GetUnitsInRange(owner, 1000, true)
                .Where(x => x.Team != CustomConvert.GetEnemyTeam(owner.Team)))
            {
                if (allyTarget is IAttackableUnit && owner != allyTarget && hasbuff == false)
                {
                    AddBuff("GangplankE", 7.0f, 1, spell, (IObjAiBase) allyTarget, owner);
                }
            }			
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {
        }

        public void OnUpdate(double diff)
        {
        }
    }
}
