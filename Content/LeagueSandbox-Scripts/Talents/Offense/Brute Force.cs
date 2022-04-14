using GameServerCore.Domain.GameObjects;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.Stats;


namespace Talents
{
    internal class Talent_4122 : ITalentScript
    {
        public void OnActivate(IObjAiBase owner, byte rank)
        {
            var attackPerLevel = new StatsModifier();
            attackPerLevel.AttackDamagePerLevel.FlatBonus = 0.22f * rank;
            owner.AddStatModifier(attackPerLevel);
        }
    }
}
