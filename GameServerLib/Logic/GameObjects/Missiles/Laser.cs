﻿using System;
using System.Linq;
using System.Numerics;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    class Laser : Projectile
    {
        private bool _affectAsCastIsOver;
        private Vector2 _rectangleCornerBegin1;
        private Vector2 _rectangleCornerBegin2;
        private Vector2 _rectangleCornerEnd1;
        private Vector2 _rectangleCornerEnd2;

        public Laser(
            float x,
            float y,
            int collisionRadius,
            AttackableUnit owner,
            Target target,
            Spell originSpell,
            int flags,
            bool affectAsCastIsOver) : base(x, y, collisionRadius, owner, target, originSpell, 0, "", flags)
        {
            CreateRectangle(new Target(x, y), target);
            _affectAsCastIsOver = affectAsCastIsOver;
        }

        public override void update(float diff)
        {
            if (!_affectAsCastIsOver)
            {
                return;
            }

            if (_originSpell.State != SpellState.STATE_CASTING)
            {
                var objects = _game.ObjectManager.GetObjects().Values;
                foreach (var obj in objects)
                {
                    var u = obj as AttackableUnit;
                    if (u != null && TargetIsInRectangle(u))
                    {
                        CheckFlagsForUnit(u);
                    }
                }

                setToRemove();
            }
        }

        public override void setToRemove()
        {
            if (Target != null && !Target.IsSimpleTarget)
            {
                (Target as GameObject).decrementAttackerCount();
            }

            Owner.decrementAttackerCount();
            toRemove = true;
        }

        protected override void CheckFlagsForUnit(AttackableUnit unit)
        {
            if (!Target.IsSimpleTarget)
                return;

            if (unit == null || ObjectsHit.Contains(unit))
                return;

            if (unit.Team == Owner.Team
                && !((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AffectFriends) > 0))
                return;

            if (unit.Team == TeamId.TEAM_NEUTRAL
                && !((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AffectNeutral) > 0))
                return;

            if (unit.Team != Owner.Team
                && unit.Team != TeamId.TEAM_NEUTRAL
                && !((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AffectEnemies) > 0))
                return;


            if (unit.IsDead && !((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AffectDead) > 0))
                return;

            var m = unit as Minion;
            if (m != null && !((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AffectMinions) > 0))
                return;

            var p = unit as Placeable;
            if (p != null && !((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AffectUseable) > 0))
                return;

            var t = unit as BaseTurret;
            if (t != null && !((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AffectTurrets) > 0))
                return;

            var i = unit as Inhibitor;
            var n = unit as Nexus;
            if ((i != null || n != null) && !((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AffectBuildings) > 0))
                return;

            var c = unit as Champion;
            if (c != null && !((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AffectHeroes) > 0))
                return;

            ObjectsHit.Add(unit);
            if (unit is AttackableUnit attackableUnit)
            {
                _originSpell.applyEffects(attackableUnit, this);
            }
        }

        /* WARNING!
         * METHODS BELOW CONTAIN TOO MUCH MATHS.
         * PLEASE TURN BACK NOW IF YOU DON'T WANT YOUR BRAIN TO BE BLOWN.
         */

        /// <summary>
        /// Assigns this <see cref="Laser"/>'s corners to form a rectangle.
        /// </summary>
        private void CreateRectangle(Target beginPoint, Target endPoint)
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
        private bool TargetIsInRectangle(AttackableUnit target)
        {
            var unitCoords = new Vector2(target.X, target.Y);

            var shortSide = Vector2.Distance(_rectangleCornerBegin1, _rectangleCornerBegin2);
            var longSide = Vector2.Distance(_rectangleCornerBegin1, _rectangleCornerEnd1);

            var totalArea = longSide * shortSide;

            var triangleAreas = new double[4];

            triangleAreas[0] = GetTriangleArea(_rectangleCornerBegin1, _rectangleCornerBegin2, unitCoords);
            triangleAreas[1] = GetTriangleArea(_rectangleCornerBegin1, _rectangleCornerEnd1, unitCoords);
            triangleAreas[2] = GetTriangleArea(_rectangleCornerBegin2, _rectangleCornerEnd2, unitCoords);
            triangleAreas[3] = GetTriangleArea(_rectangleCornerEnd1, _rectangleCornerEnd2, unitCoords);

            return Math.Abs(totalArea - triangleAreas.Sum()) < 0.0001;
        }

        /// <summary>
        /// Calculates given triangle's area using Heron's formula.
        /// </summary>
        /// <param name="first">First corner of the triangle.</param>
        /// <param name="second">Second corner of the triangle</param>
        /// <param name="third">Third corner of the triangle.</param>
        /// <returns>the area of the triangle.</returns>
        private double GetTriangleArea(Vector2 first, Vector2 second, Vector2 third)
        {
            var line1Length = Vector2.Distance(first, second);
            var line2Length = Vector2.Distance(second, third);
            var line3Length = Vector2.Distance(third, first);

            var s = (line1Length + line2Length + line3Length) / 2d;

            return Math.Sqrt(s * (s - line1Length) * (s - line2Length) * (s - line3Length));
        }
    }
}
