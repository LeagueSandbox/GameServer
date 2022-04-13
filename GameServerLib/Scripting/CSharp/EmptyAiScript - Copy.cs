using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace GameServerCore.Scripting.CSharp
{
    public class EmptyMasteryScript : IMasteryScript
    {
        public void OnActivate(IObjAiBase owner, byte level)
        {
        }
        public void OnUpdate(float diff)
        {
        }
    }
}