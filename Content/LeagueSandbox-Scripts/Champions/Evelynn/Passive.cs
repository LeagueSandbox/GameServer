using LeagueSandbox.GameServer.API;
using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;

namespace Spells
{
    public class EvelynnPassive : IGameScript
    {
        public void OnActivate(IObjAiBase owner)
        {
            ApiEventManager.OnChampionDamageTaken.AddListener(this, (IChampion)owner, SelfWasDamaged);
        }

        private void SelfWasDamaged()
        {
        }

        public void OnDeactivate(IObjAiBase owner)
        {
            //Listeners are automatically removed when GameScripts deactivate
        }

        public void OnStartCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
        }

        public void OnFinishCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, ISpellMissile projectile)
        {
        }

        public void OnUpdate(double diff)
        {
        }
    }
}

