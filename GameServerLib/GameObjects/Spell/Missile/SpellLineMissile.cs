using System;
using System.Numerics;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.Buildings;

namespace LeagueSandbox.GameServer.GameObjects.SpellNS.Missile
{
    // TODO: Complete this. Arc missiles have rotational velocity, and follow a repeating pattern similar to a wave function.
    public class SpellLineMissile : SpellCircleMissile
    {
        // Function Vars.
        private bool _atDestination;

        public override MissileType Type { get; protected set; } = MissileType.Arc;

        public SpellLineMissile(
            Game game,
            int collisionRadius,
            Spell originSpell,
            CastInfo castInfo,
            float moveSpeed,
            Vector2 overrideEndPos,
            SpellDataFlags overrideFlags = 0, // TODO: Find a use for these
            uint netId = 0,
            bool serverOnly = false
        ) : base(game, collisionRadius, originSpell, castInfo, moveSpeed, overrideEndPos, overrideFlags, netId, serverOnly)
        {
            // Basic functionality of end position is done already by SpellCircleMissile.
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
            if (IsToRemove() || (TargetUnit != null && collider != TargetUnit) || (Destination != Vector2.Zero && collider is ObjBuilding))
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
            if (unit == null || !HasDestination() || !SpellOrigin.SpellData.IsValidTarget(CastInfo.Owner, unit))
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

        // TODO: Verify if LineMissile should have its own IsValidTarget functionality.
    }
}
