using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.Stats;

namespace ItemPassives
{
    public class ItemID_3178 : IItemScript
    {
        IObjAiBase Owner;
        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        public void OnActivate(IObjAiBase owner)
        {
            Owner = owner;
            //this results in a 4.08333 attack speed
            StatsModifier.AttackSpeed.FlatBonus = 3.5f;
            StatsModifier.ArmorPenetration.FlatBonus = 0.75f;
            //League spams this as if it was in every tick. Doesn't seem necessary to spam it though
            OverrideUnitAttackSpeedCap(Owner, true, 4.0f, false, 1.0f);

            owner.AddStatModifier(StatsModifier);
        }

        public void OnDeactivate(IObjAiBase owner)
        {
        }

        public void OnUpdate(float diff)
        {
        }
    }
}
