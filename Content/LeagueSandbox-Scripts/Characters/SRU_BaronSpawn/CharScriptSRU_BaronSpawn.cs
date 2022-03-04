using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace CharScripts
{
    internal class CharScriptSRU_BaronSpawn : ICharScript
    {
        public void OnActivate(IObjAiBase owner, ISpell spell = null)
        {
            SetStatus(owner, StatusFlags.CanMove, false);
            SetStatus(owner, StatusFlags.Ghosted, true);
            SetStatus(owner, StatusFlags.Targetable, false);
            SetStatus(owner, StatusFlags.SuppressCallForHelp, true);
            SetStatus(owner, StatusFlags.IgnoreCallForHelp, true);
            SetStatus(owner, StatusFlags.CanAttack, false);
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell = null)
        {
        }

        public void OnUpdate(float diff)
        {
        }
    }
}
