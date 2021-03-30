﻿using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Domain.GameObjects.Spell;

namespace BlindMonkSonicWave
{
    internal class BlindMonkSonicWave : IBuffGameScript
    {
        public BuffType BuffType => BuffType.INTERNAL;
        public BuffAddType BuffAddType => BuffAddType.REPLACE_EXISTING;
        public bool IsHidden => false;
        public int MaxStacks => 1;

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        ISpell originSpell;
        IBuff thisBuff;

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            originSpell = ownerSpell;
            thisBuff = buff;
        }

        public void OnDeactivate(IAttackableUnit unit)
        {
        }

        public void OnUpdate(double diff)
        {
            if (thisBuff == null || originSpell == null || thisBuff.Elapsed())
            {
                return;
            }

            var owner = originSpell.Owner;
            var target = thisBuff.TargetUnit;
            if (owner.IsCollidingWith(target))
            {
                owner.SetDashingState(false);
                var ad = owner.Stats.AttackDamage.Total * 1.0f;
                var damage = 50 + (originSpell.Level * 30) + ad + (0.08f * (target.Stats.HealthPoints.Total - target.Stats.CurrentHealth));
                target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_REACTIVE, false);
                AddParticleTarget(owner, "GlobalHit_Yellow_tar.troy", target, 1);

                thisBuff.DeactivateBuff();
            }
        }
    }
}

