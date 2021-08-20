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


namespace Spells
{
    public class SionQ : ISpellScript //Was supposed to be "SionPassive", but passive scripts aren't getting read 
    {
        ISpell Spell;
        int counter;
        public ISpellScriptMetadata ScriptMetadata => new SpellScriptMetadata()
        {
            TriggersSpellCasts = true
            // TODO
        };

        public void OnActivate(IObjAiBase owner, ISpell spell)
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
        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }
        public void OnSpellPreCast(IObjAiBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {
            owner.TakeDamage(owner, 10000, GameServerCore.Enums.DamageType.DAMAGE_TYPE_TRUE, GameServerCore.Enums.DamageSource.DAMAGE_SOURCE_INTERNALRAW, false);
        }
        public void OnSpellCast(ISpell spell)
        {
        }
        public void OnSpellPostCast(ISpell spell)
        {
        }
        public void OnSpellChannel(ISpell spell)
        {
        }
        public void OnSpellChannelCancel(ISpell spell)
        {
        }
        public void OnSpellPostChannel(ISpell spell)
        {
        }
        public void OnUpdate(float diff)
        {
        }
    }
}

