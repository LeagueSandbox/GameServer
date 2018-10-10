using System.Collections.Generic;
using System.Linq;
using GameServerCore.Content;
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
    public class Projectile : ObjMissile, IProjectile
    {
        public List<GameObject> ObjectsHit { get; private set; }
        public AttackableUnit Owner { get; private set; }
        public int ProjectileId { get; private set; }
        public SpellData SpellData { get; protected set; }

        List<IGameObject> IProjectile.ObjectsHit => ObjectsHit.Cast<IGameObject>().ToList();
        IAttackableUnit IProjectile.Owner => Owner;

        protected float _moveSpeed;
        protected Spell _originSpell;

        public Projectile(
            Game game,
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
        ) : base(game, x, y, collisionRadius, 0, netId)
        {
            SpellData = _game.Config.ContentManager.GetSpellData(owner.Model, projectileName);
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
                    && !((SpellData.Flags & (int)SpellFlag.AFFECT_FRIENDS) > 0))
                {
                    return;
                }

                if (unit.Team == TeamId.TEAM_NEUTRAL
                    && !((SpellData.Flags & (int)SpellFlag.AFFECT_NEUTRAL) > 0))
                {
                    return;
                }

                if (unit.Team != Owner.Team
                    && unit.Team != TeamId.TEAM_NEUTRAL
                    && !((SpellData.Flags & (int)SpellFlag.AFFECT_ENEMIES) > 0))
                {
                    return;
                }


                if (unit.IsDead && !((SpellData.Flags & (int)SpellFlag.AFFECT_DEAD) > 0))
                {
                    return;
                }

                if (unit is Minion m && !((SpellData.Flags & (int)SpellFlag.AFFECT_MINIONS) > 0))
                {
                    return;
                }

                if (unit is Placeable p && !((SpellData.Flags & (int)SpellFlag.AFFECT_USEABLE) > 0))
                {
                    return;
                }

                if (unit is BaseTurret t && !((SpellData.Flags & (int)SpellFlag.AFFECT_TURRETS) > 0))
                {
                    return;
                }

                if ((unit is Inhibitor i || unit is Nexus n) && !((SpellData.Flags & (int)SpellFlag.AFFECT_BUILDINGS) > 0))
                {
                    return;
                }

                if (unit is Champion c && !((SpellData.Flags & (int)SpellFlag.AFFECT_HEROES) > 0))
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
                if (Target is AttackableUnit u)
                { // Autoguided spell
                    if (_originSpell != null)
                    {
                        _originSpell.ApplyEffects(u, this);
                    }
                    else
                    { // auto attack
                        if (Owner is ObjAiBase ai)
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
                (Target as GameObject)?.DecrementAttackerCount();
            }

            Owner.DecrementAttackerCount();
            base.SetToRemove();
            _game.PacketNotifier.NotifyProjectileDestroy(this);
        }
    }
}
