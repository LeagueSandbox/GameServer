using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Scripting.CSharp
{
    public interface IAIScript
    {
        IAIScriptMetaData AIScriptMetaData { get; set; }
        public void OnActivate(IObjAiBase owner)
        {
        }
        public void OnUpdate(float diff)
        {
        }
    }
}