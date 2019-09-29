using System.Collections.Generic;
using GameServerCore.Content;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.Content;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.Buildings.AnimatedBuildings;
using LeagueSandbox.GameServer.GameObjects.Other;

namespace LeagueSandbox.GameServer.GameObjects.Missiles
{
    public class Projectile : ObjMissile, IProjectile
    {
        public List<IGameObject> ObjectsHit { get; }
        public IAttackableUnit Owner { get; }
        public int ProjectileId { get; }
        public ISpellData SpellData { get; protected set; }

        protected float _moveSpeed;
        public ISpell OriginSpell { get; protected set; }
        public bool IsServerOnly { get; }

        public Projectile(
            Game game,
            float x,
            float y,
            int collisionRadius,
            IAttackableUnit owner,
            ITarget target,
            ISpell originSpell,
            float moveSpeed,
            string projectileName,
            int flags = 0,
            uint netId = 0,
            bool serverOnly = false
        ) : base(game, x, y, collisionRadius, 0, netId)
        {
            SpellData = _game.Config.ContentManager.GetSpellData(projectileName);
            OriginSpell = originSpell;
            _moveSpeed = moveSpeed;
            Owner = owner;
            Team = owner.Team;
            ProjectileId = (int)HashFunctions.HashString(projectileName);
            if (!string.IsNullOrEmpty(projectileName))
            {
                VisionRadius = SpellData.MissilePerceptionBubbleRadius;
            }
            ObjectsHit = new List<IGameObject>();

            Target = target;
            IsServerOnly = serverOnly;
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

        public override void OnCollision(IGameObject collider)
        {
            base.OnCollision(collider);
            if (Target != null && Target.IsSimpleTarget && !IsToRemove())
            {
                CheckFlagsForUnit(collider as IAttackableUnit);
            }
            else
            {
                if (Target == collider)
                {
                    CheckFlagsForUnit(collider as IAttackableUnit);
                }
            }
        }

        public override float GetMoveSpeed()
        {
            return _moveSpeed;
        }

        // todo refactor this
        protected virtual void CheckFlagsForUnit(IAttackableUnit unit)
        {
            if (Target == null)
            {
                return;
            }

            if (Target.IsSimpleTarget)
            { // Skillshot
                if (!CheckIfValidTarget(unit))
                    return;

                ObjectsHit.Add(unit);
                var attackableUnit = unit;
                if (attackableUnit != null)
                {
                    OriginSpell.ApplyEffects(attackableUnit, this);
                }
            }
            else
            {
                if (Target is IAttackableUnit u)
                { // Autoguided spell
                    if (OriginSpell != null)
                    {
                        OriginSpell.ApplyEffects(u, this);
                    }
                    else
                    { // auto attack
                        if (Owner is IObjAiBase ai)
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
            base.SetToRemove();
            _game.PacketNotifier.NotifyDestroyClientMissile(this);
        }
        
        protected bool CheckIfValidTarget(IAttackableUnit unit)
        {
            if (!Target.IsSimpleTarget || unit == null || ObjectsHit.Contains(unit))
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

            switch (unit)
            {
                // Order is important
                case ILaneMinion _ when ((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AFFECT_MINIONS) > 0):
                    return true;
                case IMinion _ when ((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AFFECT_USEABLE) > 0):
                    if (!(unit is ILaneMinion))
                        return true;
                    return false; // already got checked in ILaneMinion
                case IBaseTurret _ when ((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AFFECT_TURRETS) > 0):
                    return true;
                case IInhibitor _ when ((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AFFECT_BUILDINGS) > 0):
                    return true;
                case INexus _ when ((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AFFECT_BUILDINGS) > 0):
                    return true;
                case IChampion _ when ((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AFFECT_HEROES) > 0):
                    return true;
                default:
                    return false;
            }
        }
    }
}
