using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Spells
{
    public class Recall : IGameScript
    {
        public void OnStartCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
        }

        public void OnFinishCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
            // @TODO Interrupt the script when owner uses movement spells
            owner.AddBuffGameScript("Recall", "Recall", spell, 8.0f, true);
        }

        public void ApplyEffects(IChampion owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {
        }

        public void OnUpdate(double diff)
        {
        }

        public void OnActivate(IChampion owner)
        {
            ApiEventManager.OnChampionDamageTaken.AddListener(this, owner, () =>
            {
                if (owner.HasBuffGameScriptActive("Recall", "Recall"))
                {
                    ((ObjAiBase)owner).RemoveBuffGameScriptsWithName("Recall", "Recall");
                }
            });
        }

        public void OnDeactivate(IChampion owner)
        {

        }
    }
}

