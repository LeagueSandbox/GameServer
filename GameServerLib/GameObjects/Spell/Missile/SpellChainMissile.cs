﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using GameServerCore.Content;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.Content;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace LeagueSandbox.GameServer.GameObjects.Spell.Missile
{
    public class SpellChainMissile : SpellMissile, ISpellChainMissile
    {
        public override MissileType Type { get; protected set; } = MissileType.Chained;

        /// <summary>
        /// Number of objects this projectile has hit since it was created.
        /// </summary>
        public List<IGameObject> ObjectsHit { get; }
        /// <summary>
        /// Total number of times this missile has hit any units.
        /// </summary>
        /// TODO: Verify if we want this to be an array for different MaximumHit counts for: CanHitCaster, CanHitEnemies, CanHitFriends, CanHitSameTarget, and CanHitSameTargetConsecutively.
        public int HitCount { get; protected set; }
        /// <summary>
        /// Parameters for this chain missile, refer to IMissileParameters.
        /// </summary>
        public IMissileParameters Parameters { get; protected set; }

        public SpellChainMissile(
            Game game,
            int collisionRadius,
            ISpell originSpell,
            ICastInfo castInfo,
            IMissileParameters parameters,
            float moveSpeed,
            SpellDataFlags overrideFlags = 0, // TODO: Find a use for these
            uint netId = 0,
            bool serverOnly = false
        ) : base(game, collisionRadius, originSpell, castInfo, moveSpeed, overrideFlags, netId, serverOnly)
        {
            ObjectsHit = new List<IGameObject>();
            HitCount = 0;
            Parameters = parameters;
        }

        public override void Update(float diff)
        {
            base.Update(diff);

            // TODO: Verify if we can move this into CheckFlagsForUnit instead of checking every Update.
            if (HitCount >= Parameters.MaximumHits)
            {
                SetToRemove();
            }

            if (!IsToRemove() && IsValidTarget(TargetUnit, true))
            {
                _game.PacketNotifier.NotifyS2C_UpdateBounceMissile(this);
                _game.PacketNotifier.NotifyMissileReplication(this);
            }
        }

        public override void CheckFlagsForUnit(IAttackableUnit unit)
        {
            if (!IsValidTarget(unit) && !GetNextTarget())
            {
                SetToRemove();

                return;
            }

            ObjectsHit.Add(unit);
            HitCount++;

            // Targeted Spell (including auto attack spells)
            if (SpellOrigin != null)
            {
                SpellOrigin.ApplyEffects(TargetUnit, this);
            }

            if (CastInfo.Owner is IObjAiBase ai && SpellOrigin.CastInfo.IsAutoAttack)
            {
                ai.AutoAttackHit(TargetUnit);
            }

            if (!GetNextTarget())
            {
                SetToRemove();
            }
        }

        public bool GetNextTarget()
        {
            var units = _game.ObjectManager.GetUnitsInRange(Position, SpellOrigin.SpellData.BounceRadius, true);

            foreach (IAttackableUnit closestUnit in units.OrderBy(unit => Vector2.DistanceSquared(Position, unit.Position)))
            {
                if (IsValidTarget(closestUnit))
                {
                    TargetUnit = closestUnit;
                    var castTarget = new CastTarget(TargetUnit, HitResult.HIT_Normal);
                    CastInfo.Targets[0] = castTarget;
                    CastInfo.TargetPosition = TargetUnit.GetPosition3D();
                    CastInfo.TargetPositionEnd = CastInfo.TargetPosition;
                    CastInfo.SpellCastLaunchPosition = GetPosition3D();

                    return true;
                }
            }

            return false;
        }

        protected bool IsValidTarget(IAttackableUnit unit, bool checkOnly = false)
        {
            bool valid = SpellOrigin.SpellData.IsValidTarget(CastInfo.Owner, unit);
            bool hit = ObjectsHit.Contains(unit);

            if (hit)
            {
                // We can't hit this unit because we've hit it already.
                valid = false;

                // We can consecutively hit this same unit until we run out of bounces.
                if (Parameters.CanHitSameTarget && Parameters.CanHitSameTargetConsecutively)
                {
                    valid = true;
                }
                // We can hit it again after we bounce once.
                else if (Parameters.CanHitSameTarget && !checkOnly)
                {
                    ObjectsHit.Remove(unit);
                }
            }
            // Otherwise, we can hit this unit because we haven't hit it yet.

            return valid;
        }
    }
}
