using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Scripting.CSharp
{
    public interface IAIScript
    {
        IAIScriptMetaData AIScriptMetaData { get; set; }
        void OnActivate(IObjAiBase owner)
        {
        }
        void OnUpdate(float diff)
        {
        }
    }
}