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
        public void OnStartCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            //IChampion champion = (IChampion)owner;
            //champion.IsRecalling = true;
        }

        public void OnFinishCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            AddBuff("Recall", 8.0f, 1, spell, owner, owner);
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {
        }

        public void OnUpdate(double diff)
        {
        }

        public void OnActivate(IObjAiBase owner)
        {
            void HandleRemoveBuff()
            {
                if (HasBuff(owner, "Recall"))
                {
                    RemoveBuff(owner, "Recall");
                }
            }

            ApiEventManager.OnChampionDamageTaken.AddListener(this, (IChampion)owner, HandleRemoveBuff);
            ApiEventManager.OnChampionMove.AddListener(this, (IChampion)owner, HandleRemoveBuff);
            ApiEventManager.OnChampionCrowdControlled.AddListener(this, (IChampion)owner, HandleRemoveBuff);
        }

        public void OnDeactivate(IObjAiBase owner)
        {

        }
    }
}

