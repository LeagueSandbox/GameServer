using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using System;
using System.Numerics;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;

namespace Buffs
{
    internal class BlindMonkQTwoDash : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };

        public StatsModifier StatsModifier { get; private set; }

        ObjAIBase owner;
        Spell originSpell;
        Buff thisBuff;
        AttackableUnit target;
        bool toRemove;
        Particle selfParticle;

        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
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

        public void OnMoveEnd(AttackableUnit unit)
        {
            toRemove = true;
        }

        public void OnMoveSuccess(AttackableUnit unit)
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

            if (owner.Team != target.Team && target is Champion)
            {
                owner.SetTargetUnit(target, true);
                owner.UpdateMoveOrder(OrderType.AttackTo, true);
            }
        }

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
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