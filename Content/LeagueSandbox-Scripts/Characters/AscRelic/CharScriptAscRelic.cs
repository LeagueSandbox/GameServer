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
            AddParticleTarget(owner, owner, "Asc_RelicPrism_Sand", owner, 25000.0f, direction: new Vector3(0.0f, 0.0f, -1.0f), flags: (FXFlags)304);
            AddParticleTarget(owner, owner, "Asc_relic_Sand_buf", owner, 25000.0f, flags: (FXFlags)32);
        }
        public void OnDeactivate(IObjAiBase owner, ISpell spell = null)
        {
        }
        public void OnUpdate(float diff)
        {
        }
    }
}
