using GameServerCore.Domain.GameObjects;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.Stats;


namespace Talents
{
    internal class Talent_4132 : ITalentScript
    {
        public void OnActivate(IObjAIBase owner, byte rank)
        {
            var attackDamage = new StatsModifier();
            attackDamage.AttackDamage.FlatBonus = 5.0f;
            owner.AddStatModifier(attackDamage);
        }
    }
}