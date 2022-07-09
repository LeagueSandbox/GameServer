using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
    internal class MordekaiserCOTGDot : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.INTERNAL,
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            float basePercentDamage = 0.012f + (0.0025f * (buff.OriginSpell.CastInfo.SpellLevel - 1));
            float AP = buff.SourceUnit.Stats.AbilityPower.Total * 0.00002f;
            float damage = buff.TargetUnit.Stats.HealthPoints.Total * (basePercentDamage + AP);

            var data = buff.TargetUnit.TakeDamage(buff.SourceUnit, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_PROC, false);
            buff.SourceUnit.Stats.CurrentHealth += data.PostMitigationDamage;
        }
    }
}
