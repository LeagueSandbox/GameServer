using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;

namespace Spells
{
    public class Recall : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true
            // TODO
        };

        public void OnStartCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            owner.StopMovement();
        }

        public void OnFinishCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            // @TODO Interrupt the script when owner uses movement spells
            AddBuff("Recall", 8.0f, 1, spell, owner, owner);
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, ISpellMissile projectile)
        {
        }

        public void OnUpdate(float diff)
        {
        }

        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
            ApiEventManager.OnChampionDamageTaken.AddListener(this, (IChampion)owner, () =>
            {
                if (HasBuff(owner, "Recall"))
                {
                    RemoveBuff(owner, "Recall");
                }
            });
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {

        }
    }
}

