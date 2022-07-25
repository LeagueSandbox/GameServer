using System;
using System.Numerics;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;

namespace LeagueSandbox.GameServer.GameObjects.SpellNS.Missile
{
    public class SpellMissile : GameObject
    {
        // Function Vars.
        protected float _moveSpeed;
        private float _timeSinceCreation;

        /// <summary>
        /// Information about this missile's path.
        /// </summary>
        public CastInfo CastInfo { get; protected set; }
        /// <summary>
        /// What kind of behavior this missile has.
        /// </summary>
        public virtual MissileType Type { get; protected set; } = MissileType.Target;
        /// <summary>
        /// Current unit this projectile is homing in on and moving towards. Projectile is destroyed on contact with this unit unless it has more than one target.
        /// </summary>
        public AttackableUnit TargetUnit { get; protected set; }
        /// <summary>
        /// Spell which created this projectile.
        /// </summary>
        public Spell SpellOrigin { get; protected set; }
        /// <summary>
        /// Whether or not this projectile's visuals should not be networked to clients.
        /// </summary>
        public bool IsServerOnly { get; }

        public override bool IsAffectedByFoW => true;
        public override bool SpawnShouldBeHidden => true;

        public SpellMissile(
            Game game,
            int collisionRadius,
            Spell originSpell,
            CastInfo castInfo,
            float moveSpeed,
            SpellDataFlags overrideFlags = 0, // TODO: Find a use for these
            uint netId = 0,
            bool serverOnly = false
        ) : base(game, new Vector2(castInfo.SpellCastLaunchPosition.X, castInfo.SpellCastLaunchPosition.Z), collisionRadius, 0, 0, netId)
        {
            _moveSpeed = moveSpeed;
            _timeSinceCreation = 0.0f;

            SpellOrigin = originSpell;

            CastInfo = castInfo;

            // TODO: Implement full support for multiple targets.
            if (castInfo.Targets[0].Unit != null)
            {
                TargetUnit = castInfo.Targets[0].Unit;
            }

            VisionRadius = SpellOrigin.SpellData.MissilePerceptionBubbleRadius;

            Team = CastInfo.Owner.Team;

            IsServerOnly = serverOnly;
        }

        public override void Update(float diff)
        {
            if (HasTarget() && !TargetUnit.IsDead && TargetUnit.Status.HasFlag(StatusFlags.Targetable))
            {
                _timeSinceCreation += diff;
                Move(diff);
                API.ApiEventManager.OnSpellMissileUpdate.Publish(this, diff);
            }
            else
            {
                // Destroy any missiles which are targeting an untargetable unit.
                // TODO: Verify if this should apply to SpellSector.
                //Direction = new Vector3();
                SetToRemove();
            }
        }

        public override void OnCollision(GameObject collider, bool isTerrain = false)
        {
            if (IsToRemove() || (TargetUnit != null && collider != TargetUnit))
            {
                return;
            }

            if (isTerrain)
            {
                // TODO: Implement methods for isTerrain for projectiles such as Nautilus Q, ShyvanaDragon Q, or Ziggs Q.
                return;
            }
        }

        /// <summary>
        /// Gets the server-side speed that this Projectile moves at in units/sec.
        /// </summary>
        /// <returns>Units travelled per second.</returns>
        public float GetSpeed()
        {
            return _moveSpeed;
        }

        /// <summary>
        /// Gets the time since this projectile was created.
        /// </summary>
        /// <returns></returns>
        public float GetTimeSinceCreation()
        {
            return _timeSinceCreation;
        }

        /// <summary>
        /// Moves this projectile to either its target unit, or its destination, and updates its coordinates along the way.
        /// </summary>
        /// <param name="diff">The amount of milliseconds the AI is supposed to move</param>
        public virtual void Move(float diff)
        {
            // current position
            var cur = Position;

            var next = GetTargetPosition();

            var goingTo = new Vector3(next.X, _game.Map.NavigationGrid.GetHeightAtLocation(next), next.Y)
                        - new Vector3(cur.X, _game.Map.NavigationGrid.GetHeightAtLocation(cur), cur.Y);
            var dirTemp = Vector3.Normalize(goingTo);

            // usually doesn't happen
            if (float.IsNaN(dirTemp.X) || float.IsNaN(dirTemp.Y) || float.IsNaN(dirTemp.Z))
            {
                dirTemp = new Vector3(0, 0, 0);
            }

            Direction = dirTemp;

            var moveSpeed = GetSpeed();

            var dist = MathF.Abs(Vector2.Distance(cur, next));

            var deltaMovement = moveSpeed * 0.001f * diff;

            // Prevent moving past the next waypoint.
            if (deltaMovement > dist)
            {
                deltaMovement = dist;
            }

            var xx = Direction.X * deltaMovement;
            var yy = Direction.Z * deltaMovement;

            Position = new Vector2(Position.X + xx, Position.Y + yy);

            // (X, Y) has moved to the next position
            cur = Position;

            // Check if we reached the target position/destination.
            // REVIEW (of previous code): (deltaMovement * 2) being used here is problematic; if the server lags, the diff will be much greater than the usual values
            if ((cur - next).LengthSquared() < MOVEMENT_EPSILON * MOVEMENT_EPSILON)
            {
                if (this is SpellMissile && TargetUnit != null)
                {
                    if (Position == TargetUnit.Position)
                    {
                        CheckFlagsForUnit(TargetUnit);
                    }
                    return;
                }
            }
        }

        public virtual void CheckFlagsForUnit(AttackableUnit unit)
        {
            if (unit == null || !HasTarget() || !SpellOrigin.SpellData.IsValidTarget(CastInfo.Owner, unit) || TargetUnit != unit)
            {
                return;
            }

            // Targeted Spell (including auto attack spells)
            if (SpellOrigin != null)
            {
                SpellOrigin.ApplyEffects(TargetUnit, this);
            }

            if (CastInfo.Owner is ObjAIBase ai && SpellOrigin.CastInfo.IsAutoAttack)
            {
                ai.AutoAttackHit(TargetUnit);
            }

            SetToRemove();
        }

        public override void SetToRemove()
        {
            if (!IsToRemove())
            {
                API.ApiEventManager.OnSpellMissileEnd.Publish(this);

                base.SetToRemove();

                _game.PacketNotifier.NotifyDestroyClientMissile(this);
            }
        }

        /// <summary>
        /// Whether or not this projectile has a target unit or a destination; if it is a valid projectile.
        /// </summary>
        /// <returns>True/False.</returns>
        public bool HasTarget()
        {
            return TargetUnit != null;
        }

        /// <summary>
        /// Gets the position of this projectile's target (unit or destination).
        /// </summary>
        /// <returns>Vector2 position of target. Vector2(float.NaN, float.NaN) if projectile has no target.</returns>
        public Vector2 GetTargetPosition()
        {
            if (TargetUnit != null)
            {
                return TargetUnit.Position;
            }

            return Vector2.Zero;
        }
    }
}
