using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;

namespace GameServerCore.Scripting.CSharp
{
    public interface ITalentScript
    {
        void OnActivate(ObjAIBase owner, byte rank)
        {
        }
    }
}