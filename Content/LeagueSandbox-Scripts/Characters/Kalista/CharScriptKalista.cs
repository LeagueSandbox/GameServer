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
    internal class CharScriptKalista : ICharScript
    {
        public void OnActivate(IObjAiBase owner, ISpell spell = null)
        {
            ApiEventManager.OnPreAttack.AddListener(this, owner, OnPreAttack, false);
        }

        public void OnPreAttack(ISpell spell)
        {
            if (spell.CastInfo.Targets.Count > 0)
            {
                var target = spell.CastInfo.Targets[0].Unit;
                FaceDirection(target.Position, target, true);
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
