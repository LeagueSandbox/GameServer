using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;

namespace Buffs
{
    internal class Slow : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.SLOW,
            BuffAddType = BuffAddType.STACKS_AND_OVERLAPS,
            MaxStacks = 100
        };

        public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

        Particle slow;
        AttackableUnit owner;

        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
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

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            RemoveParticle(slow);
        }

        // TODO: Find a better way to transfer data between scripts.
        public void SetSlowMod(float slowAmount)
        {
            StatsModifier.MoveSpeed.PercentBonus -= slowAmount;
            owner.AddStatModifier(StatsModifier);
        }
    }
}