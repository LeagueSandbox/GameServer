using LeagueSandbox.GameServer.API;
using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Scripting.CSharp;
using GameServerCore.Domain;

namespace Passives
{
    public class EvelynnPassive : ICharScript
    {
        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
            ApiEventManager.OnTakeDamage.AddListener(this, owner, SelfWasDamaged, false);
        }
        private void SelfWasDamaged(IDamageData damageData)
        {
        }
        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }
        public void OnUpdate(float diff)
        {
        }
    }
}

