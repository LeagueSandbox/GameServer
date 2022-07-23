using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Scripting.CSharp;
using System.Linq;
using System.Numerics;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;

namespace Buffs
{
    internal class HowlingAbyssRelicAura : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
        };

        public StatsModifier StatsModifier { get; private set; }

        Buff thisBuff;
        Particle buffParticle;
        bool setToKill;

        AttackableUnit Unit;
        float timer = 250f;
        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            Unit = unit;
            thisBuff = buff;

            buff.SetStatusEffect(StatusFlags.NoRender, true);
            buff.SetStatusEffect(StatusFlags.Invulnerable, true);
            buff.SetStatusEffect(StatusFlags.ForceRenderParticles, true);

            buffParticle = AddParticleTarget(Unit, null, "ha_ap_healingbuff", Unit, buff.Duration);
            setToKill = false;
        }

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            buffParticle.SetToRemove();

            SetStatus(unit, StatusFlags.Targetable, true);
            SetStatus(unit, StatusFlags.Invulnerable, false);
            SetStatus(unit, StatusFlags.NoRender, true);

            unit.TakeDamage(unit, 250000.0f, DamageType.DAMAGE_TYPE_TRUE, DamageSource.DAMAGE_SOURCE_INTERNALRAW, false);
        }

        public void OnUpdate(float diff)
        {
            if (setToKill)
            {
                thisBuff.DeactivateBuff();
            }
            timer += diff;
            if (Unit != null && timer >= 250)
            {
                var units = GetUnitsInRange(Unit.Position, 175f, true).OrderBy(unit => Vector2.DistanceSquared(unit.Position, Unit.Position)).ToList();
                units.RemoveAll(x => !(x is Champion));
                if (units.Count >= 1)
                {
                    if (!setToKill)
                    {
                        AddBuff("HowlingAbyssFBHeal", 0.1f, 1, null, units[0], null);

                        setToKill = true;
                    }
                }
                timer = 0;
            }
        }
    }
}