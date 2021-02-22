using System;
using System.Collections.Generic;
using System.Numerics;
using GameServerCore.Content;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using GameServerCore.Enums;

namespace LeagueSandbox.GameServer.GameObjects.Spell.Missile
{
    public class SpellMissile : GameObject, ISpellMissile
    {
        // Function Vars.
        protected float _moveSpeed;
        private bool _atDestination;
        private float _timeSinceCreation;

        public ICastInfo CastInfo { get; protected set; }
        /// <summary>
        /// Number of objects this projectile has hit since it was created.
        /// </summary>
        public List<IGameObject> ObjectsHit { get; }
        /// <summary>
        /// Projectile spell data, housing all information about this projectile's properties. Most projectiles are counted as ExtraSpells within a character's data.
        /// </summary>
        public ISpellData SpellData { get; protected set; }
        /// <summary>
        /// Current unit this projectile is homing in on and moving towards. Projectile is destroyed on contact with this unit unless it has more than one target.
        /// </summary>
        public IAttackableUnit TargetUnit { get; protected set; }
        /// <summary>
        /// Position this projectile is moving towards. Projectile is destroyed once it reaches this destination. Equals Vector2.Zero if TargetUnit is not null.
        /// </summary>
        public Vector2 Destination { get; protected set; }
        /// <summary>
        /// Spell which created this projectile.
        /// </summary>
        public ISpell SpellOrigin { get; protected set; }
        /// <summary>
        /// Whether or not this projectile's visuals should not be networked to clients.
        /// </summary>
        public bool IsServerOnly { get; }

        public SpellMissile(
            Game game,
            int collisionRadius,
            ISpell originSpell,
            ICastInfo castInfo,
            float moveSpeed,
            string projectileName,
            SpellDataFlags flags = 0,
            uint netId = 0,
            bool serverOnly = false
        ) : base(game, castInfo.Owner.Position, collisionRadius, 0, netId)
        {
            _moveSpeed = moveSpeed;
            _atDestination = false;
            _timeSinceCreation = 0.0f;

            SpellOrigin = originSpell;
            SpellData = _game.Config.ContentManager.GetSpellData(projectileName);

            CastInfo = castInfo;
            
            // TODO: Implemented full support for multiple targets.
            if (!castInfo.Targets.Exists(t =>
            {
                if (t.Unit != null)
                {
                    TargetUnit = t.Unit;
                    Destination = Vector2.Zero;
                    return true;
                }
                return false;
            }))
            {
                if (SpellOrigin.SpellData.TargetingType != TargetingType.Location)
                {
                    Position = new Vector2(castInfo.SpellCastLaunchPosition.X, castInfo.SpellCastLaunchPosition.Z);
                }
                else
                {
                    if (CastInfo.IsOverrideCastPosition)
                    {
                        Position = new Vector2(castInfo.SpellCastLaunchPosition.X, castInfo.SpellCastLaunchPosition.Z);
                    }
                    else
                    {
                        Position = new Vector2(castInfo.TargetPosition.X, castInfo.TargetPosition.Z);
                    }
                }
                Destination = new Vector2(castInfo.TargetPositionEnd.X, castInfo.TargetPositionEnd.Z);
            }

            if (!string.IsNullOrEmpty(projectileName))
            {
                VisionRadius = SpellData.MissilePerceptionBubbleRadius;
            }

            ObjectsHit = new List<IGameObject>();

            Team = CastInfo.Owner.Team;

            IsServerOnly = serverOnly;
        }

        public override void Update(float diff)
        {
            if (!HasTarget() || _atDestination)
            {
                SetToRemove();
                return;
            }
            _timeSinceCreation += diff;

            Move(diff);
        }

        public override void OnCollision(IGameObject collider, bool isTerrain = false)
        {
            if (IsToRemove() || (TargetUnit != null && collider != TargetUnit) || (Destination != Vector2.Zero && collider is IObjBuilding))
            {
                return;
            }

            if (isTerrain)
            {
                // TODO: Implement methods for isTerrain for projectiles such as Nautilus Q, ShyvanaDragon Q, or Ziggs Q.
                return;
            }

            if (Destination != Vector2.Zero)
            {
                CheckFlagsForUnit(collider as IAttackableUnit);
            }
        }

        /// <summary>
        /// Gets the server-side speed that this Projectile moves at in units/sec.
        /// </summary>
        /// <returns>Units travelled per second.</returns>
        public float GetSpeed()
        {
            return _moveSpeed;
        }

        /// <summary>
        /// Gets the time since this projectile was created.
        /// </summary>
        /// <returns></returns>
        public float GetTimeSinceCreation()
        {
            return _timeSinceCreation;
        }

        /// <summary>
        /// Moves this projectile to either its target unit, or its destination, and updates its coordinates along the way.
        /// </summary>
        /// <param name="diff">The amount of milliseconds the AI is supposed to move</param>
        private void Move(float diff)
        {
            if (!HasTarget())
            {
                Direction = new Vector3();

                return;
            }

            // current position
            var cur = new Vector2(Position.X, Position.Y);

            var next = GetTargetPosition();

            var goingTo = new Vector3(next.X, _game.Map.NavigationGrid.GetHeightAtLocation(next.X, next.Y), next.Y) - new Vector3(cur.X, _game.Map.NavigationGrid.GetHeightAtLocation(cur.X, cur.Y), cur.Y);
            var dirTemp = Vector3.Normalize(goingTo);

            // usually doesn't happen
            if (float.IsNaN(dirTemp.X) || float.IsNaN(dirTemp.Y) || float.IsNaN(dirTemp.Z))
            {
                dirTemp = new Vector3(0, 0, 0);
            }

            Direction = dirTemp;

            var moveSpeed = GetSpeed();

            var dist = MathF.Abs(Vector2.Distance(cur, next));

            var deltaMovement = moveSpeed * 0.001f * diff;

            // Prevent moving past the next waypoint.
            if (deltaMovement > dist)
            {
                deltaMovement = dist;
            }

            var xx = Direction.X * deltaMovement;
            var yy = Direction.Z * deltaMovement;

            Position = new Vector2(Position.X + xx, Position.Y + yy);

            // (X, Y) has moved to the next position
            cur = new Vector2(Position.X, Position.Y);

            // Check if we reached the target position/destination.
            // REVIEW (of previous code): (deltaMovement * 2) being used here is problematic; if the server lags, the diff will be much greater than the usual values
            if ((cur - next).LengthSquared() < MOVEMENT_EPSILON * MOVEMENT_EPSILON)
            {
                if (this is ISpellMissile && TargetUnit != null)
                {
                    if (Position == TargetUnit.Position)
                    {
                        CheckFlagsForUnit(TargetUnit);
                    }
                    return;
                }

                // remove this projectile because it has reached its destination
                if (Position == Destination)
                {
                    _atDestination = true;
                }
            }
        }

        // TODO: refactor this
        protected virtual void CheckFlagsForUnit(IAttackableUnit unit)
        {
            if (!HasTarget())
            {
                return;
            }

            // TODO: Verify if this works in all cases, if not, then change to: if (TargetUnit == null)
            if (!CastInfo.IsClickCasted)
            {
                // Skillshot
                if (!CheckIfValidTarget(unit))
                {
                    return;
                }

                ObjectsHit.Add(unit);
                var attackableUnit = unit;
                if (SpellOrigin != null)
                {
                    SpellOrigin.ApplyEffects(attackableUnit, this);
                }
            }
            else
            {
                // Targeted Spell (including auto attacks)
                if (SpellOrigin != null)
                {
                    SpellOrigin.ApplyEffects(TargetUnit, this);
                    if (CastInfo.Owner is IObjAiBase ai && SpellOrigin.CastInfo.IsAutoAttack)
                    {
                        ai.AutoAttackHit(TargetUnit);
                    }
                }

                SetToRemove();
            }
        }

        public override void SetToRemove()
        {
            base.SetToRemove();

            if (SpellOrigin != null)
            {
                SpellOrigin.RemoveProjectile(this);
            }

            _game.PacketNotifier.NotifyDestroyClientMissile(this);
        }
        
        protected bool CheckIfValidTarget(IAttackableUnit unit)
        {
            if (TargetUnit != null || unit == null || ObjectsHit.Contains(unit))
            {
                return false;
            }

            if (unit.Team == Owner.Team && !SpellData.Flags.HasFlag(SpellDataFlags.AffectFriends))
            {
                return false;
            }

            if (unit.Team == TeamId.TEAM_NEUTRAL && !SpellData.Flags.HasFlag(SpellDataFlags.AffectNeutral))
            {
                return false;
            }

            if (unit.Team != Owner.Team && unit.Team != TeamId.TEAM_NEUTRAL && !SpellData.Flags.HasFlag(SpellDataFlags.AffectEnemies))
            if (unit.Team != CastInfo.Owner.Team && unit.Team != TeamId.TEAM_NEUTRAL && !((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AFFECT_ENEMIES) > 0))
            {
                return false;
            }

            if (unit.IsDead && !SpellData.Flags.HasFlag(SpellDataFlags.AffectDead))
            {
                return false;
            }

            switch (unit)
            {
                // TODO: Verify all
                // Order is important
                case ILaneMinion _ when SpellData.Flags.HasFlag(SpellDataFlags.AffectMinions)
                                    && !SpellData.Flags.HasFlag(SpellDataFlags.IgnoreLaneMinion):
                    return true;
                case IMinion m when (!m.IsPet && SpellData.Flags.HasFlag(SpellDataFlags.AffectNotPet))
                                || (m.IsPet && SpellData.Flags.HasFlag(SpellDataFlags.AffectUseable))
                                || (m.IsWard && SpellData.Flags.HasFlag(SpellDataFlags.AffectWards))
                                || (!m.IsClone && SpellData.Flags.HasFlag(SpellDataFlags.IgnoreClones))
                                || (unit.Team == Owner.Team && !SpellData.Flags.HasFlag(SpellDataFlags.IgnoreAllyMinion))
                                || (unit.Team != Owner.Team && unit.Team != TeamId.TEAM_NEUTRAL && !SpellData.Flags.HasFlag(SpellDataFlags.IgnoreEnemyMinion))
                                || SpellData.Flags.HasFlag(SpellDataFlags.AffectMinions):
                    if (!(unit is ILaneMinion))
                    {
                        return true;
                    }
                    return false; // already got checked in ILaneMinion
                case IBaseTurret _ when SpellData.Flags.HasFlag(SpellDataFlags.AffectTurrets):
                    return true;
                case IInhibitor _ when SpellData.Flags.HasFlag(SpellDataFlags.AffectBuildings):
                    return true;
                case INexus _ when SpellData.Flags.HasFlag(SpellDataFlags.AffectBuildings):
                    return true;
                case IChampion _ when SpellData.Flags.HasFlag(SpellDataFlags.AffectHeroes):
                    return true;
                default:
                    if (SpellData.Flags.HasFlag(SpellDataFlags.AffectAllUnitTypes))
                    {
                        return true;
                    }
                    return false;
            }
        }

        /// <summary>
        /// Whether or not this projectile has a target unit or a destination; if it is a valid projectile.
        /// </summary>
        /// <returns>True/False.</returns>
        public bool HasTarget()
        {
            return TargetUnit != null || Destination != Vector2.Zero;
        }

        /// <summary>
        /// Gets the position of this projectile's target (unit or destination).
        /// </summary>
        /// <returns>Vector2 position of target. Vector2(float.NaN, float.NaN) if projectile has no target.</returns>
        public Vector2 GetTargetPosition()
        { 
            if (!HasTarget())
            {
                return new Vector2(float.NaN, float.NaN);
            }

            if (TargetUnit != null)
            {
                return TargetUnit.Position;
            }

            return Destination;
        }
    }
}
