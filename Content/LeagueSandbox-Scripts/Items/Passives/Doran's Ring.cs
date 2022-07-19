using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.API;
using            GameServerLib.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;

namespace ItemPassives
{
    public class ItemID_1056 : IItemScript
    {
        public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

        public void OnActivate(ObjAIBase owner)
        {
            ApiEventManager.OnKillUnit.AddListener(this, owner, TargetExecute, false);
            StatsModifier.ManaRegeneration.BaseBonus += 0.6f;
            owner.AddStatModifier(StatsModifier);
        }
        public void TargetExecute (DeathData deathData)
        {
            deathData.Killer.Stats.CurrentMana += 4;
        }
        public void OnDeactivate(ObjAIBase owner)
        {
            ApiEventManager.OnKillUnit.RemoveListener(this);
        }
    }
}
