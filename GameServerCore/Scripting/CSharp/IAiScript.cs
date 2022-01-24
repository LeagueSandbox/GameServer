using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Scripting.CSharp
{
    public interface IAiScript
    {
        IAiScriptMetaData AiScriptMetaData { get; set; }
        public void OnActivate(IObjAiBase owner)
        {
        }
        public void OnUpdate(float diff)
        {
        }
    }
}