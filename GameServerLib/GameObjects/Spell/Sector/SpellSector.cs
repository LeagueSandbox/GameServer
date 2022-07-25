using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Missile;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace LeagueSandbox.GameServer.GameObjects.SpellNS.Sector
{
    /// <summary>
    /// Base class for all spell sectors. Functionally acts as a circular spell hitbox.
    /// Base functionality can be overriden to fit a specific shape.
    /// </summary>
    public class SpellSector : GameObject
    {
        // Function Vars.
        private float _timeSinceCreation;
        private float _lastTickTime;
        private List<AttackableUnit> _unitsToHit;

        /// <summary>
        /// Information about the creation of this sector.
        /// </summary>
        public CastInfo CastInfo { get; protected set; }
        /// <summary>
        /// Spell which created this projectile.
        /// </summary>
        public Spell SpellOrigin { get; protected set; }
        /// <summary>
        /// Parameters for this sector, refer to SectorParameters.
        /// </summary>
        public SectorParameters Parameters { get; protected set; }
        /// <summary>
        /// All objects this sector has hit since it was created and how many times each has been hit.
        /// </summary>
        public List<GameObject> ObjectsHit { get; }
        /// <summary>
        /// Total number of times this sector has hit any units.
        /// </summary>
        /// TODO: Verify if we want this to be an array for different MaximumHit counts for: CanHitCaster, CanHitEnemies, CanHitFriends, CanHitSameTarget, and CanHitSameTargetConsecutively.
        public int HitCount { get; protected set; }

        public SpellSector(
            Game game,
            SectorParameters parameters,
            Spell originSpell,
            CastInfo castInfo,
            uint netId = 0
        ) : base(game, new Vector2(castInfo.TargetPositionEnd.X, castInfo.TargetPositionEnd.Z), Math.Max(parameters.Length, parameters.Width), 0, 0, netId)
        {
            _timeSinceCreation = 0.0f;
            _lastTickTime = 0.0f;
            _unitsToHit = new List<AttackableUnit>();

            Parameters = parameters;
            CastInfo = castInfo;
            SpellOrigin = originSpell;
            ObjectsHit = new List<GameObject>();
            HitCount = 0;

            VisionRadius = SpellOrigin.SpellData.MissilePerceptionBubbleRadius;

            Team = CastInfo.Owner.Team;
        }

        public override void OnAdded()
        {
            base.OnAdded();

            // Update same tick of creation.
            // This prevents cases where single tick sectors change position before executing.
            _game.Map.CollisionHandler.UpdateCollision(this);
            Update(0f);
        }

        public override void Update(float diff)
        {
            if (IsToRemove())
            {
                return;
            }

            _lastTickTime += diff;

            if (Parameters.Lifetime >= 0 && (Parameters.Lifetime * 1000.0f) <= _timeSinceCreation)
            {
                SetToRemove();
                return;
            }

            if (Parameters.BindObject != null)
            {
                Move(diff);
            }

            if (_lastTickTime >= (1000.0f / Parameters.Tickrate) || (Parameters.SingleTick && Parameters.Tickrate <= 0))
            {
                _lastTickTime = 0;

                ExecuteTick();
            }

            _timeSinceCreation += diff;
        }

        public override void OnCollision(GameObject collider, bool isTerrain = false)
        {
            // TODO: Verify if we want to support acting differently depending on if it is terrain (if so, simply add it to FilterCollisions as a parameter and remove from here).
            if (IsToRemove() || isTerrain || collider is SpellSector || collider is SpellMissile)
            {
                return;
            }

            // If we CanHitSameTarget, we must remove any units hit that are outside the check area, so we can hit them again if they re-enter.
            // TODO: Should this be affected by tickrate? It may not be intuitive gameplay-wise.
            if (Parameters.CanHitSameTarget && !Parameters.CanHitSameTargetConsecutively)
            {
                for (int i = ObjectsHit.Count - 1; i >= 0; i--)
                {
                    if (!ObjectsHit[i].IsCollidingWith(this))
                    {
                        ObjectsHit.RemoveAt(i);
                    }
                }
            }

            // OnCollision has already checked the affectRadius around the sector, so now we filter the area.
            if (collider is AttackableUnit unit)
            {
                if (IsValidTarget(unit))
                {
                    _unitsToHit.Add(unit);
                }
            }
        }

        /// <summary>
        /// Filter function which checks if the given collider is within the bounds of a hitbox.
        /// </summary>
        /// <param name="collider">Object to check.</param>
        /// <returns>True/False.</returns>
        protected virtual bool FilterCollisions(GameObject collider)
        {
            // Empty, as the base functionality of SpellSector is an area, which is already accounted for by OnCollision.
            return true;
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
        public virtual void Move(float diff)
        {
            // current position
            var cur = Position;
            var next = GetTargetPosition();

            // If we are not bound to an object
            if (next == cur)
            {
                return;
            }

            // TODO: Implement interpolation of the sector hitbox

            // Simply teleport to the bind object
            SetPosition(next);
        }

        public virtual void HitUnit(AttackableUnit unit)
        {
            if (SpellOrigin != null)
            {
                SpellOrigin.ApplyEffects(unit, null, this);
            }

            if (CastInfo.Owner is ObjAIBase ai && SpellOrigin.CastInfo.IsAutoAttack)
            {
                ai.AutoAttackHit(unit);
            }

            ObjectsHit.Add(unit);

            if (Parameters.MaximumHits > 0 && ObjectsHit.Count >= Parameters.MaximumHits)
            {
                SetToRemove();
            }
        }

        protected bool IsValidTarget(AttackableUnit unit)
        {
            bool valid = SpellOrigin.SpellData.IsValidTarget(CastInfo.Owner, unit, Parameters.OverrideFlags);
            bool hit = ObjectsHit.Contains(unit);

            if (hit)
            {
                // We can't hit this unit because we've hit it already.
                valid = false;

                // We can consecutively hit this same unit until we reach MaximumHits.
                if (Parameters.CanHitSameTargetConsecutively)
                {
                    valid = true;
                }
            }

            // Otherwise, we can hit this unit because we haven't hit it yet.
            return valid && !_unitsToHit.Contains(unit);
        }

        /// <summary>
        /// Forces this spell sector to perform a tick.
        /// </summary>
        public void ExecuteTick()
        {
            // OnCollision has been called within a single Update, so now we just hit any units within the filtered area of the collision radius.
            if (_unitsToHit.Count > 0)
            {
                _unitsToHit = _unitsToHit.OrderByDescending(unit => Vector2.DistanceSquared(this.Position, unit.Position)).ToList();
                
                for (int i = _unitsToHit.Count - 1; i >= 0; i--)
                {
                    var unit = _unitsToHit[i];
                    if (unit.IsCollidingWith(this) && FilterCollisions(unit))
                    {
                        HitUnit(_unitsToHit[i]);
                    }

                    _unitsToHit.RemoveAt(i);
                }
            }

            if (Parameters.SingleTick)
            {
                SetToRemove();
            }
        }

        /// <summary>
        /// Gets the position of this projectile's target (unit or destination).
        /// </summary>
        /// <returns>Vector2 position of target. Vector2(float.NaN, float.NaN) if projectile has no target.</returns>
        public virtual Vector2 GetTargetPosition()
        {
            if (Parameters.BindObject != null)
            {
                return Parameters.BindObject.Position;
            }

            return Position;
        }
    }
}
