using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;

namespace Spells
{
    public class Recall : IGameScript
    {
        public void OnStartCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
        }

        public void OnFinishCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            // @TODO Interrupt the script when owner uses movement spells
            AddBuff("Recall", 8.0f, 1, spell, owner, owner);
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, ISpellMissile projectile)
        {
        }

        public void OnUpdate(double diff)
        {
        }

        public void OnActivate(IObjAiBase owner)
        {
            ApiEventManager.OnChampionDamageTaken.AddListener(this, (IChampion)owner, () =>
            {
                if (HasBuff(owner, "Recall"))
                {
                    RemoveBuff(owner, "Recall");
                }
            });
        }

        public void OnDeactivate(IObjAiBase owner)
        {

        }
    }
}

