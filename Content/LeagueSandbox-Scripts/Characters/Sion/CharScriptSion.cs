using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using System.Numerics;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using GameServerCore.Domain;
using GameServerLib.GameObjects.AttackableUnits;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;


namespace CharScripts
{
    public class CharScriptSion : ICharScript
    {
        ISpell Spell;
        int counter;
        public void OnActivate(IObjAiBase owner, ISpell spell = null)
        {
            ApiEventManager.OnDeath.AddListener(this, owner, OnDeath, true);
            ApiEventManager.OnResurrect.AddListener(this, owner, OnRessurect, false);
            Spell = spell;
        }
        public void OnDeath(IDeathData deathData)
        {
            AddBuff("SionPassiveDelay", 2f, 1, Spell, deathData.Unit, deathData.Unit as IObjAiBase);
        } 
        public void OnRessurect(IObjAiBase owner)
        {
            counter++;
            //This is to avoid a loop in his passive.
            if (counter == 2)
            {
                ApiEventManager.OnDeath.AddListener(this, owner, OnDeath, true); 
                counter = 0;
            }
        }
        public void OnDeactivate(IObjAiBase owner, ISpell spell = null)
        {
        }
        public void OnUpdate(float diff)
        {
        }
    }
}

