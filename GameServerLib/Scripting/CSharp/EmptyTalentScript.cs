using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace GameServerCore.Scripting.CSharp
{
    public class EmptyTalentScript : ITalentScript
    {
        public void OnActivate(IObjAiBase owner, byte level)
        {
        }
    }
}