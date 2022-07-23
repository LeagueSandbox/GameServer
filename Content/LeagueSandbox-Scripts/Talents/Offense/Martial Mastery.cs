using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.StatsNS;


namespace Talents
{
    internal class Talent_4132 : ITalentScript
    {
        public void OnActivate(ObjAIBase owner, byte rank)
        {
            var attackDamage = new StatsModifier();
            attackDamage.AttackDamage.FlatBonus = 5.0f;
            owner.AddStatModifier(attackDamage);
        }
    }
}