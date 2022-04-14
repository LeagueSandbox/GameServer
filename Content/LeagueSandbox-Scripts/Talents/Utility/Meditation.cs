using GameServerCore.Domain.GameObjects;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.Stats;


namespace Talents
{
    internal class Talent_4313 : ITalentScript
    {
        public void OnActivate(IObjAiBase owner, byte rank)
        {
            var manaRegen = new StatsModifier();
            manaRegen.ManaRegeneration.FlatBonus = 0.2f * rank;
            owner.AddStatModifier(manaRegen);
        }
    }
}
