using System;
using System.Numerics;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.Content;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.Buildings.AnimatedBuildings;
using LeagueSandbox.GameServer.GameObjects.Other;
using LeagueSandbox.GameServer.GameObjects.Spells;


namespace LeagueSandbox.GameServer.GameObjects.Missiles
{
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
            float x,
            float y,
            int collisionRadius,
            AttackableUnit owner,
            Target target,
            Spell originSpell,
            string effectName,
            int flags,
            bool affectAsCastIsOver,
            float angleDeg
            ) : base(game, x, y, collisionRadius, owner, target, originSpell, 0, effectName, flags)
        {
            SpellData = _game.Config.ContentManager.GetSpellData(effectName);
            _affectAsCastIsOver = affectAsCastIsOver;
            _angleDeg = angleDeg;
            CreateCone(new Target(x, y), target);            

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
                        if (TargetIsInCone(u))
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

        private void CreateCone(Target beginPoint, Target endPoint)
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

        private bool TargetIsInCone(AttackableUnit target)
        {
            var unitCoords = new Vector2(target.X, target.Y);
            var targetDistance = Vector2.Distance(_ownerCoords, unitCoords);

            float targetAngle = (float)Math.Acos((target.X - _ownerCoords.X) / targetDistance);

            return (targetDistance <= _radius) && (targetAngle >= _beginAngle) && (targetAngle <= _endAngle);
        }

    }
}
