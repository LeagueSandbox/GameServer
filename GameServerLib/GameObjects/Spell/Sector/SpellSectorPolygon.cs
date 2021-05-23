using System;
using System.Collections.Generic;
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
    internal class SpellSectorPolygon : SpellSector, ISpellSector
    {
        private Vector2[] _trueVertices;
        private float _trueWidth;
        private float _trueHalfLength;

        public SpellSectorPolygon(
            Game game,
            ISectorParameters parameters,
            ISpell originSpell,
            ICastInfo castInfo,
            uint netId = 0
        ) : base(game, parameters, originSpell, castInfo, netId)
        {
            // TODO: Verify if assignment is necessary.
            _trueVertices = new Vector2[parameters.PolygonVertices.Length];

            // This whole section involving vertices is simply scaling vertices by Width/HalfLength,
            // then making sure the distance between vertices after scaling is less than Width/HalfLength
            // (otherwise, we override them)
            parameters.PolygonVertices.CopyTo(_trueVertices, 0);
            _trueWidth = parameters.Width;
            _trueHalfLength = parameters.HalfLength;

            Vector2? lastVertex = null;
            for (int i = 0; i < _trueVertices.Length; i++)
            {
                Vector2 currVertex = _trueVertices[i];

                // Scale the vertex
                Vector2 trueVertex = new Vector2(currVertex.X * parameters.Width, currVertex.Y * parameters.HalfLength);

                // Compare distances and override HalfWidth/Length as necessary
                if (lastVertex != null)
                {
                    var distX = currVertex.X - trueVertex.X;
                    if (distX > _trueWidth)
                    {
                        _trueWidth = distX;
                    }

                    var distY = currVertex.Y - trueVertex.Y;
                    if (distY > _trueHalfLength)
                    {
                        _trueHalfLength = distY;
                    }
                }

                // Reassign with true width/halflength.
                trueVertex = new Vector2(currVertex.X * _trueWidth, currVertex.Y * _trueHalfLength);

                // Save current vertex for next iteration so we can compare distance.
                lastVertex = trueVertex;
                // Assign scaled vertex.
                _trueVertices[i] = trueVertex;
            }

            if (_trueWidth > _trueHalfLength)
            {
                CollisionRadius = _trueWidth;
            }
            else
            {
                CollisionRadius = _trueHalfLength;
            }

            Position = GetTargetPosition();

            // The below section is making sure the sector has a correct direction.

            var goingTo = new Vector2(castInfo.TargetPositionEnd.X, castInfo.TargetPositionEnd.Z) - Position;
            var dirTemp = Vector2.Normalize(goingTo);

            if (float.IsNaN(dirTemp.X) || float.IsNaN(dirTemp.Y) && Parameters.BindObject != null)
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
                var startPos = Position - (dirTemp * SpellOrigin.GetCurrentCastRange());
                CastInfo.TargetPosition = new Vector3(startPos.X, 0, startPos.Y);
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

            base.Move(diff);
        }

        /// <summary>
        /// Filter function which checks if the given collider is within the bounds of a hitbox.
        /// </summary>
        /// <param name="collider">Object to check.</param>
        /// <returns>True/False.</returns>
        protected override bool FilterCollisions(IGameObject collider)
        {
            if (_trueVertices.Length <= 0)
            {
                return false;
            }

            var start = new Vector2(CastInfo.TargetPosition.X, CastInfo.TargetPosition.Z);
            var end = new Vector2(CastInfo.TargetPositionEnd.X, CastInfo.TargetPositionEnd.Z);

            // First check if the start or end points are inside the collider.
            // Without this check, due to how the sector was positioned in initialization, units at the very edge of the sector will fail the circle -> line intersection check.
            if (Extensions.IsVectorWithinRange(start, collider.Position, collider.CollisionRadius)
                || Extensions.IsVectorWithinRange(end, collider.Position, collider.CollisionRadius))
            {
                return true;
            }

            // Get the current direction of the sector (negated and added 90 degrees for clockwise rotation of vertices)
            var angleDir = -Extensions.UnitVectorToAngle(new Vector2(Direction.X, Direction.Z)) + 90f;

            // This section checks if the any of the lines connecting each vertex intersect with the collider's collision radius.
            var collision = false;
            int next = 0;
            for (int curr = 0; curr < _trueVertices.Length; curr++)
            {
                next = curr++;

                // Last vertex connects to the first.
                if (next == _trueVertices.Length)
                {
                    next = 0;
                }

                // Move to sector position, rotate with facing direction, then make collider.Position the origin for CircleLineIntersection.
                var currVert = Position + _trueVertices[curr].Rotate(angleDir) - collider.Position;
                var nextVert = Position + _trueVertices[next].Rotate(angleDir) - collider.Position;

                // Then, for each vertex, check if it is colliding.
                if (Extensions.IsVectorWithinRange(currVert, Vector2.Zero, collider.CollisionRadius)
                    || Extensions.IsVectorWithinRange(nextVert, Vector2.Zero, collider.CollisionRadius))
                {
                    return true;
                }

                // Otherwise, we perform a circle -> line intersection check.
                collision = Extensions.CircleLineIntersection(currVert, nextVert, collider.CollisionRadius).Count > 0;

                if (collision)
                {
                    break;
                }
            }

            return collision;
        }

        public override Vector2 GetTargetPosition()
        {
            if (Parameters.BindObject != null)
            {
                return API.ApiFunctionManager.GetPointFromUnit(Parameters.BindObject, _trueHalfLength);
            }

            return new Vector2(CastInfo.SpellCastLaunchPosition.X, CastInfo.SpellCastLaunchPosition.Z);
        }
    }
}
