using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace GameServerCore.Scripting.CSharp
{
    public class EmptyAIScript : IAIScript
    {

        public IAIScriptMetaData AIScriptMetaData { get; set; } = new AIScriptMetaData();

        public void OnActivate(IObjAIBase owner)
        {
        }

        public void OnUpdate(float diff)
        {
        }
    }
}