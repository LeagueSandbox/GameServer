using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;

namespace ItemPassives
{
    public class ItemID_3115 : IItemScript
    {
        public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

        public void OnActivate(ObjAIBase owner)
        {
            StatsModifier.CooldownReduction.FlatBonus = 0.2f;
            owner.AddStatModifier(StatsModifier);
        }
    }
}
