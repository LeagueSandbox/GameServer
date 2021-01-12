using System;
using System.Collections.Generic;
using System.Numerics;
using GameServerCore.Content;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.Content;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.Buildings.AnimatedBuildings;
using LeagueSandbox.GameServer.GameObjects.Other;

namespace LeagueSandbox.GameServer.GameObjects.Missiles
{
    public class Projectile : ObjMissile, IProjectile
    {
        // Function Vars.
        protected float _moveSpeed;
        private bool _atDestination;

        /// <summary>
        /// Number of objects this projectile has hit since it was created.
        /// </summary>
        public List<IGameObject> ObjectsHit { get; }
        /// <summary>
        /// Unit which owns the spell that created this projectile.
        /// </summary>
        public IAttackableUnit Owner { get; }
        /// <summary>
        /// Unique identification of this projectile.
        /// </summary>
        public int ProjectileId { get; }
        /// <summary>
        /// Projectile spell data, housing all information about this projectile's properties. Most projectiles are counted as ExtraSpells within a character's data.
        /// </summary>
        public ISpellData SpellData { get; protected set; }
        /// <summary>
        /// Current unit this projectile is homing in on and moving towards. Projectile is destroyed on contact with this unit.
        /// </summary>
        public IAttackableUnit TargetUnit { get; }
        /// <summary>
        /// Position this projectile is moving towards. Projectile is destroyed once it reaches this destination. Equals Vector2.Zero if TargetUnit is not null.
        /// </summary>
        public Vector2 Destination { get; }
        /// <summary>
        /// Spell which created this projectile.
        /// </summary>
        public ISpell OriginSpell { get; protected set; }
        /// <summary>
        /// Whether or not this projectile's visuals should not be networked to clients.
        /// </summary>
        public bool IsServerOnly { get; }

        public Projectile(
            Game game,
            float x,
            float y,
            int collisionRadius,
            IAttackableUnit owner,
            IAttackableUnit unit,
            ISpell originSpell,
            float moveSpeed,
            string projectileName,
            int flags = 0,
            uint netId = 0,
            bool serverOnly = false
        ) : base(game, x, y, collisionRadius, 0, netId)
        {
            SpellData = _game.Config.ContentManager.GetSpellData(projectileName);
            OriginSpell = originSpell;
            _moveSpeed = moveSpeed;
            Owner = owner;
            Team = owner.Team;
            ProjectileId = (int)HashFunctions.HashString(projectileName);
            if (!string.IsNullOrEmpty(projectileName))
            {
                VisionRadius = SpellData.MissilePerceptionBubbleRadius;
            }
            ObjectsHit = new List<IGameObject>();

            TargetUnit = unit;
            Destination = Vector2.Zero;
            IsServerOnly = serverOnly;
            _atDestination = false;
        }

        public Projectile(
            Game game,
            float x,
            float y,
            int collisionRadius,
            IAttackableUnit owner,
            Vector2 targetPos,
            ISpell originSpell,
            float moveSpeed,
            string projectileName,
            int flags = 0,
            uint netId = 0,
            bool serverOnly = false
        ) : this(game, x, y, collisionRadius, owner, null, originSpell, moveSpeed, projectileName, flags, netId, serverOnly)
        {
            Destination = targetPos;
        }

        public override void Update(float diff)
        {
            if (!HasTarget() || _atDestination)
            {
                SetToRemove();
                return;
            }

            base.Update(diff);
        }

        public override void OnCollision(IGameObject collider, bool isTerrain = false)
        {
            if (IsToRemove() || (TargetUnit != null && collider != TargetUnit) || (Destination != Vector2.Zero && collider is IObjBuilding))
            {
                return;
            }

            if (isTerrain)
            {
                // TODO: Implement methods for isTerrain for projectiles such as Nautilus Q, ShyvanaDragon Q, or Ziggs Q.
                return;
            }

            if (Destination != Vector2.Zero)
            {
                CheckFlagsForUnit(collider as IAttackableUnit);
            }
            else
            {
                if (TargetUnit == collider)
                {
                    CheckFlagsForUnit(collider as IAttackableUnit);
                }
            }
        }

        public override float GetMoveSpeed()
        {
            return _moveSpeed;
        }

        /// <summary>
        /// Moves this projectile to either its target unit, or its destination, and updates its coordinates along the way.
        /// </summary>
        /// <param name="diff">The amount of milliseconds the AI is supposed to move</param>
        public override void Move(float diff)
        {
            if (!HasTarget())
            {
                _direction = new Vector2();

                return;
            }

            // current position
            var cur = new Vector2(X, Y);

            var next = GetTargetPosition();

            var goingTo = next - cur;
            _direction = Vector2.Normalize(goingTo);

            // usually doesn't happen
            if (float.IsNaN(_direction.X) || float.IsNaN(_direction.Y))
            {
                _direction = new Vector2(0, 0);
            }

            var moveSpeed = GetMoveSpeed();

            var dist = MathF.Abs(Vector2.Distance(cur, next));

            var deltaMovement = moveSpeed * 0.001f * diff;

            // Prevent moving past the next waypoint.
            if (deltaMovement > dist)
            {
                deltaMovement = dist;
            }

            var xx = _direction.X * deltaMovement;
            var yy = _direction.Y * deltaMovement;

            X += xx;
            Y += yy;

            // (X, Y) has moved to the next position
            cur = new Vector2(X, Y);

            // Check if we reached the target position/destination.
            // REVIEW (of previous code): (deltaMovement * 2) being used here is problematic; if the server lags, the diff will be much greater than the usual values
            if ((cur - next).LengthSquared() < MOVEMENT_EPSILON * MOVEMENT_EPSILON)
            {
                if (this is IProjectile && TargetUnit != null)
                {
                    return;
                }

                // remove this projectile because it has reached its destination
                if (GetPosition() == Destination)
                {
                    _atDestination = true;
                }
            }
        }

        // TODO: refactor this
        protected virtual void CheckFlagsForUnit(IAttackableUnit unit)
        {
            if (!HasTarget())
            {
                return;
            }

            if (TargetUnit == null)
            {
                // Skillshot
                if (!CheckIfValidTarget(unit))
                {
                    return;
                }

                ObjectsHit.Add(unit);
                var attackableUnit = unit;
                if (attackableUnit != null)
                {
                    OriginSpell.ApplyEffects(attackableUnit, this);
                }
            }
            else
            {
                // Homing spell
                if (OriginSpell != null)
                {
                    OriginSpell.ApplyEffects(TargetUnit, this);
                }
                else
                {
                    // Auto attack
                    if (Owner is IObjAiBase ai)
                    {
                        ai.AutoAttackHit(TargetUnit);
                    }
                    SetToRemove();
                }
            }
        }

        public override void SetToRemove()
        {
            base.SetToRemove();
            _game.PacketNotifier.NotifyDestroyClientMissile(this);
        }
        
        protected bool CheckIfValidTarget(IAttackableUnit unit)
        {
            if (TargetUnit != null || unit == null || ObjectsHit.Contains(unit))
            {
                return false;
            }

            if (unit.Team == Owner.Team && !((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AFFECT_FRIENDS) > 0))
            {
                return false;
            }

            if (unit.Team == TeamId.TEAM_NEUTRAL && !((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AFFECT_NEUTRAL) > 0))
            {
                return false;
            }

            if (unit.Team != Owner.Team && unit.Team != TeamId.TEAM_NEUTRAL && !((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AFFECT_ENEMIES) > 0))
            {
                return false;
            }

            if (unit.IsDead && !((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AFFECT_DEAD) > 0))
            {
                return false;
            }

            switch (unit)
            {
                // TODO: Verify all
                // Order is important
                case ILaneMinion _ when ((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AFFECT_MINIONS) > 0):
                    return true;
                case IMinion m when (!m.IsPet && ((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AFFECT_NOT_PET) > 0))
                                || (m.IsPet && ((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AFFECT_USEABLE) > 0))
                                || (m.IsWard && ((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AFFECT_WARDS) > 0))
                                || (!m.IsClone && ((SpellData.Flags & (int)(SpellFlag.SPELL_FLAG_IGNORE_CLONES - 1)) > 0))
                                || (SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AFFECT_ALL_UNIT_TYPES) > 0:
                    if (!(unit is ILaneMinion))
                    {
                        return true;
                    }
                    return false; // already got checked in ILaneMinion
                case IBaseTurret _ when ((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AFFECT_TURRETS) > 0):
                    return true;
                case IInhibitor _ when ((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AFFECT_BUILDINGS) > 0):
                    return true;
                case INexus _ when ((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AFFECT_BUILDINGS) > 0):
                    return true;
                case IChampion _ when ((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AFFECT_HEROES) > 0):
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Whether or not this projectile has a target unit or a destination; if it is a valid projectile.
        /// </summary>
        /// <returns>True/False.</returns>
        public bool HasTarget()
        {
            return TargetUnit != null || Destination != Vector2.Zero;
        }

        /// <summary>
        /// Gets the position of this projectile's target (unit or destination).
        /// </summary>
        /// <returns>Vector2 position of target. Vector2(float.NaN, float.NaN) if projectile has no target.</returns>
        public Vector2 GetTargetPosition()
        { 
            if (!HasTarget())
            {
                return new Vector2(float.NaN, float.NaN);
            }

            if (TargetUnit != null)
            {
                return TargetUnit.GetPosition();
            }

            return Destination;
        }
    }
}
