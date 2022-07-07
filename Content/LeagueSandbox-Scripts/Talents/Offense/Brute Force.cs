using GameServerCore.Domain.GameObjects;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.Stats;


namespace Talents
{
    internal class Talent_4122 : ITalentScript
    {
        public void OnActivate(IObjAIBase owner, byte rank)
        {
            var attackPerLevel = new StatsModifier();
            float[] statPerRank = new[] { 0.22f, 0.39f, 0.55f };
            attackPerLevel.AttackDamagePerLevel.FlatBonus = statPerRank[rank - 1];

            owner.AddStatModifier(attackPerLevel);
        }
    }
}
