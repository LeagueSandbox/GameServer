using System;
using System.Numerics;
using GameServerCore;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.Content;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.Buildings.AnimatedBuildings;
using LeagueSandbox.GameServer.GameObjects.Other;
using LeagueSandbox.GameServer.GameObjects.Spells;

namespace LeagueSandbox.GameServer.GameObjects.Missiles
{
    /// <summary>
    /// Class representing rectangular spell hitboxes.
    /// </summary>
    /// TODO: Create a generalized class for spell based hitboxes instead of inheriting Projectile.
    /// TODO: Refactor the collision detection method for this class.
    internal class Laser : Projectile
    {
        private bool _affectAsCastIsOver;
        private Vector2 _rectangleCornerBegin1;
        private Vector2 _rectangleCornerBegin2;
        private Vector2 _rectangleCornerEnd1;
        private Vector2 _rectangleCornerEnd2;

        public Laser(
            Game game,
            float x,
            float y,
            int collisionRadius,
            IAttackableUnit owner,
            Vector2 targetPos,
            ISpell originSpell,
            string effectName,
            int flags,
            bool affectAsCastIsOver,
            uint netid) : base(game, x, y, collisionRadius, owner, targetPos, originSpell, 0, effectName, flags, netid)
        {
            SpellData = _game.Config.ContentManager.GetSpellData(effectName);
            CreateRectangle(new Vector2(x, y), targetPos);
            _affectAsCastIsOver = affectAsCastIsOver;
        }

        public override void Update(float diff)
        {
            if (!_affectAsCastIsOver)
            {
                return;
            }

            if (OriginSpell.State != SpellState.STATE_CASTING)
            {
                var objects = _game.ObjectManager.GetObjects().Values;
                foreach (var obj in objects)
                {
                    var u = obj as IAttackableUnit;
                    if (u != null && CheckIfValidTarget(u))
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
                OriginSpell.ApplyEffects(unit, this);
            }
        }

        /// <summary>
        /// Assigns this <see cref="Laser"/>'s corners to form a rectangle.
        /// </summary>
        private void CreateRectangle(Vector2 beginPoint, Vector2 endPoint)
        {
            var beginCoords = new Vector2(beginPoint.X, beginPoint.Y);
            var trueEndCoords = new Vector2(endPoint.X, endPoint.Y);
            var distance = Vector2.Distance(beginCoords, trueEndCoords);
            var fakeEndCoords = new Vector2(beginCoords.X, beginCoords.Y + distance);
            var startCorner1 = new Vector2(beginCoords.X + CollisionRadius, beginCoords.Y);
            var startCorner2 = new Vector2(beginCoords.X - CollisionRadius, beginCoords.Y);
            var endCorner1 = new Vector2(fakeEndCoords.X + CollisionRadius, fakeEndCoords.Y);
            var endCorner2 = new Vector2(fakeEndCoords.X - CollisionRadius, fakeEndCoords.Y);

            var angle = fakeEndCoords.AngleBetween(trueEndCoords, beginCoords);

            _rectangleCornerBegin1 = startCorner1.Rotate(beginCoords, angle);
            _rectangleCornerBegin2 = startCorner2.Rotate(beginCoords, angle);
            _rectangleCornerEnd1 = endCorner1.Rotate(beginCoords, angle);
            _rectangleCornerEnd2 = endCorner2.Rotate(beginCoords, angle);
        }

        /// <summary>
        /// Checks if given target is inside corners of this <see cref="Laser"/>.
        /// </summary>
        /// <param name="target">Target to be checked</param>
        /// <returns>true if target is in rectangle, otherwise false.</returns>
        private bool TargetIsInRectangle(IAttackableUnit target)
        {
            var unitCoords = new Vector2(target.X, target.Y);

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
