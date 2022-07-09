using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace Buffs
{
    internal class MordekaiserCOTGSelf : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER,
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            IPet ghost = buff.SourceUnit.GetPet();

            float ADBonus = ghost.ClonedUnit.Stats.AttackDamage.Total * 0.2f;
            float APBonus = ghost.ClonedUnit.Stats.AbilityPower.Total * 0.2f;

            StatsModifier.AttackDamage.FlatBonus = ADBonus;
            StatsModifier.AbilityPower.FlatBonus = APBonus;

            unit.AddStatModifier(StatsModifier);

            buff.SetToolTipVar(0, ADBonus);
            buff.SetToolTipVar(1, APBonus);
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
        }

        public void OnUpdate(float diff)
        {
        }
    }
}
