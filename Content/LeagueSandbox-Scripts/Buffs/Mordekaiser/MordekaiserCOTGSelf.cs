using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
    internal class MordekaiserCOTGSelf : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER,
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };

        public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            Pet ghost = buff.SourceUnit.GetPet();

            float ADBonus = ghost.ClonedUnit.Stats.AttackDamage.Total * 0.2f;
            float APBonus = ghost.ClonedUnit.Stats.AbilityPower.Total * 0.2f;

            StatsModifier.AttackDamage.FlatBonus = ADBonus;
            StatsModifier.AbilityPower.FlatBonus = APBonus;

            unit.AddStatModifier(StatsModifier);

            buff.SetToolTipVar(0, ADBonus);
            buff.SetToolTipVar(1, APBonus);
        }
    }
}
