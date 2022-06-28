using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Scripting.CSharp
{
    public interface IItemScript
    {
        IStatsModifier StatsModifier { get; }

        void OnActivate(IObjAiBase owner)
        {
        }

        void OnDeactivate(IObjAiBase owner)
        {
        }

        void OnUpdate(float diff)
        {
        }
    }
}
