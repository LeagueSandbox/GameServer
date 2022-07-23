using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using            GameServerLib.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace CharScripts
{
    public class CharScriptSion : ICharScript
    {
        Spell Spell;
        int counter;
        public void OnActivate(ObjAIBase owner, Spell spell = null)
        {
            ApiEventManager.OnDeath.AddListener(this, owner, OnDeath, true);
            ApiEventManager.OnResurrect.AddListener(this, owner, OnRessurect, false);
            Spell = spell;
        }
        public void OnDeath(DeathData deathData)
        {
            AddBuff("SionPassiveDelay", 2f, 1, Spell, deathData.Unit, deathData.Unit as ObjAIBase);
        } 
        public void OnRessurect(ObjAIBase owner)
        {
            counter++;
            //This is to avoid a loop in his passive.
            if (counter == 2)
            {
                ApiEventManager.OnDeath.AddListener(this, owner, OnDeath, true); 
                counter = 0;
            }
        }
    }
}

