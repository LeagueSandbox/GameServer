using LeagueSandbox.GameServer.API;
using GameServerCore.Scripting.CSharp;
using            GameServerLib.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;

namespace CharScripts
{
    public class CharScriptEvelynn : ICharScript
    {
        public void OnActivate(ObjAIBase owner, Spell spell = null)
        {
            ApiEventManager.OnTakeDamage.AddListener(this, owner, SelfWasDamaged, false);
        }
        private void SelfWasDamaged(DamageData damageData)
        {
        }
    }
}

