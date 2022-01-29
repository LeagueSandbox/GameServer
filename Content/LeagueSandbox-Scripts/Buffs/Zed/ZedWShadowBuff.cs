using System.Numerics;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.Stats;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
    class ZedWShadowBuff : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER,
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        IBuff ThisBuff;
        IMinion Shadow;
        IParticle currentIndicator;
        int previousIndicatorState;

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            ThisBuff = buff;
            Shadow = unit as IMinion;

            buff.SetStatusEffect(StatusFlags.Targetable, false);
            buff.SetStatusEffect(StatusFlags.Ghosted, true);

            AddParticleTarget(Shadow.Owner, Shadow, "zed_base_w_tar", Shadow);

            currentIndicator = AddParticleTarget(Shadow.Owner, Shadow.Owner, "zed_shadowindicatorfar", Shadow, buff.Duration, flags: FXFlags.TargetDirection);
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            if (Shadow != null && !Shadow.IsDead)
            {
                if (currentIndicator != null)
                {
                    currentIndicator.SetToRemove();
                }

                SetStatus(Shadow, StatusFlags.NoRender, true);
                AddParticle(Shadow.Owner, null, "zed_base_clonedeath", Shadow.Position);
                Shadow.TakeDamage(Shadow.Owner, 10000f, DamageType.DAMAGE_TYPE_TRUE, DamageSource.DAMAGE_SOURCE_INTERNALRAW, DamageResultType.RESULT_NORMAL);
            }
        }

        public int GetIndicatorState()
        {
            var dist = Vector2.Distance(Shadow.Owner.Position, Shadow.Position);
            var state = 0;

            if (!Shadow.Owner.HasBuff("ZedW2"))
            {
                return state;
            }

            if (dist >= 1000.0f)
            {
                state = 0;
            }
            else if (dist >= 800.0f)
            {
                state = 1;
            }
            else if (dist >= 0f)
            {
                state = 2;
            }

            return state;
        }

        public string GetIndicatorName(int state)
        {
            switch (state)
            {
                case 0:
                {
                    return "zed_shadowindicatorfar";
                }
                case 1:
                {
                    return "zed_shadowindicatormed";
                }
                case 2:
                {
                    return "zed_shadowindicatornearbloop";
                }
                default:
                {
                    return "zed_shadowindicatorfar";
                }
            }
        }

        public void OnUpdate(float diff)
        {
            if (Shadow != null && !Shadow.IsDead)
            {
                int state = GetIndicatorState();
                if (state != previousIndicatorState)
                {
                    previousIndicatorState = state;
                    if (currentIndicator != null)
                    {
                        currentIndicator.SetToRemove();
                    }

                    currentIndicator = AddParticleTarget(Shadow.Owner, Shadow.Owner, GetIndicatorName(state), Shadow, ThisBuff.Duration - ThisBuff.TimeElapsed, flags: FXFlags.TargetDirection);
                }
            }
        }
    }
}
