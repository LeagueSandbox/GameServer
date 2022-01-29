using System.Numerics;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace Buffs
{
    internal class Burning : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.DAMAGE,
        };

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        IBuff thisBuff;
        IAttackableUnit Unit;
        float damageTimer = 0;
        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            thisBuff  = buff;
            Unit = unit;
            float slowAmount = 0;
            if (buff.SourceUnit != null)
            {
                if (buff.SourceUnit.Stats.Level < 6)
                {
                    slowAmount = 0.05f;
                }
                else if (buff.SourceUnit.Stats.Level < 11)
                {
                    slowAmount = 0.075f;
                }
                else
                {
                    slowAmount = 0.1f;
                }

                if (!buff.SourceUnit.IsMelee)
                {
                    slowAmount /= 2;
                }
  
                StatsModifier.MoveSpeed.PercentBonus -= slowAmount;
                unit.AddStatModifier(StatsModifier);
            }
        }

        public void OnDeath(IDeathData deathData)
        {
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            ApiEventManager.OnDeath.RemoveListener(this);
        }

        public void OnUpdate(float diff)
        {
            damageTimer -= diff;
            if(thisBuff != null && thisBuff.SourceUnit != null && Unit != null && damageTimer <= 0)
            {
                Unit.TakeDamage(thisBuff.SourceUnit, 0.67f + thisBuff.SourceUnit.Stats.Level, DamageType.DAMAGE_TYPE_TRUE, DamageSource.DAMAGE_SOURCE_PERIODIC, false);
                damageTimer = 1000.0f;
            }
        }
    }
}
