using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
    internal class Chilled : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.SLOW,
            BuffAddType = BuffAddType.STACKS_AND_OVERLAPS,
            MaxStacks = 100
        };

        public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            // TODO: Implement global variables which can be assigned values outside of the buff script.
            StatsModifier.AttackSpeed.PercentBonus = StatsModifier.AttackSpeed.PercentBonus - 0.2f;
            StatsModifier.MoveSpeed.PercentBonus = StatsModifier.MoveSpeed.PercentBonus - 0.2f;
            unit.AddStatModifier(StatsModifier);

            // ApplyAssistMarker
        }

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
        }

        public void OnUpdate(float diff)
        {

        }
    }
}

