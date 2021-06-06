using System;
using System.Numerics;
using GameServerCore;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Sector;

namespace LeagueSandbox.GameServer.GameObjects.Spell.Sector
{
    /// <summary>
    /// Base class for all spell sectors. Functionally acts as a circular spell hitbox.
    /// Base functionality can be overriden to fit a specific shape.
    /// </summary>
    internal class SpellSectorCone : SpellSector, ISpellSector
    {
        public SpellSectorCone(
            Game game,
            ISectorParameters parameters,
            ISpell originSpell,
            ICastInfo castInfo,
            uint netId = 0
        ) : base(game, parameters, originSpell, castInfo, netId)
        {
            Position = new Vector2(castInfo.SpellCastLaunchPosition.X, castInfo.SpellCastLaunchPosition.Z);

            var goingTo = new Vector2(castInfo.TargetPositionEnd.X, castInfo.TargetPositionEnd.Z) - Position;
            var dirTemp = Vector2.Normalize(goingTo);

            // usually doesn't happen
            if (Parameters.BindObject != null)
            {
                if (float.IsNaN(Parameters.BindObject.Direction.X) || float.IsNaN(Parameters.BindObject.Direction.Y))
                {
                    dirTemp = new Vector2(1, 0);
                }
                else
                {
                    dirTemp = new Vector2(Parameters.BindObject.Direction.X, Parameters.BindObject.Direction.Z);
                }

                var endPos = Position + (dirTemp * SpellOrigin.GetCurrentCastRange());
                CastInfo.TargetPositionEnd = new Vector3(endPos.X, 0, endPos.Y);
            }
            else if (float.IsNaN(dirTemp.X) || float.IsNaN(dirTemp.Y))
            {
                dirTemp = new Vector2(1, 0);
            }

            Direction = new Vector3(dirTemp.X, 0, dirTemp.Y);

            VisionRadius = SpellOrigin.SpellData.MissilePerceptionBubbleRadius;

            Team = CastInfo.Owner.Team;
        }

        public override void Move(float diff)
        {
            // Change direction to the bind object's facing direction.
            Direction = Parameters.BindObject.Direction;

            // Then move.
            base.Move(diff);
        }

        /// <summary>
        /// Filter function which checks if the given collider is within the bounds of a hitbox.
        /// </summary>
        /// <param name="collider">Object to check.</param>
        /// <returns>True/False.</returns>
        protected override bool FilterCollisions(IGameObject collider)
        {
            if (Parameters.ConeAngle <= 0)
            {
                return false;
            }

            // Here we simply check if the angle from the sector position to the collider's left and right bounds is greater than the ConeAngle.

            // Get the current direction of the sector
            var angleDir = Extensions.UnitVectorToAngle(new Vector2(Direction.X, Direction.Z));
            // Get the left and round bounds of the collider (from the sector's perspective)
            var colliderBounds = Extensions.CastRayCircleBounds(Position, collider.Position, collider.CollisionRadius);

            // True if the angle from the sector to either the left or right bounds is within the ConeAngle.
            return MathF.Abs(Position.AngleTo(colliderBounds[0], Position) - angleDir) <= Parameters.ConeAngle
                || MathF.Abs(Position.AngleTo(colliderBounds[1], Position) - angleDir) <= Parameters.ConeAngle;
        }
    }
}
