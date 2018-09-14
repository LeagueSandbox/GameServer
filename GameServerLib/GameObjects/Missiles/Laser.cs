using System;
using System.Numerics;
using GameServerCore;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.Content;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.Buildings.AnimatedBuildings;
using LeagueSandbox.GameServer.GameObjects.Other;
using LeagueSandbox.GameServer.GameObjects.Spells;

namespace LeagueSandbox.GameServer.GameObjects.Missiles
{
    internal class Laser : Projectile
    {
        private bool _affectAsCastIsOver;
        private Vector2 _rectangleCornerBegin1;
        private Vector2 _rectangleCornerBegin2;
        private Vector2 _rectangleCornerEnd1;
        private Vector2 _rectangleCornerEnd2;

        public Laser(
            Game game,
            Vector2 position,
            int collisionRadius,
            AttackableUnit owner,
            Target target,
            Spell originSpell,
            string effectName,
            int flags,
            bool affectAsCastIsOver) : base(game, position, collisionRadius, owner, target, originSpell, 0, effectName, flags)
        {
            SpellData = _game.Config.ContentManager.GetSpellData(effectName);
            CreateRectangle(new Target(position), target);
            _affectAsCastIsOver = affectAsCastIsOver;
        }

        public override void Update(float diff)
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

        public override void SetToRemove()
        {
            if (Target != null && !Target.IsSimpleTarget)
            {
                (Target as GameObject).DecrementAttackerCount();
            }

            Owner.DecrementAttackerCount();
            base.SetToRemove();
            _game.PacketNotifier.NotifyProjectileDestroy(this);
        }

        private bool CheckIfValidTarget(AttackableUnit unit)
        {
            if (!Target.IsSimpleTarget)
            {
                return false;
            }

            if (unit == null || ObjectsHit.Contains(unit))
            {
                return false;
            }

            if (unit.Team == Owner.Team
                && !((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AFFECT_FRIENDS) > 0))
            {
                return false;
            }

            if (unit.Team == TeamId.TEAM_NEUTRAL
                && !((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AFFECT_NEUTRAL) > 0))
            {
                return false;
            }

            if (unit.Team != Owner.Team
                && unit.Team != TeamId.TEAM_NEUTRAL
                && !((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AFFECT_ENEMIES) > 0))
            {
                return false;
            }

            if (unit.IsDead && !((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AFFECT_DEAD) > 0))
            {
                return false;
            }

            var m = unit as Minion;
            if (m != null && !((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AFFECT_MINIONS) > 0))
            {
                return false;
            }

            var p = unit as Placeable;
            if (p != null && !((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AFFECT_USEABLE) > 0))
            {
                return false;
            }

            var t = unit as BaseTurret;
            if (t != null && !((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AFFECT_TURRETS) > 0))
            {
                return false;
            }

            var i = unit as Inhibitor;
            var n = unit as Nexus;
            if ((i != null || n != null) && !((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AFFECT_BUILDINGS) > 0))
            {
                return false;
            }

            var c = unit as Champion;
            if (c != null && !((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AFFECT_HEROES) > 0))
            {
                return false;
            }

            return true;
        }

        private void ApplyEffects(AttackableUnit unit)
        {
            ObjectsHit.Add(unit);
            var attackableUnit = unit;
            if (attackableUnit != null)
            {
                _originSpell.ApplyEffects(attackableUnit, this);
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
            var distance = Vector2.Distance(beginPoint.Position, endPoint.Position);
            var fakeEndCoords = new Vector2(beginPoint.Position.X, beginPoint.Position.Y + distance);
            var startCorner1 = new Vector2(beginPoint.Position.X + CollisionRadius, beginPoint.Position.Y);
            var startCorner2 = new Vector2(beginPoint.Position.X - CollisionRadius, beginPoint.Position.Y);
            var endCorner1 = new Vector2(fakeEndCoords.X + CollisionRadius, fakeEndCoords.Y);
            var endCorner2 = new Vector2(fakeEndCoords.X - CollisionRadius, fakeEndCoords.Y);

            var angle = fakeEndCoords.AngleBetween(endPoint.Position, beginPoint.Position);

            _rectangleCornerBegin1 = startCorner1.Rotate(beginPoint.Position, angle);
            _rectangleCornerBegin2 = startCorner2.Rotate(beginPoint.Position, angle);
            _rectangleCornerEnd1 = endCorner1.Rotate(beginPoint.Position, angle);
            _rectangleCornerEnd2 = endCorner2.Rotate(beginPoint.Position, angle);
        }

        /// <summary>
        /// Checks if given target is inside corners of this <see cref="Laser"/>.
        /// </summary>
        /// <param name="target">Target to be checked</param>
        /// <returns>true if target is in rectangle, otherwise false.</returns>
        private bool TargetIsInRectangle(AttackableUnit target)
        {
            var shortSide = Vector2.Distance(_rectangleCornerBegin1, _rectangleCornerBegin2);
            var longSide = Vector2.Distance(_rectangleCornerBegin1, _rectangleCornerEnd1);

            var totalArea = longSide * shortSide;

            var triangle1Area = GetTriangleArea(_rectangleCornerBegin1, _rectangleCornerBegin2, target.Position);
            var triangle2Area = GetTriangleArea(_rectangleCornerBegin1, _rectangleCornerEnd1, target.Position);
            var triangle3Area = GetTriangleArea(_rectangleCornerBegin2, _rectangleCornerEnd2, target.Position);
            var triangle4Area = GetTriangleArea(_rectangleCornerEnd1, _rectangleCornerEnd2, target.Position);

            return totalArea >= triangle1Area + triangle2Area + triangle3Area + triangle4Area;
        }

        /// <summary>
        /// Calculates given triangle's area using Heron's formula.
        /// </summary>
        /// <param name="first">First corner of the triangle.</param>
        /// <param name="second">Second corner of the triangle</param>
        /// <param name="third">Third corner of the triangle.</param>
        /// <returns>the area of the triangle.</returns>
        private float GetTriangleArea(Vector2 first, Vector2 second, Vector2 third)
        {
            var line1Length = Vector2.Distance(first, second);
            var line2Length = Vector2.Distance(second, third);
            var line3Length = Vector2.Distance(third, first);

            var s = (line1Length + line2Length + line3Length) / 2;

            return (float)Math.Sqrt(s * (s - line1Length) * (s - line2Length) * (s - line3Length));
        }
    }
}
