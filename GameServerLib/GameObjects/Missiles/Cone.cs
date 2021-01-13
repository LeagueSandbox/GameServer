using System;
using System.Numerics;
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
    /// Class representing cone based spells.
    /// </summary>
    /// TODO: Create a generalized class for spell based hitboxes instead of inheriting Projectile.
    internal class Cone : Projectile
    {
        private bool _affectAsCastIsOver;
        private float _radius;
        private float _angleDeg;
        private Vector2 _ownerCoords;
        private float _beginAngle;
        private float _endAngle;

        public Cone(
            Game game,
            Vector2 position,
            int collisionRadius,
            IAttackableUnit owner,
            Vector2 targetPos,
            ISpell originSpell,
            string effectName,
            int flags,
            bool affectAsCastIsOver,
            float angleDeg,
            uint netid
            ) : base(game, position, collisionRadius, owner, targetPos, originSpell, 0, effectName, flags, netid)
        {
            _affectAsCastIsOver = affectAsCastIsOver;
            _angleDeg = angleDeg;
            CreateCone(position, targetPos);
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
                        if (TargetIsInCone(u))
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
            var attackableUnit = unit;
            if (attackableUnit != null)
            {
                OriginSpell.ApplyEffects(attackableUnit, this);
            }
        }

        private void CreateCone(Vector2 beginPoint, Vector2 endPoint)
        {
            var beginCoords = new Vector2(beginPoint.X, beginPoint.Y);
            var trueEndCoords = new Vector2(endPoint.X, endPoint.Y);
            var distance = Vector2.Distance(beginCoords, trueEndCoords);

            float radians = (float)Math.PI / 180.0f * _angleDeg;
            float middlePointAngle = (float)Math.Acos((endPoint.X - beginPoint.X) / distance);
            _beginAngle = middlePointAngle - radians;
            _endAngle = middlePointAngle + radians;

            _ownerCoords = beginCoords;
            _radius = distance;
        }

        /// <summary>
        /// Checks if given GameObject is inside this <see cref="Cone"/>.
        /// </summary>
        /// <param name="target">AttackableUnit to check.</param>
        /// <returns>True if unit is in rectangle, otherwise False.</returns>
        private bool TargetIsInCone(IAttackableUnit target)
        {
            var unitCoords = new Vector2(target.Position.X, target.Position.Y);
            var targetDistance = Vector2.Distance(_ownerCoords, unitCoords);

            float targetAngle = (float)Math.Acos((target.Position.X - _ownerCoords.X) / targetDistance);

            return targetDistance <= _radius && targetAngle >= _beginAngle && targetAngle <= _endAngle;
        }

        public override void OnCollision(IGameObject collider, bool isTerrain = false)
        {
        }
    }
}
