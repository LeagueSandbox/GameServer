using GameServerCore.Domain.GameObjects;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.Stats;

namespace Talents
{
    internal class Talent_4222 : ITalentScript
    {
        public void OnActivate(IObjAiBase owner, byte rank)
        {
            var healthModifier = new StatsModifier();
            healthModifier.HealthPoints.FlatBonus = 12.0f * rank;
            owner.AddStatModifier(healthModifier);
            owner.Stats.CurrentHealth = owner.Stats.HealthPoints.Total;
        }
    }
}
