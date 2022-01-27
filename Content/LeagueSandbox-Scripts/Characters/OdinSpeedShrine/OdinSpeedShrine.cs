using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using System;
using System.Collections.Generic;
using System.Text;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;


namespace CharScripts
{
    internal class CharScriptOdinSpeedShrine : ICharScript
    {
        public void OnActivate(IObjAiBase owner, ISpell spell = null)
        {
            SetStatus(owner, StatusFlags.Rooted, true);
            SetStatus(owner, StatusFlags.CanMove, false);
            SetStatus(owner, StatusFlags.Ghosted, true);
            SetStatus(owner, StatusFlags.Targetable, false);
            SetStatus(owner, StatusFlags.SuppressCallForHelp, true);
            SetStatus(owner, StatusFlags.IgnoreCallForHelp, true);
            SetStatus(owner, StatusFlags.ForceRenderParticles, true);

            AddBuff("OdinSpeedShrineAura", 25000.0f, 1, null, owner, owner, false);
        }
        public void OnDeactivate(IObjAiBase owner, ISpell spell = null)
        {
        }
        public void OnUpdate(float diff)
        {
        }
    }
}
