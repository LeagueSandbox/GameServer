using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Scripting.CSharp;
using System.Linq;
using System.Numerics;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;

namespace Buffs
{
    internal class OdinShieldRelicAura : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
        };

        public StatsModifier StatsModifier { get; private set; }

        bool setToKill;
        Buff thisBuff;
        Particle buffParticle;
        AttackableUnit Unit;
        float timer = 250f;
        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            thisBuff = buff;
            Unit = unit;

            buff.SetStatusEffect(StatusFlags.Targetable, false);
            buff.SetStatusEffect(StatusFlags.Invulnerable, true);
            buff.SetStatusEffect(StatusFlags.ForceRenderParticles, true);
            buff.SetStatusEffect(StatusFlags.NoRender, true);

            buffParticle = AddParticleTarget(unit, unit, "odin_heal_rune", unit, -1f);
            setToKill = false;
        }

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            buffParticle.SetToRemove();

            SetStatus(unit, StatusFlags.Targetable, true);
            SetStatus(unit, StatusFlags.Invulnerable, false);

            unit.TakeDamage(unit, 250000.0f, DamageType.DAMAGE_TYPE_TRUE, DamageSource.DAMAGE_SOURCE_INTERNALRAW, false);

            SetStatus(unit, StatusFlags.NoRender, true);
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
                        AddBuff("OdinShieldRelicBuffHeal", 0.1f, 1, null, units[0], null);

                        setToKill = true;
                    }
                }
                timer = 0;
            }
        }
    }
}