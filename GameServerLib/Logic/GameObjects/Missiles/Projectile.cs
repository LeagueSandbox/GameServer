﻿using System.Collections.Generic;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.Buildings.AnimatedBuildings;
using LeagueSandbox.GameServer.Logic.GameObjects.Other;
using LeagueSandbox.GameServer.Logic.GameObjects.Spells;

namespace LeagueSandbox.GameServer.Logic.GameObjects.Missiles
{
    public class Projectile : ObjMissile
    {
        public List<GameObject> ObjectsHit { get; private set; }
        public AttackableUnit Owner { get; private set; }
        public int ProjectileId { get; private set; }
        public SpellData SpellData { get; private set; }
        protected float _moveSpeed;
        protected Spell _originSpell;

        public Projectile(
            float x,
            float y,
            int collisionRadius,
            AttackableUnit owner,
            Target target,
            Spell originSpell,
            float moveSpeed,
            string projectileName,
            int flags = 0,
            uint netId = 0
        ) : base(x, y, collisionRadius, 0, netId)
        {
            SpellData = Game.Config.ContentManager.GetSpellData(projectileName);
            _originSpell = originSpell;
            _moveSpeed = moveSpeed;
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
                ((GameObject)target).IncrementAttackerCount();
            }

            owner.IncrementAttackerCount();
        }

        public override void Update(float diff)
        {
            if (Target == null)
            {
                SetToRemove();
                return;
            }

            base.Update(diff);
        }

        public override void OnCollision(GameObject collider)
        {
            base.OnCollision(collider);
            if (Target != null && Target.IsSimpleTarget && !IsToRemove())
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

        public override float GetMoveSpeed()
        {
            return _moveSpeed;
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
                {
                    return;
                }

                if (unit.Team == Owner.Team
                    && !((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AFFECT_FRIENDS) > 0))
                {
                    return;
                }

                if (unit.Team == TeamId.TEAM_NEUTRAL
                    && !((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AFFECT_NEUTRAL) > 0))
                {
                    return;
                }

                if (unit.Team != Owner.Team
                    && unit.Team != TeamId.TEAM_NEUTRAL
                    && !((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AFFECT_ENEMIES) > 0))
                {
                    return;
                }


                if (unit.IsDead && !((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AFFECT_DEAD) > 0))
                {
                    return;
                }

                var m = unit as Minion;
                if (m != null && !((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AFFECT_MINIONS) > 0))
                {
                    return;
                }

                var p = unit as Placeable;
                if (p != null && !((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AFFECT_USEABLE) > 0))
                {
                    return;
                }

                var t = unit as BaseTurret;
                if (t != null && !((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AFFECT_TURRETS) > 0))
                {
                    return;
                }

                var i = unit as Inhibitor;
                var n = unit as Nexus;
                if ((i != null || n != null) && !((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AFFECT_BUILDINGS) > 0))
                {
                    return;
                }

                var c = unit as Champion;
                if (c != null && !((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AFFECT_HEROES) > 0))
                {
                    return;
                }

                ObjectsHit.Add(unit);
                var attackableUnit = unit;
                if (attackableUnit != null)
                {
                    _originSpell.ApplyEffects(attackableUnit, this);
                }
            }
            else
            {
                var u = Target as AttackableUnit;
                if (u != null)
                { // Autoguided spell
                    if (_originSpell != null)
                    {
                        _originSpell.ApplyEffects(u, this);
                    }
                    else
                    { // auto attack
                        var ai = Owner as ObjAiBase;
                        if (ai != null)
                        {
                            ai.AutoAttackHit(u);
                        }
                        SetToRemove();
                    }
                }
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
            Game.PacketNotifier.NotifyProjectileDestroy(this);
        }
    }
}
