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
using GameServerCore.Domain;

namespace Buffs
{
    internal class HowlingAbyssRelicAura : IBuffGameScript
    {
        public BuffType BuffType => BuffType.INTERNAL;
        public BuffAddType BuffAddType => BuffAddType.RENEW_EXISTING;
        public int MaxStacks => 1;
        public bool IsHidden => false;

        public IStatsModifier StatsModifier { get; private set; }

        IBuff thisBuff;
        IParticle buffParticle;
        bool setToKill;

        IAttackableUnit Unit;
        float timer = 250f;
        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            Unit = unit;
            thisBuff = buff;

            buff.SetStatusEffect(StatusFlags.NoRender, true);
            buff.SetStatusEffect(StatusFlags.Invulnerable, true);
            buff.SetStatusEffect(StatusFlags.ForceRenderParticles, true);

            buffParticle = AddParticleTarget(Unit, null, "ha_ap_healingbuff", Unit, buff.Duration, reqVision: false);
            setToKill = false;
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
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
                units.RemoveAll(x => !(x is IChampion));
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