using System.Numerics;
using GameServerCore;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects.Spell.Missile;

namespace LeagueSandbox.GameServer.GameObjects.Spell.Sector
{
    /// <summary>
    /// Class representing rectangular spell hitboxes.
    /// </summary>
    /// TODO: Create a generalized class for spell based hitboxes instead of inheriting Projectile.
    /// TODO: Refactor the collision detection method for this class.
    internal class Laser : SpellCircleMissile
    {
        private bool _affectAsCastIsOver;
        private Vector2 _rectangleCornerBegin1;
        private Vector2 _rectangleCornerBegin2;
        private Vector2 _rectangleCornerEnd1;
        private Vector2 _rectangleCornerEnd2;

        public Laser(
            Game game,
            int collisionRadius,
            ISpell originSpell,
            ICastInfo castInfo,
            string effectName,
            SpellDataFlags flags,
            bool affectAsCastIsOver,
            uint netid) : base(game, collisionRadius, originSpell, castInfo, 0, flags, netid)
        {
            CreateRectangle(new Vector2(castInfo.TargetPosition.X, castInfo.TargetPosition.Z), new Vector2(castInfo.TargetPositionEnd.X, castInfo.TargetPositionEnd.Z));
            _affectAsCastIsOver = affectAsCastIsOver;
        }

        public override void Update(float diff)
        {
            if (!_affectAsCastIsOver)
            {
                var objects = _game.ObjectManager.GetObjects().Values;
                foreach (var obj in objects)
                {
                    var u = obj as IAttackableUnit;
                    if (u != null && IsValidTarget(u))
                    {
                        if (TargetIsInRectangle(u))
                        {
                            ApplyEffects(u);
                        }
                    }
                }

                return;
            }

            if (SpellOrigin.State != SpellState.STATE_CASTING)
            {
                var objects = _game.ObjectManager.GetObjects().Values;
                foreach (var obj in objects)
                {
                    var u = obj as IAttackableUnit;
                    if (u != null && IsValidTarget(u))
                    {
                        if (TargetIsInRectangle(u))
                        {
                            ApplyEffects(u);
                        }
                    }
                }

                SetToRemove();
            }
        }

        private void ApplyEffects(IAttackableUnit unit)
        {
            ObjectsHit.Add(unit);
            if (unit != null)
            {
                if (SpellOrigin != null)
                {
                    SpellOrigin.ApplyEffects(unit, this);
                }
            }
        }

        /// <summary>
        /// Assigns this <see cref="Laser"/>'s corners to form a rectangle.
        /// </summary>
        private void CreateRectangle(Vector2 beginPoint, Vector2 endPoint)
        {
            var distance = Vector2.Distance(beginPoint, endPoint);
            var fakeEndCoords = new Vector2(beginPoint.X, endPoint.Y + distance);
            var startCorner1 = new Vector2(beginPoint.X + CollisionRadius, endPoint.Y);
            var startCorner2 = new Vector2(beginPoint.X - CollisionRadius, endPoint.Y);
            var endCorner1 = new Vector2(fakeEndCoords.X + CollisionRadius, fakeEndCoords.Y);
            var endCorner2 = new Vector2(fakeEndCoords.X - CollisionRadius, fakeEndCoords.Y);

            var angle = fakeEndCoords.AngleBetween(endPoint, beginPoint);

            _rectangleCornerBegin1 = startCorner1.Rotate(beginPoint, angle);
            _rectangleCornerBegin2 = startCorner2.Rotate(beginPoint, angle);
            _rectangleCornerEnd1 = endCorner1.Rotate(beginPoint, angle);
            _rectangleCornerEnd2 = endCorner2.Rotate(beginPoint, angle);
        }

        /// <summary>
        /// Checks if given target is inside corners of this <see cref="Laser"/>.
        /// </summary>
        /// <param name="target">Target to be checked</param>
        /// <returns>true if target is in rectangle, otherwise false.</returns>
        private bool TargetIsInRectangle(IAttackableUnit target)
        {
            var unitCoords = new Vector2(target.Position.X, target.Position.Y);

            var shortSide = Vector2.Distance(_rectangleCornerBegin1, _rectangleCornerBegin2);
            var longSide = Vector2.Distance(_rectangleCornerBegin1, _rectangleCornerEnd1);

            var totalArea = longSide * shortSide;

            var triangle1Area = Extensions.GetTriangleArea(_rectangleCornerBegin1, _rectangleCornerBegin2, unitCoords);
            var triangle2Area = Extensions.GetTriangleArea(_rectangleCornerBegin1, _rectangleCornerEnd1, unitCoords);
            var triangle3Area = Extensions.GetTriangleArea(_rectangleCornerBegin2, _rectangleCornerEnd2, unitCoords);
            var triangle4Area = Extensions.GetTriangleArea(_rectangleCornerEnd1, _rectangleCornerEnd2, unitCoords);

            return totalArea >= triangle1Area + triangle2Area + triangle3Area + triangle4Area;
        }

        public override void OnCollision(IGameObject collider, bool isTerrain = false)
        {
        }
    }
}
