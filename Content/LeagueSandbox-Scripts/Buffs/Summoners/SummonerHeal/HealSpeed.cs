using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using GameServerCore.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
    internal class HealSpeed : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.HEAL,
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };

        public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            AddParticleTarget(ownerSpell.CastInfo.Owner, unit, "global_ss_heal_02", unit, buff.Duration);
            AddParticleTarget(ownerSpell.CastInfo.Owner, unit, "global_ss_heal_speedboost", unit, buff.Duration);

            StatsModifier.MoveSpeed.PercentBonus = 0.3f;
            unit.AddStatModifier(StatsModifier);
        }
    }
}
