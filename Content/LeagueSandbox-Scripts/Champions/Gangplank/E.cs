using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore;

namespace Spells
{
    public class RaiseMorale : IGameScript
    {
        public void OnActivate(IChampion owner)
        {
        }

        private void SelfWasDamaged()
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
            var hasbuff = owner.HasBuffGameScriptActive("GangplankE", "GangplankE");
            
            if (hasbuff == false)
            {
                var buff = ((ObjAIBase) target).AddBuffGameScript("GangplankE", "GangplankE", spell);
            }
            
            foreach (var allyTarget in GetUnitsInRange(owner, 1000, true)
                .Where(x => x.Team != CustomConvert.GetEnemyTeam(owner.Team)))
            {
                if (allyTarget is IAttackableUnit && owner != allyTarget && hasbuff == false)
                {
                    ((ObjAIBase) allyTarget).AddBuffGameScript("GangplankE", "GangplankE", spell, 7.0f, true);
                }
            }			
        }

        public void ApplyEffects(IChampion owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {
        }

        public void OnUpdate(double diff)
        {
        }
    }
}
