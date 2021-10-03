using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.API;
using GameServerCore.Domain;

namespace ItemPassives
{
    public class ItemID_1056 : IItemScript
    {
        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        public void OnActivate(IObjAiBase owner)
        {
            ApiEventManager.OnKillUnit.AddListener(this, owner, TargetExecute, false);
            StatsModifier.ManaRegeneration.BaseBonus += 0.6f;
            owner.AddStatModifier(StatsModifier);
        }
        public void TargetExecute (IDeathData deathData)
        {
            deathData.Killer.Stats.CurrentMana += 4;
        }
        public void OnDeactivate(IObjAiBase owner)
        {
            ApiEventManager.OnKillUnit.RemoveListener(this);
        }
        public void OnUpdate(float diff)
        {
        }
    }
}
