﻿using System.Collections.Generic;
using System.Numerics;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Core.Logic;
using Newtonsoft.Json.Linq;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class Projectile : ObjMissile
    {
        public List<GameObject> ObjectsHit { get; private set; }
        public AttackableUnit Owner { get; private set; }
        public int ProjectileId { get; private set; }
        public SpellData SpellData { get; private set; }
        public float ProjectileSpeed { get; protected set; }
        protected Spell _originSpell;
        private Logger _logger = Program.ResolveDependency<Logger>();
        
        public Projectile(
            float x,
            float y,
            int collisionRadius,
            AttackableUnit owner,
            Target target,
            Spell originSpell,
            float projectileSpeed,
            string projectileName,
            int flags = 0,
            uint netId = 0
        ) : base(x, y, collisionRadius, 0, netId)
        {
            SpellData = _game.Config.ContentManager.GetSpellData(projectileName);
            _originSpell = originSpell;
            ProjectileSpeed = projectileSpeed;
            Owner = owner;
            Team = owner.Team;
            ProjectileId = (int)HashFunctions.HashString(projectileName);
            if (!string.IsNullOrEmpty(projectileName))
            {
                VisionRadius = SpellData.MissilePerceptionBubbleRadius;
            }
            ObjectsHit = new List<GameObject>();

            Target = target;

            if (!target.IsSimpleTarget)
            {
                ((GameObject)target).incrementAttackerCount();
            }

            owner.incrementAttackerCount();
        }

        public override void update(float diff)
        {
            if (Target == null)
            {
                setToRemove();
                return;
            }

            base.update(diff);
        }

        public override void onCollision(GameObject collider)
        {
            base.onCollision(collider);
            if (Target != null && Target.IsSimpleTarget && !isToRemove())
            {
                CheckFlagsForUnit(collider as AttackableUnit);
            }
            else
            {
                if (Target == collider)
                {
                    CheckFlagsForUnit(collider as AttackableUnit);
                }
            }
        }

        public override void Move(float diff)
        {
            if (Target == null)
            {
                return;
            }

            var to = new Vector2(Target.X, Target.Y);
            var cur = new Vector2(X, Y);

            var goingTo = to - cur;
            _direction = Vector2.Normalize(goingTo);
            if (float.IsNaN(_direction.X) || float.IsNaN(_direction.Y))
            {
                _direction = new Vector2(0, 0);
            }

            var deltaMovement = ProjectileSpeed * 0.001f * diff;

            var xx = _direction.X * deltaMovement;
            var yy = _direction.Y * deltaMovement;

            X += xx;
            Y += yy;
        }

        protected virtual void CheckFlagsForUnit(AttackableUnit unit)
        {
            if (Target == null)
            {
                return;
            }

            if (Target.IsSimpleTarget)
            { // Skillshot
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
                var attackableUnit = unit as AttackableUnit;
                if (attackableUnit != null)
                {
                    _originSpell.applyEffects(attackableUnit, this);
                }
            }
            else
            {
                var u = Target as AttackableUnit;
                if (u != null)
                { // Autoguided spell
                    if (_originSpell != null)
                    {
                        _originSpell.applyEffects(u, this);
                    }
                    else
                    { // auto attack
                        if (Owner is ObjAIBase ai)
                        {
                            ai.AutoAttackHit(u);
                        }

                        setToRemove();
                    }
                }
            }
        }

        public override void setToRemove()
        {
            if (Target != null && !Target.IsSimpleTarget)
                (Target as GameObject).decrementAttackerCount();

            Owner.decrementAttackerCount();
            base.setToRemove();
            _game.PacketNotifier.NotifyProjectileDestroy(this);
        }
    }
}
