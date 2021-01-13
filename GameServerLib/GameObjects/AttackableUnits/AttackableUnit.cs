using System;
using System.Collections.Generic;
using System.Numerics;
using GameServerCore;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.Items;
using LeagueSandbox.GameServer.Logging;
using log4net;

namespace LeagueSandbox.GameServer.GameObjects.AttackableUnits
{
    /// <summary>
    /// Base class for all attackable units.
    /// AttackableUnits normally follow these guidelines of functionality: Death state, basic Movement, Stats (including modifiers and basic replication), Buffs (and their scripts), and Call for Help.
    /// </summary>
    public class AttackableUnit : GameObject, IAttackableUnit
    {
        // Crucial Vars.
        private float _statUpdateTimer;

        // Utility Vars.
        internal const float DETECT_RANGE = 475.0f;
        internal const int EXP_RANGE = 1400;
        protected readonly ILog Logger;

        /// <summary>
        /// Whether or not this Unit is dead. Refer to TakeDamage() and Die().
        /// </summary>
        public bool IsDead { get; protected set; }
        /// <summary>
        /// Whether or not this Unit's model has been changeds this tick. Resets to False when the next tick update happens in ObjectManager.
        /// </summary>
        public bool IsModelUpdated { get; set; }
        /// <summary>
        /// The "score" of this Unit which increases as kills are gained and decreases as deaths are inflicted.
        /// Used in determining kill gold rewards.
        /// </summary>
        public int KillDeathCounter { get; set; }
        /// <summary>
        /// Number of minions this Unit has killed. Unused besides in replication which is used for packets, refer to NotifyUpdateStats in PacketNotifier.
        /// </summary>
        /// TODO: Verify if we want to move this to ObjAIBase since AttackableUnits cannot attack or kill anything.
        public int MinionCounter { get; protected set; }
        /// <summary>
        /// This Unit's current internally named model.
        /// </summary>
        public string Model { get; protected set; }
        /// <summary>
        /// Waypoints that make up the path a game object is walking in.
        /// </summary>
        public List<Vector2> Waypoints { get; protected set; }
        /// <summary>
        /// Index of the waypoint in the list of waypoints that the object is currently on.
        /// </summary>
        public KeyValuePair<int, Vector2> CurrentWaypoint { get; protected set; }
        /// <summary>
        /// Speed of the unit's current dash.
        /// </summary>
        /// TODO: Implement a dash class so dash based variables and functions can be separate from units.
        public float DashSpeed { get; set; }
        /// <summary>
        /// Amount of time passed since the unit started dashing.
        /// </summary>
        /// TODO: Implement a dash class so dash based variables and functions can be separate from units.
        public float DashElapsedTime { get; set; }
        /// <summary>
        /// Total amount of time the unit will dash.
        /// </summary>
        /// TODO: Implement a dash class so dash based variables and functions can be separate from units.
        public float DashTime { get; set; }
        /// <summary>
        /// Whether or not this unit is currently dashing.
        /// </summary>
        public bool IsDashing { get; protected set; }
        /// <summary>
        /// Stats used purely in networking the accompishments or status of units and their gameplay affecting stats.
        /// </summary>
        public IReplication Replication { get; protected set; }
        /// <summary>
        /// Variable housing all of this Unit's stats such as health, mana, armor, magic resist, ActionState, etc.
        /// Currently these are only initialized manually by ObjAIBase and ObjBuilding.
        /// </summary>
        public IStats Stats { get; protected set; }

        public AttackableUnit(
            Game game,
            string model,
            IStats stats,
            int collisionRadius = 40,
            Vector2 position = new Vector2(),
            int visionRadius = 0,
            uint netId = 0,
            TeamId team = TeamId.TEAM_NEUTRAL
        ) : base(game, position, collisionRadius, visionRadius, netId, team)

        {
            Logger = LoggerProvider.GetLogger();
            Stats = stats;
            Model = model;
            Waypoints = new List<Vector2> { Position };
            CurrentWaypoint = new KeyValuePair<int, Vector2>(1, Position);
            IsDashing = false;
            Stats.AttackSpeedMultiplier.BaseValue = 1.0f;
        }

        public override void OnAdded()
        {
            base.OnAdded();
            _game.ObjectManager.AddVisionUnit(this);
        }

        public override void Update(float diff)
        {
            // TODO: Rework stat management.
            _statUpdateTimer += diff;
            while (_statUpdateTimer >= 500)
            {
                // update Stats (hpregen, manaregen) every 0.5 seconds
                Stats.Update(_statUpdateTimer);
                _statUpdateTimer -= 500;
            }

            Move(diff);

            if (IsDashing && IsPathEnded())
            {
                if (DashTime == 0)
                {
                    IsDashing = false;
                    var animList = new List<string> { "RUN" };
                    _game.PacketNotifier.NotifySetAnimation(this, animList);
                    return;
                }

                DashElapsedTime += diff;
                if (DashElapsedTime >= DashTime)
                {
                    IsDashing = false;
                    DashTime = 0;
                    DashElapsedTime = 0;
                    var animList = new List<string> { "RUN" };
                    _game.PacketNotifier.NotifySetAnimation(this, animList);
                }
            }
        }

        public override void OnRemoved()
        {
            base.OnRemoved();
            _game.ObjectManager.RemoveVisionUnit(this);
        }

        /// <summary>
        /// Sets the position of this unit to the specified position and stops its movements.
        /// </summary>
        /// <param name="x">X coordinate to set.</param>
        /// <param name="y">Y coordinate to set.</param>
        public override void TeleportTo(float x, float y)
        {
            base.TeleportTo(x, y);
            StopMovement();
        }

        /// <summary>
        /// Returns whether or not this unit is targetable to the specified team.
        /// </summary>
        /// <param name="team">TeamId to check for.</param>
        /// <returns>True/False.</returns>
        public bool GetIsTargetableToTeam(TeamId team)
        {
            if (!Stats.IsTargetable)
            {
                return false;
            }

            if (Team == team)
            {
                return !Stats.IsTargetableToTeam.HasFlag(SpellFlags.NonTargetableAlly);
            }

            return !Stats.IsTargetableToTeam.HasFlag(SpellFlags.NonTargetableEnemy);
        }

        /// <summary>
        /// Sets whether or not this unit should be targetable.
        /// </summary>
        /// <param name="targetable">True/False.</param>
        public void SetIsTargetable(bool targetable)
        {
            Stats.IsTargetable = targetable;
        }

        /// <summary>
        /// Sets whether or not this unit is targetable to the specified team.
        /// </summary>
        /// <param name="team">TeamId to change.</param>
        /// <param name="targetable">True/False.</param>
        public void SetIsTargetableToTeam(TeamId team, bool targetable)
        {
            Stats.IsTargetableToTeam &= ~SpellFlags.TargetableToAll;
            if (team == Team)
            {
                if (!targetable)
                {
                    Stats.IsTargetableToTeam |= SpellFlags.NonTargetableAlly;
                }
                else
                {
                    Stats.IsTargetableToTeam &= ~SpellFlags.NonTargetableAlly;
                }
            }
            else
            {
                if (!targetable)
                {
                    Stats.IsTargetableToTeam |= SpellFlags.NonTargetableEnemy;
                }
                else
                {
                    Stats.IsTargetableToTeam &= ~SpellFlags.NonTargetableEnemy;
                }
            }
        }

        /// <summary>
        /// Applies damage to this unit.
        /// </summary>
        /// <param name="attacker">Unit that is dealing the damage.</param>
        /// <param name="damage">Amount of damage to deal.</param>
        /// <param name="type">Whether the damage is physical, magical, or true.</param>
        /// <param name="source">What the damage came from: attack, spell, summoner spell, or passive.</param>
        /// <param name="damageText">Type of damage the damage text should be.</param>
        public virtual void TakeDamage(IAttackableUnit attacker, float damage, DamageType type, DamageSource source,
            DamageResultType damageText)
        {
            float defense = 0;
            float regain = 0;
            var attackerStats = attacker.Stats;

            switch (type)
            {
                case DamageType.DAMAGE_TYPE_PHYSICAL:
                    defense = Stats.Armor.Total;
                    defense = (1 - attackerStats.ArmorPenetration.PercentBonus) * defense -
                              attackerStats.ArmorPenetration.FlatBonus;

                    break;
                case DamageType.DAMAGE_TYPE_MAGICAL:
                    defense = Stats.MagicPenetration.Total;
                    defense = (1 - attackerStats.MagicPenetration.PercentBonus) * defense -
                              attackerStats.MagicPenetration.FlatBonus;
                    break;
                case DamageType.DAMAGE_TYPE_TRUE:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            switch (source)
            {
                case DamageSource.DAMAGE_SOURCE_RAW:
                    break;
                case DamageSource.DAMAGE_SOURCE_INTERNALRAW:
                    break;
                case DamageSource.DAMAGE_SOURCE_PERIODIC:
                    break;
                case DamageSource.DAMAGE_SOURCE_PROC:
                    break;
                case DamageSource.DAMAGE_SOURCE_REACTIVE:
                    break;
                case DamageSource.DAMAGE_SOURCE_ONDEATH:
                    break;
                case DamageSource.DAMAGE_SOURCE_SPELL:
                    regain = attackerStats.SpellVamp.Total;
                    break;
                case DamageSource.DAMAGE_SOURCE_ATTACK:
                    regain = attackerStats.LifeSteal.Total;
                    break;
                case DamageSource.DAMAGE_SOURCE_DEFAULT:
                    break;
                case DamageSource.DAMAGE_SOURCE_SPELLAOE:
                    break;
                case DamageSource.DAMAGE_SOURCE_SPELLPERSIST:
                    break;
                case DamageSource.DAMAGE_SOURCE_PET:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(source), source, null);
            }

            if (damage < 0f)
            {
                damage = 0f;
            }
            else
            {
                //Damage dealing. (based on leagueoflegends' wikia)
                damage = defense >= 0 ? 100 / (100 + defense) * damage : (2 - 100 / (100 - defense)) * damage;
            }

            ApiEventManager.OnUnitDamageTaken.Publish(this);

            Stats.CurrentHealth = Math.Max(0.0f, Stats.CurrentHealth - damage);
            if (!IsDead && Stats.CurrentHealth <= 0)
            {
                IsDead = true;
                Die(attacker);
            }

            int attackerId = 0, targetId = 0;

            // todo: check if damage dealt by disconnected players cause anything bad 
            if (attacker is IChampion attackerChamp)
            {
                attackerId = (int)_game.PlayerManager.GetClientInfoByChampion(attackerChamp).PlayerId;
            }

            if (this is IChampion targetChamp)
            {
                targetId = (int)_game.PlayerManager.GetClientInfoByChampion(targetChamp).PlayerId;
            }
            // Show damage text for owner of pet
            if (attacker is IMinion attackerMinion && attackerMinion.IsPet && attackerMinion.Owner is IChampion)
            {
                attackerId = (int)_game.PlayerManager.GetClientInfoByChampion((IChampion)attackerMinion.Owner).PlayerId;
            }

            _game.PacketNotifier.NotifyUnitApplyDamage(attacker, this, damage, type, damageText,
                _game.Config.IsDamageTextGlobal, attackerId, targetId);

            // TODO: send this in one place only
            _game.PacketNotifier.NotifyUpdatedStats(this, false);

            // Get health from lifesteal/spellvamp
            if (regain > 0)
            {
                attackerStats.CurrentHealth = Math.Min(attackerStats.HealthPoints.Total,
                    attackerStats.CurrentHealth + regain * damage);
                // TODO: send this in one place only (preferably a central EventHandler class)
                _game.PacketNotifier.NotifyUpdatedStats(attacker, false);
            }
        }

        /// <summary>
        /// Applies damage to this unit.
        /// </summary>
        /// <param name="attacker">Unit that is dealing the damage.</param>
        /// <param name="damage">Amount of damage to deal.</param>
        /// <param name="type">Whether the damage is physical, magical, or true.</param>
        /// <param name="source">What the damage came from: attack, spell, summoner spell, or passive.</param>
        /// <param name="isCrit">Whether or not the damage text should be shown as a crit.</param>
        public virtual void TakeDamage(IAttackableUnit attacker, float damage, DamageType type, DamageSource source, bool isCrit)
        {
            var text = DamageResultType.RESULT_NORMAL;

            if (isCrit)
            {
                text = DamageResultType.RESULT_CRITICAL;
            }

            TakeDamage(attacker, damage, type, source, text);
        }

        /// <summary>
        /// Whether or not this unit is currently calling for help. Unimplemented.
        /// </summary>
        /// <returns>True/False.</returns>
        /// TODO: Implement this.
        public virtual bool IsInDistress()
        {
            return false; //return DistressCause;
        }

        /// <summary>
        /// Called when this unit dies.
        /// </summary>
        /// <param name="killer">Unit that killed this unit.</param>
        public virtual void Die(IAttackableUnit killer)
        {
            SetToRemove();
            _game.ObjectManager.StopTargeting(this);

            _game.PacketNotifier.NotifyNpcDie(this, killer);

            var exp = _game.Map.MapProperties.GetExperienceFor(this);
            var champs = _game.ObjectManager.GetChampionsInRange(Position, EXP_RANGE, true);
            //Cull allied champions
            champs.RemoveAll(l => l.Team == Team);

            if (champs.Count > 0)
            {
                var expPerChamp = exp / champs.Count;
                foreach (var c in champs)
                {
                    c.Stats.Experience += expPerChamp;
                    _game.PacketNotifier.NotifyAddXp(c, expPerChamp);
                }
            }

            if (killer != null && killer is IChampion champion)
                champion.OnKill(this);
        }

        /// <summary>
        /// Sets this unit's current model to the specified internally named model. *NOTE*: If the model is not present in the client files, all connected players will crash.
        /// </summary>
        /// <param name="model">Internally named model to set.</param>
        /// <returns></returns>
        /// TODO: Implement model verification (perhaps by making a list of all models in Content) so that clients don't crash if a model which doesn't exist in client files is given.
        public bool ChangeModel(string model)
        {
            if (Model.Equals(model))
            {
                return false;
            }
            IsModelUpdated = true;
            Model = model;
            return true;
        }

        /// <summary>
        /// Gets the movement speed stat of this unit.
        /// </summary>
        /// <returns>Float units/sec.</returns>
        public virtual float GetMoveSpeed()
        {
            return Stats.MoveSpeed.Total;
        }

        /// <summary>
        /// Moves this unit to its specified waypoints, updating its position along the way.
        /// </summary>
        /// <param name="diff">The amount of milliseconds the unit is supposed to move</param>
        private void Move(float diff)
        {
            // no waypoints remained - clear the Waypoints
            if (CurrentWaypoint.Key >= Waypoints.Count)
            {
                StopMovement();
                return;
            }

            // current -> next positions
            var cur = new Vector2(Position.X, Position.Y);
            var next = CurrentWaypoint.Value;

            if (cur == next)
            {
                StopMovement();
            }

            var goingTo = next - cur;
            _direction = Vector2.Normalize(goingTo);

            // usually doesn't happen
            if (float.IsNaN(_direction.X) || float.IsNaN(_direction.Y))
            {
                _direction = new Vector2(0, 0);
            }

            var moveSpeed = GetMoveSpeed();

            var distSqr = MathF.Abs(Vector2.DistanceSquared(cur, next));

            var deltaMovement = moveSpeed * 0.001f * diff;

            // Prevent moving past the next waypoint.
            if (deltaMovement * deltaMovement > distSqr)
            {
                deltaMovement = MathF.Sqrt(distSqr);
            }

            var xx = _direction.X * deltaMovement;
            var yy = _direction.Y * deltaMovement;

            // TODO: Prevent movement past obstacles (after moving movement functionality to AttackableUnit).
            //Vector2 nextPos = new Vector2(X + xx, Y + yy);
            //KeyValuePair<bool, Vector2> pathBlocked = _game.Map.NavigationGrid.IsAnythingBetween(GetPosition(), nextPos);
            //if (pathBlocked.Key)
            //{
            //    nextPos = _game.Map.NavigationGrid.GetClosestTerrainExit(pathBlocked.Value, CollisionRadius + 1.0f);
            //}

            Position = new Vector2(Position.X + xx, Position.Y + yy);

            // (X, Y) have now moved to the next position
            cur = new Vector2(Position.X, Position.Y);

            // Check if we reached the next waypoint
            // REVIEW (of previous code): (deltaMovement * 2) being used here is problematic; if the server lags, the diff will be much greater than the usual values
            if ((cur - next).LengthSquared() < MOVEMENT_EPSILON * MOVEMENT_EPSILON)
            {
                var nextIndex = CurrentWaypoint.Key + 1;
                // stop moving because we have reached our last waypoint
                if (nextIndex >= Waypoints.Count)
                {
                    return;
                }
                // start moving to our next waypoint
                else
                {
                    CurrentWaypoint = new KeyValuePair<int, Vector2>(nextIndex, Waypoints[nextIndex]);
                }
            }
        }

        /// <summary>
        /// Returns the next waypoint. If all waypoints have been reached then this returns a -inf Vector2
        /// </summary>
        public Vector2 GetNextWaypoint()
        {
            if (CurrentWaypoint.Key < Waypoints.Count)
            {
                return CurrentWaypoint.Value;
            }
            return new Vector2(float.NegativeInfinity, float.NegativeInfinity);
        }

        /// <summary>
        /// Returns whether this unit has reached the last waypoint in its path of waypoints.
        /// </summary>
        public bool IsPathEnded()
        {
            return CurrentWaypoint.Key >= Waypoints.Count;
        }

        /// <summary>
        /// Sets this unit's movement path to the given waypoints.
        /// </summary>
        /// <param name="newWaypoints">New path of Vector2 coordinates that the unit will move to.</param>
        public void SetWaypoints(List<Vector2> newWaypoints)
        {
            Waypoints = newWaypoints;

            _movementUpdated = true;
            if (Waypoints.Count == 1)
            {
                StopMovement();
                return;
            }

            CurrentWaypoint = new KeyValuePair<int, Vector2>(1, Waypoints[1]);
        }

        /// <summary>
        /// Forces this unit to stop moving.
        /// </summary>
        public virtual void StopMovement()
        {
            Waypoints = new List<Vector2> { Position };
            CurrentWaypoint = new KeyValuePair<int, Vector2>(1, Position);
        }

        /// <summary>
        /// Returns whether this unit has set its waypoints this update.
        /// </summary>
        /// <returns>True/False</returns>
        public bool IsMovementUpdated()
        {
            return _movementUpdated;
        }

        /// <summary>
        /// Used each object manager update after this unit has set its waypoints and the server has networked it.
        /// </summary>
        public void ClearMovementUpdated()
        {
            _movementUpdated = false;
        }

        /// <summary>
        /// Forces this unit to perform a dash which ends at the given position.
        /// </summary>
        /// <param name="endPos">Position to end the dash at.</param>
        /// <param name="dashSpeed">Amount of units the dash should travel in a second (movespeed).</param>
        /// <param name="animation">Internal name of the dash animation.</param>
        /// <param name="leapGravity">Optionally how much gravity the unit will experience when above the ground while dashing.</param>
        /// <param name="keepFacingLastDirection">Whether or not the AI unit should face the direction they were facing before the dash.</param>
        /// TODO: Find a good way to grab these variables from spell data.
        /// TODO: Verify if we should count Dashing as a form of Crowd Control.
        /// TODO: Implement Dash class which houses these parameters, then have that as the only parameter to this function (and other Dash-based functions).
        public void DashToLocation(Vector2 endPos, float dashSpeed, string animation = "RUN", float leapGravity = 0.0f, bool keepFacingLastDirection = true)
        {
            var newCoords = _game.Map.NavigationGrid.GetClosestTerrainExit(endPos, CollisionRadius + 1.0f);
            // TODO: Take into account the rest of the arguments
            IsDashing = true;
            DashSpeed = dashSpeed;
            DashTime = 0;
            DashElapsedTime = 0;
            SetWaypoints(new List<Vector2> { Position, newCoords });

            _game.PacketNotifier.NotifyWaypointGroupWithSpeed(this, dashSpeed, leapGravity, keepFacingLastDirection);

            if (animation == null)
            {
                animation = "RUN";
            }

            var animList = new List<string> { "RUN", animation };
            _game.PacketNotifier.NotifySetAnimation(this, animList);
        }

        /// <summary>
        /// Sets this unit's current dash state to the given state.
        /// </summary>
        /// <param name="state">State to set. True = dashing, false = not dashing.</param>
        /// TODO: Verify if we want to classify Dashing as a form of Crowd Control.
        public virtual void SetDashingState(bool state)
        {
            IsDashing = state;

            if (state == false)
            {
                StopMovement();
                DashTime = 0;
                DashElapsedTime = 0;

                var animList = new List<string> { "RUN" };
                _game.PacketNotifier.NotifySetAnimation(this, animList);
            }
        }
    }
}
