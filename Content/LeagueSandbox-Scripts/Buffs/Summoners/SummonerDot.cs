using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.Stats;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
    internal class SummonerDot : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.DAMAGE,
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        IParticle ignite;
        IObjAiBase Owner;
        IAttackableUnit Target;

        float timeSinceLastTick = 1000.0f;
        float damage;

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            Owner = ownerSpell.CastInfo.Owner;
            Target = unit;
            damage = 10 + Owner.Stats.Level * 4;
            ignite = AddParticleTarget(Owner, unit, "Global_SS_Ignite", unit, buff.Duration, bone: "C_BUFFBONE_GLB_CHEST_LOC");
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            Owner = null;
            Target = null;
            ignite.SetToRemove();
        }

        public void OnUpdate(float diff)
        {
            if (Target == null || Owner == null)
            {
                return;
            }

            timeSinceLastTick += diff;

            if (timeSinceLastTick >= 1000.0f)
            {
                Target.TakeDamage(Owner, damage, DamageType.DAMAGE_TYPE_TRUE, DamageSource.DAMAGE_SOURCE_SPELL, false);
                timeSinceLastTick = 0;
            }
        }
    }
}
