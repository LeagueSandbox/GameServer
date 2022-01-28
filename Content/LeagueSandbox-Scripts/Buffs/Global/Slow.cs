using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects.Spell;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Scripting.CSharp;

namespace Buffs
{
    internal class Slow : IBuffGameScript
    {
        public BuffType BuffType => BuffType.SLOW;
        public BuffAddType BuffAddType => BuffAddType.STACKS_AND_OVERLAPS;
        public int MaxStacks => 100;
        public bool IsHidden => false;

        public IStatsModifier StatsModifier { get; private set; }

        IParticle slow;
        IAttackableUnit owner;

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            owner = unit;
            slow = AddParticleTarget(ownerSpell.CastInfo.Owner, unit, "LOC_Slow", unit, buff.Duration);

            // Normally this would be here and would use data given by another script.
            //StatsModifier.MoveSpeed.PercentBonus -= slowAmount;
            //owner.AddStatModifier(StatsModifier);

            // ApplyAssistMarker(buff.SourceUnit, unit, 10.0f);

            // For attack speed and move speed mod changes:
            // ApiEventManager.OnUpdateBuffs.AddListener(this, buff, OnUpdateBuffs, false);
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            RemoveParticle(slow);
        }

        // TODO: Find a better way to transfer data between scripts.
        public void SetSlowMod(float slowAmount)
        {
            StatsModifier.MoveSpeed.PercentBonus -= slowAmount;
            owner.AddStatModifier(StatsModifier);
        }

        public void OnUpdate(float diff)
        {
        }
    }
}