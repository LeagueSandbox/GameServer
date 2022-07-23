using System;
using System.Collections.Generic;
using System.Numerics;
using GameMaths.Geometry.Polygons;
using GameServerCore;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace LeagueSandbox.GameServer.GameObjects.SpellNS.Sector
{
    /// <summary>
    /// Base class for all spell sectors. Functionally acts as a circular spell hitbox.
    /// Base functionality can be overriden to fit a specific shape.
    /// </summary>
    internal class SpellSectorPolygon : SpellSector
    {
        private Vector2[] _trueVertices;
        private float _trueWidth;
        private float _trueLength;
        private Polygon _clipperPoly;

        public SpellSectorPolygon(
            Game game,
            SectorParameters parameters,
            Spell originSpell,
            CastInfo castInfo,
            uint netId = 0
        ) : base(game, parameters, originSpell, castInfo, netId)
        {
            // TODO: Verify if assignment is necessary.
            _trueVertices = new Vector2[parameters.PolygonVertices.Length];

            // This whole section involving vertices is simply scaling vertices by Width/Length,
            // then making sure the distance between vertices after scaling is less than Width/Length
            // (otherwise, we override them)
            parameters.PolygonVertices.CopyTo(_trueVertices, 0);
            _trueWidth = parameters.Width;
            _trueLength = parameters.Length;

            _clipperPoly = new Polygon();

            Vector2? lastVertex = null;
            for (int i = 0; i < _trueVertices.Length; i++)
            {
                Vector2 currVertex = _trueVertices[i];

                // Scale the vertex
                Vector2 trueVertex = new Vector2(currVertex.X * parameters.Width, currVertex.Y * parameters.Length);

                // Compare distances and override Width/Length as necessary
                if (lastVertex != null)
                {
                    var distX = currVertex.X - trueVertex.X;
                    if (distX > _trueWidth)
                    {
                        _trueWidth = distX;
                    }

                    var distY = currVertex.Y - trueVertex.Y;
                    if (distY > _trueLength)
                    {
                        _trueLength = distY;
                    }
                }

                // Reassign with true width/length.
                trueVertex = new Vector2(currVertex.X * _trueWidth, currVertex.Y * _trueLength);

                // Save current vertex for next iteration so we can compare distance.
                lastVertex = trueVertex;
                // Assign scaled vertex.
                _trueVertices[i] = trueVertex;

                _clipperPoly.Add(trueVertex);
            }

            if (_trueWidth > _trueLength)
            {
                CollisionRadius = _trueWidth;
            }
            else
            {
                CollisionRadius = _trueLength;
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
        protected override bool FilterCollisions(GameObject collider)
        {
            if (_trueVertices.Length <= 0)
            {
                return false;
            }

            // Get the current direction of the sector.
            var angleDir = Extensions.UnitVectorToAngle(new Vector2(Direction.X, Direction.Z));
            var startPos = new Vector2(CastInfo.SpellCastLaunchPosition.X, CastInfo.SpellCastLaunchPosition.Z);
            if (Parameters.BindObject != null)
            {
                startPos = Parameters.BindObject.Position;
            }

            // Get position of collider relative to polygon (including rotation).
            var relativePos = (collider.Position - startPos).Rotate(angleDir + 270f);
            return _clipperPoly.IsInside(relativePos);
        }

        public override Vector2 GetTargetPosition()
        {
            if (Parameters.BindObject != null)
            {
                // Center of polygon
                return API.ApiFunctionManager.GetPointFromUnit(Parameters.BindObject, _trueLength / 2f);
            }

            return new Vector2(CastInfo.SpellCastLaunchPosition.X, CastInfo.SpellCastLaunchPosition.Z);
        }

        public Vector2[] GetPolygonVertices()
        {
            return _trueVertices;
        }
    }
}
