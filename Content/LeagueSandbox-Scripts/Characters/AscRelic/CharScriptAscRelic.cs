using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using System.Numerics;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using static LeagueSandbox.GameServer.API.ApiMapFunctionManager;


namespace CharScripts
{
    internal class CharScriptAscRelic : ICharScript
    {
        public void OnActivate(IObjAiBase owner, ISpell spell = null)
        {
            AddBuff("AscRelicPointBuff", 25000.0f, 1, null, owner, owner);
        }
        public void OnDeactivate(IObjAiBase owner, ISpell spell = null)
        {
        }
        public void OnUpdate(float diff)
        {
        }
    }
}
