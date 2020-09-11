using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace Spells
{
    public class Recall : IGameScript
    {
        protected IBuff _buff;

        public void OnStartCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
        }

        public void OnFinishCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            _buff = AddBuff("Recall", 8.0f, 1, spell, owner, owner);
            ApiEventManager.OnChampionMove.AddListener(this, (IChampion)owner, _buff.DeactivateBuff);
            ApiEventManager.OnChampionDamageTaken.AddListener(this, (IChampion)owner, _buff.DeactivateBuff);
            ApiEventManager.OnChampionCrowdControlled.AddListener(this, (IChampion)owner, _buff.DeactivateBuff);
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {
        }

        public void OnUpdate(double diff)
        {
        }

        public void OnActivate(IObjAiBase owner)
        {
        }

        public void OnDeactivate(IObjAiBase owner)
        {

        }
    }
}

