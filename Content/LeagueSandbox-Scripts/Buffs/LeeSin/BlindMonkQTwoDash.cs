using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using System;
using System.Numerics;

namespace Buffs
{
    internal class BlindMonkQTwoDash : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };

        public IStatsModifier StatsModifier { get; private set; }

        IObjAIBase owner;
        ISpell originSpell;
        IBuff thisBuff;
        IAttackableUnit target;
        bool toRemove;
        IParticle selfParticle;

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            owner = ownerSpell.CastInfo.Owner;
            thisBuff = buff;
            originSpell = ownerSpell;
            target = buff.SourceUnit;
            ApiEventManager.OnMoveEnd.AddListener(this, unit, OnMoveEnd, true);
            ApiEventManager.OnMoveSuccess.AddListener(this, unit, OnMoveSuccess, true);

            var dashSpeed = 1350f + owner.Stats.GetTrueMoveSpeed();

            ForceMovement(owner, target, "", dashSpeed, 0f, Vector2.Distance(target.Position, owner.Position), 0f, -1f, movementOrdersType: ForceMovementOrdersType.CANCEL_ORDER);

            selfParticle = AddParticleTarget(owner, owner, "blindMonk_Q_resonatingStrike_mis", owner, flags: 0);
            // Flags: Blend false, Lock true, Loop true
            PlayAnimation(owner, "Spell1b", 0f, flags: AnimationFlags.Lock);
            toRemove = false;

            buff.SetStatusEffect(StatusFlags.Ghosted, true);
        }

        public void OnMoveEnd(IAttackableUnit unit)
        {
            toRemove = true;
        }

        public void OnMoveSuccess(IAttackableUnit unit)
        {
            var missingHealth = target.Stats.HealthPoints.Total - target.Stats.CurrentHealth;
            var bonusDamage = missingHealth * 0.08f;

            if (target.Team == TeamId.TEAM_NEUTRAL)
            {
                bonusDamage = MathF.Min(bonusDamage, 400.0f);
            }

            // BreakSpellShields(target);

            var ad = owner.Stats.AttackDamage.Total * 1.0f;
            var damage = 50 + (originSpell.CastInfo.SpellLevel * 30) + ad + bonusDamage;

            AddBuff("BlindMonkQTwoDashParticle", 0.1f, 1, originSpell, target, owner);

            target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);

            RemoveBuff(thisBuff);

            if (owner.Team != target.Team && target is IChampion)
            {
                owner.SetTargetUnit(target, true);
                owner.UpdateMoveOrder(OrderType.AttackTo, true);
            }
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            RemoveParticle(selfParticle);
            // Flags: Blend true
            // UnlockAnimation(true, unit);
        }

        public void OnUpdate(float diff)
        {
            if (thisBuff != null && toRemove)
            {
                RemoveBuff(thisBuff);
            }
        }
    }
}