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
    internal class CharScriptHA_AP_HealthRelic : ICharScript
    {
        public void OnActivate(IObjAiBase owner, ISpell spell = null)
        {
            SetStatus(owner, StatusFlags.CanMove, false);
            SetStatus(owner, StatusFlags.Ghosted, true);
            SetStatus(owner, StatusFlags.SuppressCallForHelp, true);
            SetStatus(owner, StatusFlags.IgnoreCallForHelp, true);
            SetStatus(owner, StatusFlags.ForceRenderParticles, true);

            AddBuff("HowlingAbyssRelicAura", 25000.0f, 1, null, owner, owner, false);
        }
        public void OnDeactivate(IObjAiBase owner, ISpell spell = null)
        {
        }
        public void OnUpdate(float diff)
        {

        }
    }
}
