using GameServerCore.Enums;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Overdrive
{
    internal class Overdrive : IBuffGameScript
    {
        public BuffType BuffType => BuffType.HASTE;
        public BuffAddType BuffAddType => BuffAddType.REPLACE_EXISTING;
        public int MaxStacks => 5;
        public bool IsHidden => false;

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        //IParticle p;

        public void OnActivate(IObjAiBase unit, IBuff buff, ISpell ownerSpell)
        {
            //p = AddParticleTarget(unit, "Overdrive_buf.troy", unit, 1);
            StatsModifier.MoveSpeed.PercentBonus = StatsModifier.MoveSpeed.PercentBonus + (12f + ownerSpell.Level * 4) / 100f;
            StatsModifier.AttackSpeed.PercentBonus = StatsModifier.AttackSpeed.PercentBonus + (22f + 8f * ownerSpell.Level) / 100f;
            unit.AddStatModifier(StatsModifier);

        }

        public void OnDeactivate(IObjAiBase unit)
        {
            //RemoveParticle(p);
            unit.RemoveStatModifier(StatsModifier);
        }

        public void OnUpdate(double diff)
        {

        }
    }
}

