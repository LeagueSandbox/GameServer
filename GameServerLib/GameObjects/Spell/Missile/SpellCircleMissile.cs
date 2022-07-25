using System;
using System.Collections.Generic;
using System.Numerics;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.Buildings;

namespace LeagueSandbox.GameServer.GameObjects.SpellNS.Missile
{
    public class SpellCircleMissile : SpellMissile
    {
        // Function Vars.
        private bool _atDestination;

        public override MissileType Type { get; protected set; } = MissileType.Circle;
        /// <summary>
        /// Number of objects this projectile has hit since it was created.
        /// </summary>
        public List<GameObject> ObjectsHit { get; }
        /// <summary>
        /// Position this projectile is moving towards. Projectile is destroyed once it reaches this destination. Equals Vector2.Zero if TargetUnit is not null.
        /// </summary>
        public Vector2 Destination { get; protected set; }

        public SpellCircleMissile(
            Game game,
            int collisionRadius,
            Spell originSpell,
            CastInfo castInfo,
            float moveSpeed,
            Vector2 overrideEndPos,
            SpellDataFlags overrideFlags = 0, // TODO: Find a use for these
            uint netId = 0,
            bool serverOnly = false
        ) : base(game, collisionRadius, originSpell, castInfo, moveSpeed, overrideFlags, netId, serverOnly)
        {
            // TODO: Verify if there is a case which contradicts this.
            // Line and Circle Missiles are location targeted only.
            TargetUnit = null;

            Position = new Vector2(castInfo.SpellCastLaunchPosition.X, castInfo.SpellCastLaunchPosition.Z);

            var goingTo = new Vector2(castInfo.TargetPositionEnd.X, castInfo.TargetPositionEnd.Z) - Position;
            var dirTemp = Vector2.Normalize(goingTo);
            var endPos = Position + (dirTemp * SpellOrigin.GetCurrentCastRange());

            // usually doesn't happen
            if (float.IsNaN(dirTemp.X) || float.IsNaN(dirTemp.Y))
            {
                if (float.IsNaN(CastInfo.Owner.Direction.X) || float.IsNaN(CastInfo.Owner.Direction.Y))
                {
                    dirTemp = new Vector2(1, 0);
                }
                else
                {
                    dirTemp = new Vector2(CastInfo.Owner.Direction.X, CastInfo.Owner.Direction.Z);
                }

                endPos = Position + (dirTemp * SpellOrigin.GetCurrentCastRange());
                CastInfo.TargetPositionEnd = new Vector3(endPos.X, 0, endPos.Y);
            }

            if (overrideEndPos != default)
            {
                endPos = overrideEndPos;
            }

            Destination = endPos;

            ObjectsHit = new List<GameObject>();
        }

        public override void Update(float diff)
        {
            if (!HasDestination() || _atDestination)
            {
                SetToRemove();
                return;
            }

            Move(diff);
        }

        public override void OnCollision(GameObject collider, bool isTerrain = false)
        {
            if (IsToRemove() || (Destination != Vector2.Zero && collider is ObjBuilding))
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
                CheckFlagsForUnit(collider as AttackableUnit);
            }
        }

        /// <summary>
        /// Moves this projectile to either its target unit, or its destination, and updates its coordinates along the way.
        /// </summary>
        /// <param name="diff">The amount of milliseconds the AI is supposed to move</param>
        public override void Move(float diff)
        {
            // current position
            var cur = Position;

            var next = Destination;

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
                // remove this projectile because it has reached its destination
                if (Position == Destination)
                {
                    _atDestination = true;
                }
            }
        }

        public override void CheckFlagsForUnit(AttackableUnit unit)
        {
            if (unit == null || !HasDestination() || ObjectsHit.Contains(unit) || !SpellOrigin.SpellData.IsValidTarget(CastInfo.Owner, unit))
            {
                return;
            }

            ObjectsHit.Add(unit);

            if (SpellOrigin != null)
            {
                SpellOrigin.ApplyEffects(unit, this);
            }

            if (CastInfo.Owner is ObjAIBase ai && SpellOrigin.CastInfo.IsAutoAttack)
            {
                ai.AutoAttackHit(TargetUnit);
            }
        }

        /// <summary>
        /// Whether or not this projectile has a destination; if it is a valid projectile.
        /// </summary>
        /// <returns>True/False.</returns>
        public bool HasDestination()
        {
            return Destination != Vector2.Zero && Destination.X != float.NaN && Destination.Y != float.NaN;
        }
    }
}
