using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using GameMaths.Geometry.Polygons;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.Content;
using LeagueSandbox.GameServer.GameObjects.Missiles;
using LeagueSandbox.GameServer.Items;

namespace LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI
{
    /// <summary>
    /// Base class for all moving, attackable, and attacking units.
    /// ObjAIBases normally follow these guidelines of functionality: Self movement, Inventory, Targeting, Attacking, and Spells.
    /// </summary>
    public class ObjAiBase : AttackableUnit, IObjAiBase
    {
        // Crucial Vars
        private float _autoAttackCurrentCooldown;
        private float _autoAttackCurrentDelay;
        private uint _autoAttackProjId;
        private bool _isNextAutoCrit;
        protected ItemManager _itemManager;
        private bool _nextAttackFlag;
        private Random _random = new Random();

        /// <summary>
        /// Variable storing all the data related to this AI's current auto attack. *NOTE*: Will be deprecated as the spells system gets finished.
        /// </summary>
        public ISpellData AaSpellData { get; private set; }
        /// <summary>
        /// Variable for the cast time of this AI's current auto attack.
        /// </summary>
        public float AutoAttackCastTime { get; set; }
        /// <summary>
        /// Variable for the projectile speed of this AI's current auto attack projectile.
        /// </summary>
        public float AutoAttackProjectileSpeed { get; set; }
        /// <summary>
        /// This AI's current auto attack target. Null if no target.
        /// </summary>
        public IAttackableUnit AutoAttackTarget { get; set; }
        /// <summary>
        /// Variable containing all data about the AI's current character such as base health, base mana, whether or not they are melee, base movespeed, per level stats, etc.
        /// </summary>
        /// TODO: Move to AttackableUnit as it relates to stats.
        public ICharData CharData { get; }
        /// <summary>
        /// Whether or not this AI has made their first auto attack against their current target. Refreshes after untargeting or targeting another unit.
        /// </summary>
        public bool HasMadeInitialAttack { get; set; }
        /// <summary>
        /// Variable housing all variables and functions related to this AI's Inventory, ex: Items.
        /// </summary>
        /// TODO: Verify if we want to move this to AttackableUnit since items are related to stats.
        public IInventoryManager Inventory { get; protected set; }
        /// <summary>
        /// Whether or not this AI is currently auto attacking.
        /// </summary>
        public bool IsAttacking { get; set; }
        /// <summary>
        /// Whether or not this AI is currently casting a spell. *NOTE*: Not to be confused with channeling (which isn't implemented yet).
        /// </summary>
        public bool IsCastingSpell { get; set; }
        /// <summary>
        /// Whether or not this AI's auto attacks apply damage to their target immediately after their cast time ends.
        /// </summary>
        public bool IsMelee { get; set; }
        /// <summary>
        /// Current order this AI is performing. *NOTE*: Does not contain all possible values.
        /// </summary>
        /// TODO: Rework AI so this enum can be finished.
        public MoveOrder MoveOrder { get; set; }
        /// <summary>
        /// Unit this AI will auto attack when it is in auto attack range.
        /// </summary>
        public IAttackableUnit TargetUnit { get; set; }

        public ObjAiBase(Game game, string model, Stats.Stats stats, int collisionRadius = 40,
            Vector2 position = new Vector2(), int visionRadius = 0, uint netId = 0, TeamId team = TeamId.TEAM_NEUTRAL) :
            base(game, model, stats, collisionRadius, position, visionRadius, netId, team)
        {
            _itemManager = game.ItemManager;

            CharData = _game.Config.ContentManager.GetCharData(Model);

            stats.LoadStats(CharData);

            // TODO: Centralize this instead of letting it lay in the initialization.
            if (CharData.PathfindingCollisionRadius > 0)
            {
                CollisionRadius = CharData.PathfindingCollisionRadius;
            }
            else if (collisionRadius > 0)
            {
                CollisionRadius = collisionRadius;
            }
            else
            {
                CollisionRadius = 40;
            }

            // TODO: Centralize this instead of letting it lay in the initialization.
            if (CharData.PerceptionBubbleRadius > 0)
            {
                VisionRadius = CharData.PerceptionBubbleRadius;
            }
            else if (collisionRadius > 0)
            {
                VisionRadius = visionRadius;
            }
            else
            {
                VisionRadius = 1100;
            }

            Stats.CurrentMana = stats.ManaPoints.Total;
            Stats.CurrentHealth = stats.HealthPoints.Total;

            if (!string.IsNullOrEmpty(model))
            {
                AaSpellData = _game.Config.ContentManager.GetSpellData(model + "BasicAttack");
                float baseAttackCooldown = 1.6f * (1.0f + CharData.AttackDelayOffsetPercent);
                AutoAttackCastTime = baseAttackCooldown * (0.3f + CharData.AttackDelayCastOffsetPercent);
                AutoAttackProjectileSpeed = AaSpellData.MissileSpeed;
                IsMelee = CharData.IsMelee;
            }
            else
            {
                AutoAttackCastTime = 0;
                AutoAttackProjectileSpeed = 500;
                IsMelee = true;
            }
        }

        public override bool CanMove()
        {
            // False if any are true.
            return !(IsDead || IsCastingSpell || IsDashing
                    // TODO: Remove these and implement them as buffs, then just check the BuffType here.
                    || HasCrowdControl(CrowdControlType.AIRBORNE)
                    || HasCrowdControl(CrowdControlType.ROOT)
                    || HasCrowdControl(CrowdControlType.STASIS)
                    || HasCrowdControl(CrowdControlType.STUN)
                    || HasCrowdControl(CrowdControlType.SNARE));
        }

        /// <summary>
        /// Function called by this AI's auto attack projectile when it hits its target.
        /// </summary>
        public virtual void AutoAttackHit(IAttackableUnit target)
        {
            if (HasCrowdControl(CrowdControlType.BLIND))
            {
                target.TakeDamage(this, 0, DamageType.DAMAGE_TYPE_PHYSICAL,
                                             DamageSource.DAMAGE_SOURCE_ATTACK,
                                             DamageResultType.RESULT_MISS);
                return;
            }

            var damage = Stats.AttackDamage.Total;
            if (_isNextAutoCrit)
            {
                damage *= Stats.CriticalDamage.Total;
            }

            var onAutoAttack = _game.ScriptEngine.GetStaticMethod<Action<IAttackableUnit, IAttackableUnit>>(Model, "Passive", "OnAutoAttack");
            onAutoAttack?.Invoke(this, target);

            target.TakeDamage(this, damage, DamageType.DAMAGE_TYPE_PHYSICAL,
                DamageSource.DAMAGE_SOURCE_ATTACK,
                _isNextAutoCrit);
        }

        /// <summary>
        /// Sets this AI's current auto attack to the given auto attack. *NOTE*: Will be deprecated when spells are fully implemented.
        /// </summary>
        /// <param name="newAutoAttackSpellData">Auto attack spell data to use.</param>
        public void ChangeAutoAttackSpellData(ISpellData newAutoAttackSpellData)
        {
            AaSpellData = newAutoAttackSpellData;
        }

        /// <summary>
        /// Sets this AI's current auto attack to the given auto attack. *NOTE*: Will be deprecated when spells are fully implemented.
        /// </summary>
        /// <param name="newAutoAttackSpellDataName">Name of the auto attack to use.</param>
        public void ChangeAutoAttackSpellData(string newAutoAttackSpellDataName)
        {
            AaSpellData = _game.Config.ContentManager.GetSpellData(newAutoAttackSpellDataName);
        }

        /// <summary>
        /// Classifies the given unit. Used for AI attack priority, such as turrets or minions. Known in League internally as "Call for help".
        /// </summary>
        /// <param name="target">Unit to classify.</param>
        /// <returns>Classification for the given unit.</returns>
        /// TODO: Verify if we want to rename this to something which relates more to the internal League name "Call for Help".
        /// TODO: Move to AttackableUnit.
        public ClassifyUnit ClassifyTarget(IAttackableUnit target)
        {
            if (target is IObjAiBase ai && ai.TargetUnit != null && ai.TargetUnit.IsInDistress()) // If an ally is in distress, target this unit. (Priority 1~5)
            {
                switch (target)
                {
                    // Champion attacking an allied champion
                    case IChampion _ when ai.TargetUnit is IChampion:
                        return ClassifyUnit.CHAMPION_ATTACKING_CHAMPION;
                    // Champion attacking lane minion
                    case IChampion _ when ai.TargetUnit is ILaneMinion:
                        return ClassifyUnit.CHAMPION_ATTACKING_MINION;
                    // Champion attacking minion
                    case IChampion _ when ai.TargetUnit is IMinion:
                        return ClassifyUnit.CHAMPION_ATTACKING_MINION;
                    // Minion attacking an allied champion.
                    case IMinion _ when ai.TargetUnit is IChampion:
                        return ClassifyUnit.MINION_ATTACKING_CHAMPION;
                    // Minion attacking lane minion
                    case IMinion _ when ai.TargetUnit is ILaneMinion:
                        return ClassifyUnit.MINION_ATTACKING_MINION;
                    // Minion attacking minion
                    case IMinion _ when ai.TargetUnit is IMinion:
                        return ClassifyUnit.MINION_ATTACKING_MINION;
                    // Turret attacking lane minion
                    case IBaseTurret _ when ai.TargetUnit is ILaneMinion:
                        return ClassifyUnit.TURRET_ATTACKING_MINION;
                    // Turret attacking minion
                    case IBaseTurret _ when ai.TargetUnit is IMinion:
                        return ClassifyUnit.TURRET_ATTACKING_MINION;
                }
            }

            switch (target)
            {
                case IMinion m:
                    if (m.IsLaneMinion)
                    {
                        switch ((m as ILaneMinion).MinionSpawnType)
                        {
                            case MinionSpawnType.MINION_TYPE_MELEE:
                                return ClassifyUnit.MELEE_MINION;
                            case MinionSpawnType.MINION_TYPE_CASTER:
                                return ClassifyUnit.CASTER_MINION;
                            case MinionSpawnType.MINION_TYPE_CANNON:
                            case MinionSpawnType.MINION_TYPE_SUPER:
                                return ClassifyUnit.SUPER_OR_CANNON_MINION;
                        }
                    }
                    return ClassifyUnit.MINION;
                case IBaseTurret _:
                    return ClassifyUnit.TURRET;
                case IChampion _:
                    return ClassifyUnit.CHAMPION;
                case IInhibitor _ when !target.IsDead:
                    return ClassifyUnit.INHIBITOR;
                case INexus _:
                    return ClassifyUnit.NEXUS;
            }

            return ClassifyUnit.DEFAULT;
        }

        /// <summary>
        /// Called when this AI collides with the terrain or with another GameObject. Refer to CollisionHandler for exact cases.
        /// </summary>
        /// <param name="collider">GameObject that collided with this AI. Null if terrain.</param>
        /// <param name="isTerrain">Whether or not this AI collided with terrain.</param>
        public override void OnCollision(IGameObject collider, bool isTerrain = false)
        {
            if (!isTerrain)
            {
                // Champions do not teleport out of lower level GameObjects.
                // TODO: Implement Collision Priority in CollisionHandler?
                if (!(collider is IChampion || collider is IBaseTurret))
                {
                    return;
                }
            }

            base.OnCollision(collider, isTerrain);

            // If we were trying to path somewhere before colliding, then repath from our new position.
            if (!IsPathEnded())
            {
                // TODO: When using this safePath, sometimes we collide with the terrain again, so we use an unsafe path the next collision, however,
                // sometimes we collide again before we can finish the unsafe path, so we end up looping collisions between safe and unsafe paths, never actually escaping (ex: sharp corners).
                // Edit the current method to fix the above problem.
                List<Vector2> safePath = _game.Map.NavigationGrid.GetPath(Position, _game.Map.NavigationGrid.GetClosestTerrainExit(Waypoints.Last()));
                if (safePath != null)
                {
                    SetWaypoints(safePath);
                }
            }
        }

        /// <summary>
        /// Forces this AI unit to perform a dash which follows the specified AttackableUnit.
        /// </summary>
        /// <param name="target">Unit to follow.</param>
        /// <param name="dashSpeed">Constant speed that the unit will have during the dash.</param>
        /// <param name="animation">Internal name of the dash animation.</param>
        /// <param name="leapGravity">How much gravity the unit will experience when above the ground while dashing.</param>
        /// <param name="keepFacingLastDirection">Whether or not the unit should maintain the direction they were facing before dashing.</param>
        /// <param name="followTargetMaxDistance">Maximum distance the unit will follow the Target before stopping the dash or reaching to the Target.</param>
        /// <param name="backDistance">Unknown parameter.</param>
        /// <param name="travelTime">Total time the dash will follow the GameObject before stopping or reaching the Target.</param>
        /// TODO: Implement Dash class which houses these parameters, then have that as the only parameter to this function (and other Dash-based functions).
        public void DashToTarget
        (
            IAttackableUnit target,
            float dashSpeed,
            string animation,
            float leapGravity,
            bool keepFacingLastDirection,
            float followTargetMaxDistance,
            float backDistance,
            float travelTime
        )
        {
            // TODO: Take into account the rest of the arguments
            IsDashing = true;
            DashTime = Vector2.Distance(target.Position, Position) / (dashSpeed * 0.001f);
            DashElapsedTime = 0;
            DashSpeed = dashSpeed;

            SetWaypoints(new List<Vector2> { Position, target.Position }, false);
            SetTargetUnit(target);

            _game.PacketNotifier.NotifyWaypointGroupWithSpeed
            (
                this,
                dashSpeed,
                leapGravity,
                keepFacingLastDirection,
                target,
                followTargetMaxDistance,
                backDistance,
                travelTime
            );

            if (animation == null)
            {
                animation = "RUN";
            }

            var animList = new List<string> { "RUN", animation };
            _game.PacketNotifier.NotifySetAnimation(this, animList);
        }

        /// <summary>
        /// Automatically paths this AI to a favorable auto attacking position. 
        /// Used only for Minions currently.
        /// </summary>
        /// <returns></returns>
        /// TODO: Move this to Minion? It isn't used anywhere else.
        /// TODO: Re-implement this for LaneMinions and add a patience or distance threshold so they don't follow forever.
        public bool RecalculateAttackPosition()
        {
            // If we are already where we should be, which means we are in attack range and not colliding, then keep our current position.
            if (TargetUnit != null && !TargetUnit.IsDead && Vector2.DistanceSquared(Position, TargetUnit.Position) > CollisionRadius * CollisionRadius
                && Vector2.DistanceSquared(Position, TargetUnit.Position) <= Stats.Range.Total * Stats.Range.Total)
            {
                return false;
            }
            var objects = _game.ObjectManager.GetObjects();
            List<CirclePoly> usedPositions = new List<CirclePoly>();
            var isCurrentlyOverlapping = false;

            var thisCollisionCircle = new CirclePoly(TargetUnit?.Position ?? Position, CollisionRadius + 10);

            foreach (var gameObject in objects)
            {
                var unit = gameObject.Value as IAttackableUnit;
                if (unit == null ||
                    unit.NetId == NetId ||
                    unit.IsDead ||
                    unit.Team != Team ||
                    Vector2.DistanceSquared(Position, TargetUnit.Position) > DETECT_RANGE * DETECT_RANGE
                )
                {
                    continue;
                }
                var targetCollisionCircle = new CirclePoly(unit.Position, unit.CollisionRadius + 10);
                if (targetCollisionCircle.CheckForOverLaps(thisCollisionCircle))
                {
                    isCurrentlyOverlapping = true;
                }
                usedPositions.Add(targetCollisionCircle);
            }
            if (isCurrentlyOverlapping)
            {
                // TODO: Optimize this, preferably without things like CirclePoly.
                var targetCircle = new CirclePoly(TargetUnit.Position, Stats.Range.Total, 72);
                //Find optimal position...
                foreach (var point in targetCircle.Points.OrderBy(x => Vector2.DistanceSquared(Position, x)))
                {
                    if (!_game.Map.NavigationGrid.IsVisible(point))
                    {
                        continue;
                    }
                    var positionUsed = false;
                    foreach (var circlePoly in usedPositions)
                    {
                        if (circlePoly.CheckForOverLaps(new CirclePoly(point, CollisionRadius + 10, 20)))
                        {
                            positionUsed = true;
                        }
                    }

                    if (positionUsed)
                    {
                        continue;
                    }
                    SetWaypoints(new List<Vector2> { Position, point });
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Function which refreshes this AI's waypoints if they have a target.
        /// </summary>
        public virtual void RefreshWaypoints()
        {
            if (TargetUnit == null || TargetUnit.IsDead || (Vector2.DistanceSquared(Position, TargetUnit.Position) <= Stats.Range.Total * Stats.Range.Total && Waypoints.Count == 1))
            {
                return;
            }

            // If the target is already in range, stay where we are.
            if (!IsDashing && Vector2.DistanceSquared(Position, TargetUnit.Position) <= (Stats.Range.Total - 2f) * (Stats.Range.Total - 2f))
            {
                StopMovement();
            }
            // Stop dashing to target if we reached them.
            // TODO: Implement events so we can centralize things like this.
            else if (IsDashing && IsCollidingWith(TargetUnit))
            {
                SetDashingState(false);
            }

            // Otherwise, move to the target.
            else
            {
                Vector2 targetPos = TargetUnit.Position;
                if (!_game.Map.NavigationGrid.IsWalkable(targetPos, TargetUnit.CollisionRadius))
                {
                    targetPos = _game.Map.NavigationGrid.GetClosestTerrainExit(targetPos, CollisionRadius);
                }

                var newWaypoints = _game.Map.NavigationGrid.GetPath(Position, targetPos);
                if (newWaypoints.Count > 1)
                {
                    SetWaypoints(newWaypoints);
                }
            }
        }

        /// <summary>
        /// Sets this AI's current auto attack to their base auto attack.
        /// *NOTE*: Will be depricated when spell systems are fully implemented.
        /// </summary>
        public void ResetAutoAttackSpellData()
        {
            AaSpellData = _game.Config.ContentManager.GetSpellData(Model + "BasicAttack");
        }

        /// <summary>
        /// Sets this AI's current target unit. This relates to both auto attacks as well as general spell targeting.
        /// </summary>
        /// <param name="target">Unit to target.</param>
        /// TODO: Remove Target class.
        public void SetTargetUnit(IAttackableUnit target)
        {
            TargetUnit = target;
            RefreshWaypoints();
        }

        public override void Update(float diff)
        {
            base.Update(diff);
            UpdateAutoAttackTarget(diff);
        }

        /// <summary>
        /// Updates this AI's current target and auto attack actions depending on conditions such as crowd control, death state, vision, distance to target, etc.
        /// </summary>
        /// <param name="diff">Number of milliseconds that passed before this tick occurred.</param>
        private void UpdateAutoAttackTarget(float diff)
        {
            if (HasCrowdControl(CrowdControlType.DISARM) || HasCrowdControl(CrowdControlType.STUN))
            {
                return;
            }

            if (IsDead)
            {
                if (TargetUnit != null)
                {
                    SetTargetUnit(null);
                    IsAttacking = false;
                    _game.PacketNotifier.NotifySetTarget(this, null);
                    HasMadeInitialAttack = false;
                }
                return;
            }

            if (TargetUnit != null)
            {
                if (TargetUnit.IsDead || !_game.ObjectManager.TeamHasVisionOn(Team, TargetUnit) && !(TargetUnit is IBaseTurret) && !(TargetUnit is IObjBuilding))
                {
                    SetTargetUnit(null);
                    IsAttacking = false;
                    _game.PacketNotifier.NotifySetTarget(this, null);
                    HasMadeInitialAttack = false;
                }
                else if (IsAttacking && TargetUnit != null && !IsDashing)
                {
                    _autoAttackCurrentDelay += diff / 1000.0f;
                    if (_autoAttackCurrentDelay >= AutoAttackCastTime / Stats.AttackSpeedMultiplier.Total)
                    {
                        if (!IsMelee)
                        {
                            var p = new Projectile(
                                _game,
                                Position,
                                5,
                                this,
                                TargetUnit,
                                null,
                                AutoAttackProjectileSpeed,
                                "",
                                0,
                                _autoAttackProjId
                            );
                            _game.ObjectManager.AddObject(p);
                            _game.PacketNotifier.NotifyForceCreateMissile(p);
                        }
                        else
                        {
                            AutoAttackHit(TargetUnit);
                        }
                        _autoAttackCurrentCooldown = 1.0f / Stats.GetTotalAttackSpeed();
                        IsAttacking = false;
                    }

                }
                else if (Vector2.DistanceSquared(Position, TargetUnit.Position) <= Stats.Range.Total * Stats.Range.Total && !IsDashing)
                {
                    RefreshWaypoints();
                    _isNextAutoCrit = _random.Next(0, 100) < Stats.CriticalChance.Total * 100;
                    if (_autoAttackCurrentCooldown <= 0)
                    {
                        IsAttacking = true;
                        _autoAttackCurrentDelay = 0;
                        _autoAttackProjId = _networkIdManager.GetNewNetId();

                        if (!HasMadeInitialAttack)
                        {
                            HasMadeInitialAttack = true;
                            _game.PacketNotifier.NotifyBeginAutoAttack(
                                this,
                                TargetUnit,
                                _autoAttackProjId,
                                _isNextAutoCrit
                            );
                        }
                        else
                        {
                            _nextAttackFlag = !_nextAttackFlag; // The first auto attack frame has occurred
                            _game.PacketNotifier.NotifyNextAutoAttack(
                                this,
                                TargetUnit,
                                _autoAttackProjId,
                                _isNextAutoCrit,
                                _nextAttackFlag
                                );
                        }

                        var attackType = IsMelee ? AttackType.ATTACK_TYPE_MELEE : AttackType.ATTACK_TYPE_TARGETED;
                        _game.PacketNotifier.NotifyOnAttack(this, TargetUnit, attackType);
                    }

                }
                else
                {
                    RefreshWaypoints();
                }

            }
            else
            {
                IsAttacking = false;
                HasMadeInitialAttack = false;
                _autoAttackCurrentDelay = 0;
                _game.PacketNotifier.NotifyNPC_InstantStopAttack(this, false);
            }

            if (_autoAttackCurrentCooldown > 0)
            {
                _autoAttackCurrentCooldown -= diff / 1000.0f;
            }
        }

        /// <summary>
        /// Sets this unit's move order to the given order.
        /// </summary>
        /// <param name="order">MoveOrder to set.</param>
        public virtual void UpdateMoveOrder(MoveOrder order)
        {
            MoveOrder = order;
        }

        /// <summary>
        /// Sets this AI's current target unit.
        /// </summary>
        /// <param name="unit">Unit to target.</param>
        public void UpdateTargetUnit(IAttackableUnit unit)
        {
            TargetUnit = unit;
        }
    }
}
