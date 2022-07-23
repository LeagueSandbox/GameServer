using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;

namespace ItemPassives
{
    public class ItemID_3178 : IItemScript
    {
        ObjAIBase Owner;
        public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

        public void OnActivate(ObjAIBase owner)
        {
            Owner = owner;

            //League spams this constantly. Doesn't seem necessary to spam it though
            OverrideUnitAttackSpeedCap(Owner, true, 4.0f, false, 1.0f);

            StatsModifier.AttackSpeed.FlatBonus = 3.5f;
            StatsModifier.ArmorPenetration.FlatBonus = 0.75f;
            owner.AddStatModifier(StatsModifier);

            //This was the best way i could replicate the tower's attack speed shown in the tool tip vs it's actual attackspeed
            owner.Stats.AttackSpeedFlat = 0.45f;
        }
    }
}
