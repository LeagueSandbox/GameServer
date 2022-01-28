using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using System;
using System.Collections.Generic;
using System.Text;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;


namespace CharScripts
{
    internal class CharScriptLizardElder : ICharScript
    {
        public void OnActivate(IObjAiBase owner, ISpell spell = null)
        {
            AddBuff("GlobalMonsterBuff", 25000.0f, 1, spell, owner, owner, true);
            AddBuff("BlessingoftheLizardElder", 25000.0f, 1, null, owner, owner, true);
        }

        public void OnHitUnit(IDamageData data)
        {
            // TODO: Multiply damage in data (currently unsupported).
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell = null)
        {
        }
        public void OnUpdate(float diff)
        {
        }
    }
}
