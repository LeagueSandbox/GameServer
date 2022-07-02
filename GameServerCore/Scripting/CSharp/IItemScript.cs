using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Scripting.CSharp
{
    public interface IItemScript
    {
        IStatsModifier StatsModifier { get; }

        void OnActivate(IObjAIBase owner)
        {
        }

        void OnDeactivate(IObjAIBase owner)
        {
        }

        void OnUpdate(float diff)
        {
        }
    }
}
