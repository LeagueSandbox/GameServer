using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;


namespace CharScripts
{
    internal class CharScriptOdinCenterRelic : ICharScript
    {
        public void OnActivate(IObjAiBase owner, ISpell spell = null)
        {
            SetStatus(owner, StatusFlags.MagicImmune, true);
            SetStatus(owner, StatusFlags.PhysicalImmune, true);
            SetStatus(owner, StatusFlags.CanAttack, false);
            SetStatus(owner, StatusFlags.CanMove, false);

            AddBuff("OdinBombBuff", 25000.0f, 1, null, owner, owner, false);
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell = null)
        {
        }

        public void OnUpdate(float diff)
        {
        }
    }
}
