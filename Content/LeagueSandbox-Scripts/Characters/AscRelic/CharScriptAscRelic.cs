using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace CharScripts
{
    internal class CharScriptAscRelic : ICharScript
    {
        public void OnActivate(IObjAIBase owner, ISpell spell = null)
        {
            AddBuff("AscRelicBombBuff", 25000.0f, 1, null, owner, owner);
        }
        public void OnDeactivate(IObjAIBase owner, ISpell spell = null)
        {
        }
        public void OnUpdate(float diff)
        {
        }
    }
}
