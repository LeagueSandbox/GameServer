using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using System.Numerics;

namespace GameServerCore.Scripting.CSharp
{
    public interface IItemScript
    {
        IStatsModifier StatsModifier { get; }

        void OnActivate(IObjAiBase owner);

        void OnDeactivate(IObjAiBase owner);

        void OnUpdate(float diff);
    }
}
