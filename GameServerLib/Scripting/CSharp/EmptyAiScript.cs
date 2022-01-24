using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace GameServerCore.Scripting.CSharp
{
    public class EmptyAiScript : IAiScript
    {
        public IAiScriptMetaData AiScriptMetaData { get; set; } = new AiScriptMetaData();
        public void OnActivate(IObjAiBase owner)
        {
        }
        public void OnUpdate(float diff)
        {
        }
    }
}