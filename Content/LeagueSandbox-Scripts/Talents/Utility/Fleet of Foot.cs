using GameServerCore.Domain.GameObjects;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.Stats;


namespace Talents
{
    internal class Talent_4312 : ITalentScript
    {
        public void OnActivate(IObjAiBase owner, byte rank)
        {
            var movSpeed = new StatsModifier();
            movSpeed.MoveSpeed.PercentBonus = 0.05f * rank;
            owner.AddStatModifier(movSpeed);
        }
    }
}
