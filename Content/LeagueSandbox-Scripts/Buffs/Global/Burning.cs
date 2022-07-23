using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
    internal class Burning : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.DAMAGE,
        };

        public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

        Buff thisBuff;
        AttackableUnit Unit;
        float damageTimer = 0;
        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
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

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
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
