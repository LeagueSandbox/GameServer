using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Scripting.CSharp
{
    public class EmptyTalentScript : ITalentScript
    {
        public void OnActivate(IObjAiBase owner, byte level)
        {
        }
    }
}