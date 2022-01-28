using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Collections.Generic;

namespace GameServerCore.Scripting.CSharp
{
    public class EmptyAIScript : IAIScript
    {
        public IAIScriptMetaData AIScriptMetaData { get; set; } = new AIScriptMetaData();
        public Dictionary<IAttackableUnit, int> unitsAttackingAllies { get; }
        public void OnActivate(IObjAiBase owner)
        {
        }
        public void OnUpdate(float diff)
        {
        }
    }
}