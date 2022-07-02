using GameServerCore.Domain.GameObjects;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.Stats;

namespace LeagueSandbox.GameServer.Scripting.CSharp
{
    public class ItemScriptEmpty : IItemScript
    {
        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        public void OnActivate(IObjAIBase owner)
        {
        }

        public void OnDeactivate(IObjAIBase owner)
        {
        }

        public void OnUpdate(float diff)
        {
        }
    }
}
