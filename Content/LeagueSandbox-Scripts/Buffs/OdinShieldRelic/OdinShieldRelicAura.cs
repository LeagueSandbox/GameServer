using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects.Spell;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using GameServerCore.Domain.GameObjects.Spell.Sector;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using System.Linq;
using System.Numerics;

namespace Buffs
{
    internal class OdinShieldRelicAura : IBuffGameScript
    {
        public BuffType BuffType => BuffType.INTERNAL;
        public BuffAddType BuffAddType => BuffAddType.RENEW_EXISTING;
        public int MaxStacks => 1;
        public bool IsHidden => false;

        public IStatsModifier StatsModifier { get; private set; }

        bool setToKill;
        IBuff thisBuff;
        IParticle buffParticle;
        IAttackableUnit Unit;
        float timer = 250f;
        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            thisBuff = buff;
            Unit = unit;

            buff.SetStatusEffect(StatusFlags.Targetable, false);
            buff.SetStatusEffect(StatusFlags.Invulnerable, true);
            buff.SetStatusEffect(StatusFlags.ForceRenderParticles, true);
            buff.SetStatusEffect(StatusFlags.NoRender, true);

            buffParticle = AddParticleTarget(unit, unit, "odin_heal_rune", unit, -1f, reqVision: false);
            setToKill = false;
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
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
                units.RemoveAll(x => !(x is IChampion));
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