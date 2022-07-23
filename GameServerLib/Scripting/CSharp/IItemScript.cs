using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.StatsNS;

namespace GameServerCore.Scripting.CSharp
{
    public interface IItemScript
    {
        StatsModifier StatsModifier { get; }

        void OnActivate(ObjAIBase owner)
        {
        }

        void OnDeactivate(ObjAIBase owner)
        {
        }

        void OnUpdate(float diff)
        {
        }
    }
}
