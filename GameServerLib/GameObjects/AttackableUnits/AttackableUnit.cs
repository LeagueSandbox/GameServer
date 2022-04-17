using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using GameServerCore;
using GameServerCore.Content;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using GameServerCore.Domain.GameObjects.Spell.Sector;
using GameServerCore.Enums;
using GameServerLib.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.Logging;
using log4net;

namespace LeagueSandbox.GameServer.GameObjects.AttackableUnits
{
    /// <summary>
    /// Base class for all attackable units.
    /// AttackableUnits normally follow these guidelines of functionality: Death state, forced movements, Crowd Control, Stats (including modifiers and basic replication), Buffs (and their scripts), and Call for Help.
    /// </summary>
    public class AttackableUnit : GameObject, IAttackableUnit
    {
        // Crucial Vars.
        private float _statUpdateTimer;
        private object _buffsLock;
        private IDeathData _death;
        protected readonly ILog Logger;

        //TODO: Find out where this variable came from and if it can be unhardcoded
        internal const float DETECT_RANGE = 475.0f;

        /// <summary>
        /// Whether or not this Unit is dead. Refer to TakeDamage() and Die().
        /// </summary>
        public bool IsDead { get; protected set; }
        /// <summary>
        /// Whether or not this Unit's model has been changeds this tick. Resets to False when the next tick update happens in ObjectManager.
        /// </summary>
        public bool IsModelUpdated { get; set; }
        /// <summary>
        /// Number of minions this Unit has killed. Unused besides in replication which is used for packets, refer to NotifyOnReplication in PacketNotifier.
        /// </summary>
        /// TODO: Verify if we want to move this to ObjAIBase since AttackableUnits cannot attack or kill anything.
        public int MinionCounter { get; protected set; }
        /// <summary>
        /// This Unit's current internally named model.
        /// </summary>
        public string Model { get; protected set; }
        /// <summary>
        /// Stats used purely in networking the accompishments or status of units and their gameplay affecting stats.
        /// </summary>
        public IReplication Replication { get; protected set; }
        /// <summary>
        /// Variable housing all of this Unit's stats such as health, mana, armor, magic resist, ActionState, etc.
        /// Currently these are only initialized manually by ObjAIBase and ObjBuilding.
        /// </summary>
        public IStats Stats { get; protected set; }
        /// <summary>
        /// Variable which stores the number of times a unit has teleported. Used purely for networking.
        /// Resets when reaching byte.MaxValue (255).
        /// </summary>
        public byte TeleportID { get; set; }
        /// <summary>
        /// Array of buff slots which contains all parent buffs (oldest buff of a given name) applied to this AI.
        /// Maximum of 256 slots, hard limit due to packets.
        /// </summary>
        /// TODO: Move to AttackableUnit.
        private IBuff[] BuffSlots { get; }
        /// <summary>
        /// Dictionary containing all parent buffs (oldest buff of a given name). Used for packets and assigning stacks if a buff of the same name is added.
        /// </summary>
        /// TODO: Move to AttackableUnit.
        private Dictionary<string, IBuff> ParentBuffs { get; }
        /// <summary>
        /// List of all buffs applied to this AI. Used for easier indexing of buffs.
        /// </summary>
        /// TODO: Verify if we can remove this in favor of BuffSlots while keeping the functions which allow for easy accessing of individual buff instances.
        /// TODO: Move to AttackableUnit.
        private List<IBuff> BuffList { get; }
        /// <summary>
        /// Waypoints that make up the path a game object is walking in.
        /// </summary>
        public List<Vector2> Waypoints { get; protected set; }
        /// <summary>
        /// Index of the waypoint in the list of waypoints that the object is currently on.
        /// </summary>
        public KeyValuePair<int, Vector2> CurrentWaypoint { get; protected set; }
        /// <summary>
        /// Status effects enabled on this unit. Refer to StatusFlags enum.
        /// </summary>
        public StatusFlags Status { get; protected set; }
        /// <summary>
        /// Parameters of any forced movements (dashes) this unit is performing.
        /// </summary>
        public IForceMovementParameters MovementParameters { get; protected set; }

        public AttackableUnit(
            Game game,
            string model,
            IStats stats,
            int collisionRadius = 40,
            Vector2 position = new Vector2(),
            int visionRadius = 0,
            uint netId = 0,
            TeamId team = TeamId.TEAM_NEUTRAL
        ) : base(game, position, collisionRadius, collisionRadius, visionRadius, netId, team)

        {
            Logger = LoggerProvider.GetLogger();
            Stats = stats;
            Model = model;
            Waypoints = new List<Vector2> { Position };
            CurrentWaypoint = new KeyValuePair<int, Vector2>(1, Position);
            Status = StatusFlags.CanAttack | StatusFlags.CanCast | StatusFlags.CanMove | StatusFlags.CanMoveEver | StatusFlags.Targetable;
            MovementParameters = null;
            Stats.AttackSpeedMultiplier.BaseValue = 1.0f;

            _buffsLock = new object();
            BuffSlots = new IBuff[256];
            ParentBuffs = new Dictionary<string, IBuff>();
            BuffList = new List<IBuff>();
        }

        public override void OnAdded()
        {
            base.OnAdded();
            _game.ObjectManager.AddVisionProvider(this, Team);
        }

        public override void OnRemoved()
        {
            base.OnRemoved();
            _game.ObjectManager.RemoveVisionProvider(this, Team);
        }

        public override void SetTeam(TeamId team)
        {
            _game.ObjectManager.RemoveVisionProvider(this, Team);
            base.SetTeam(team);
            _game.ObjectManager.AddVisionProvider(this, Team);
        }

        /// <summary>
        /// Gets the HashString for this unit's model. Used for packets so clients know what data to load.
        /// </summary>
        /// <returns>Hashed string of this unit's model.</returns>
        public uint GetObjHash()
        {
            var gobj = "[Character]" + Model;

            // TODO: Account for any other units that have skins (requires skins to be implemented for those units)
            if (this is IChampion c)
            {
                var szSkin = "";
                if (c.SkinID < 10)
                {
                    szSkin = "0" + c.SkinID;
                }
                else
                {
                    szSkin = c.SkinID.ToString();
                }
                gobj += szSkin;
            }

            return HashFunctions.HashStringNorm(gobj);
        }

        /// <summary>
        /// Sets the server-sided position of this object. Optionally takes into account the AI's current waypoints.
        /// </summary>
        /// <param name="vec">Position to set.</param>
        /// <param name="repath">Whether or not to repath the AI from the given position (assuming it has a path).</param>
        public void SetPosition(Vector2 vec, bool repath = true)
        {
            Position = vec;
            _movementUpdated = true;

            if (!IsPathEnded())
            {
                // Reevaluate our current path to account for the starting position being changed.
                if (repath)
                {
                    List<Vector2> safePath = _game.Map.PathingHandler.GetPath(Position, _game.Map.NavigationGrid.GetClosestTerrainExit(Waypoints.Last(), PathfindingRadius));

                    // TODO: When using this safePath, sometimes we collide with the terrain again, so we use an unsafe path the next collision, however,
                    // sometimes we collide again before we can finish the unsafe path, so we end up looping collisions between safe and unsafe paths, never actually escaping (ex: sharp corners).
                    // This is a more fundamental issue where the pathfinding should be taking into account collision radius, rather than simply pathing from center of an object.
                    if (safePath != null)
                    {
                        SetWaypoints(safePath);
                    }
                }
                else
                {
                    Waypoints[0] = Position;
                }
            }
            else
            {
                ResetWaypoints();
            }
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
                API.ApiEventManager.OnUpdateStats.Publish(this, diff);
            }

            Replication.Update();

            if (Waypoints.Count > 1 && CanMove())
            {
                Move(diff);
            }

            // Prevents edge cases where a movement command is performed in the same tick as a ForceMovement.
            // TODO: Perhaps just make a check for MovementParameters in ObjectManager.Sync, and send WaypointGroupWithSpeed instead.
            if (IsMovementUpdated() && !CanChangeWaypoints())
            {
                _movementUpdated = false;
            }

            if (MovementParameters != null && MovementParameters.FollowNetID > 0)
            {
                MovementParameters.SetTimeElapsed(MovementParameters.ElapsedTime + diff);
                if (MovementParameters.ElapsedTime >= MovementParameters.FollowTravelTime && MovementParameters.FollowTravelTime >= 0)
                {
                    SetDashingState(false);
                }
            }

            if (IsDead && _death != null)
            {
                Die(_death);
                _death = null;
            }

            UpdateStatus();
        }

        /// <summary>
        /// Called when this unit collides with the terrain or with another GameObject. Refer to CollisionHandler for exact cases.
        /// </summary>
        /// <param name="collider">GameObject that collided with this AI. Null if terrain.</param>
        /// <param name="isTerrain">Whether or not this AI collided with terrain.</param>
        public override void OnCollision(IGameObject collider, bool isTerrain = false)
        {
            // We do not want to teleport out of missiles, sectors, owned regions, or buildings. Buildings in particular are already baked into the Navigation Grid.
            if (collider is ISpellMissile || collider is ISpellSector || collider is IObjBuilding || (collider is IRegion region && region.CollisionUnit == this))
            {
                return;
            }

            if (isTerrain)
            {
                ApiEventManager.OnCollisionTerrain.Publish(this);

                if (MovementParameters != null)
                {
                    return;
                }

                // only time we would collide with terrain is if we are inside of it, so we should teleport out of it.
                Vector2 exit = _game.Map.NavigationGrid.GetClosestTerrainExit(Position, PathfindingRadius + 1.0f);
                SetPosition(exit, false);
            }
            else
            {
                ApiEventManager.OnCollision.Publish(this, collider);

                if (MovementParameters != null || Status.HasFlag(StatusFlags.Ghosted)
                    || (collider is IAttackableUnit unit &&
                    (unit.MovementParameters != null || unit.Status.HasFlag(StatusFlags.Ghosted))))
                {
                    return;
                }

                // We should not teleport here because Pathfinding should handle it.
                // TODO: Implement a PathfindingHandler, and remove currently implemented manual pathfinding.
                Vector2 exit = Extensions.GetCircleEscapePoint(Position, PathfindingRadius + 1, collider.Position, collider.PathfindingRadius);
                if (!_game.Map.PathingHandler.IsWalkable(exit, PathfindingRadius))
                {
                    exit = _game.Map.NavigationGrid.GetClosestTerrainExit(exit, PathfindingRadius + 1.0f);
                }
                SetPosition(exit, false);
            }
        }

        /// <summary>
        /// Returns whether or not this unit is targetable to the specified team.
        /// </summary>
        /// <param name="team">TeamId to check for.</param>
        /// <returns>True/False.</returns>
        public bool GetIsTargetableToTeam(TeamId team)
        {
            if (!Status.HasFlag(StatusFlags.Targetable))
            {
                return false;
            }

            if (Team == team)
            {
                return !Stats.IsTargetableToTeam.HasFlag(SpellDataFlags.NonTargetableAlly);
            }

            return !Stats.IsTargetableToTeam.HasFlag(SpellDataFlags.NonTargetableEnemy);
        }

        /// <summary>
        /// Sets whether or not this unit is targetable to the specified team.
        /// </summary>
        /// <param name="team">TeamId to change.</param>
        /// <param name="targetable">True/False.</param>
        public void SetIsTargetableToTeam(TeamId team, bool targetable)
        {
            Stats.IsTargetableToTeam &= ~SpellDataFlags.TargetableToAll;
            if (team == Team)
            {
                if (!targetable)
                {
                    Stats.IsTargetableToTeam |= SpellDataFlags.NonTargetableAlly;
                }
                else
                {
                    Stats.IsTargetableToTeam &= ~SpellDataFlags.NonTargetableAlly;
                }
            }
            else
            {
                if (!targetable)
                {
                    Stats.IsTargetableToTeam |= SpellDataFlags.NonTargetableEnemy;
                }
                else
                {
                    Stats.IsTargetableToTeam &= ~SpellDataFlags.NonTargetableEnemy;
                }
            }
        }

        /// <summary>
        /// Whether or not this unit can move itself.
        /// </summary>
        /// <returns></returns>
        public virtual bool CanMove()
        {
            // Only case where AttackableUnit should move is if it is forced.
            return MovementParameters != null;
        }

        /// <summary>
        /// Whether or not this unit can modify its Waypoints.
        /// </summary>
        public virtual bool CanChangeWaypoints()
        {
            // Only case where we can change waypoints is if we are being forced to move towards a target.
            return MovementParameters != null && MovementParameters.FollowNetID != 0;
        }

        /// <summary>
        /// Whether or not this unit can take damage of the given type.
        /// </summary>
        /// <param name="type">Type of damage to check.</param>
        /// <returns>True/False</returns>
        public bool CanTakeDamage(DamageType type)
        {
            if (Status.HasFlag(StatusFlags.Invulnerable))
            {
                return false;
            }

            switch (type)
            {
                case DamageType.DAMAGE_TYPE_PHYSICAL:
                {
                    if (Status.HasFlag(StatusFlags.PhysicalImmune))
                    {
                        return false;
                    }
                    break;
                }
                case DamageType.DAMAGE_TYPE_MAGICAL:
                {
                    if (Status.HasFlag(StatusFlags.MagicImmune))
                    {
                        return false;
                    }
                    break;
                }
                case DamageType.DAMAGE_TYPE_MIXED:
                {
                    if (Status.HasFlag(StatusFlags.MagicImmune) || Status.HasFlag(StatusFlags.PhysicalImmune))
                    {
                        return false;
                    }
                    break;
                }
            }

            return true;
        }

        /// <summary>
        /// Adds a modifier to this unit's stats, ex: Armor, Attack Damage, Movespeed, etc.
        /// </summary>
        /// <param name="statModifier">Modifier to add.</param>
        public void AddStatModifier(IStatsModifier statModifier)
        {
            Stats.AddModifier(statModifier);
        }

        /// <summary>
        /// Removes the given stat modifier instance from this unit.
        /// </summary>
        /// <param name="statModifier">Stat modifier instance to remove.</param>
        public void RemoveStatModifier(IStatsModifier statModifier)
        {
            Stats.RemoveModifier(statModifier);
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
            float regain = 0;
            var attackerStats = attacker.Stats;
            float postMitigationDamage = Stats.GetPostMitigationDamage(damage, type, attacker);

            IDamageData damageData = new DamageData
            {
                IsAutoAttack = source == DamageSource.DAMAGE_SOURCE_ATTACK,
                Attacker = attacker,
                Target = this,
                Damage = damage,
                PostMitigationdDamage = postMitigationDamage,
                DamageSource = source,
                DamageType = type,
            };

            ApiEventManager.OnPreTakeDamage.Publish(damageData);

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

            if (!CanTakeDamage(type))
            {
                return;
            }

            Stats.CurrentHealth = Math.Max(0.0f, Stats.CurrentHealth - postMitigationDamage);

            ApiEventManager.OnTakeDamage.Publish(damageData);

            if (!IsDead && Stats.CurrentHealth <= 0)
            {
                IsDead = true;
                _death = new DeathData
                {
                    BecomeZombie = false, // TODO: Unhardcode
                    DieType = 0, // TODO: Unhardcode
                    Unit = this,
                    Killer = attacker,
                    DamageType = type,
                    DamageSource = source,
                    DeathDuration = 0 // TODO: Unhardcode
                };
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
            if (attacker is IMinion attackerMinion && attackerMinion is IPet && attackerMinion.Owner is IChampion)
            {
                attackerId = (int)_game.PlayerManager.GetClientInfoByChampion((IChampion)attackerMinion.Owner).PlayerId;
            }

            if (attacker.Team != Team)
            {
                _game.PacketNotifier.NotifyUnitApplyDamage(attacker, this, postMitigationDamage, type, damageText,
                    _game.Config.IsDamageTextGlobal, attackerId, targetId);
            }

            // Get health from lifesteal/spellvamp
            if (regain > 0)
            {
                attackerStats.CurrentHealth = Math.Min
                (
                    attackerStats.HealthPoints.Total,
                    attackerStats.CurrentHealth + regain * postMitigationDamage
                );
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
        /// Applies damage to this unit.
        /// </summary>
        /// <param name="attacker">Unit that is dealing the damage.</param>
        /// <param name="damage">Amount of damage to deal.</param>
        /// <param name="type">Whether the damage is physical, magical, or true.</param>
        /// <param name="source">What the damage came from: attack, spell, summoner spell, or passive.</param>
        /// <param name="damageText">Type of damage the damage text should be.</param>
        public virtual void TakeDamage(IDamageData damageData, DamageResultType damageText)
        {
            float regain = 0;
            var attacker = damageData.Attacker;
            var attackerStats = damageData.Attacker.Stats;
            var type = damageData.DamageType;
            var source = damageData.DamageSource;
            var postMitigationDamage = damageData.PostMitigationdDamage;

            ApiEventManager.OnPreTakeDamage.Publish(damageData);

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

            if (!CanTakeDamage(type))
            {
                return;
            }

            Stats.CurrentHealth = Math.Max(0.0f, Stats.CurrentHealth - postMitigationDamage);

            ApiEventManager.OnTakeDamage.Publish(damageData);

            if (!IsDead && Stats.CurrentHealth <= 0)
            {
                IsDead = true;
                _death = new DeathData
                {
                    BecomeZombie = false, // TODO: Unhardcode
                    DieType = 0, // TODO: Unhardcode
                    Unit = this,
                    Killer = attacker,
                    DamageType = type,
                    DamageSource = source,
                    DeathDuration = 0 // TODO: Unhardcode
                };
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
            if (attacker is IMinion attackerMinion && attackerMinion is IPet && attackerMinion.Owner is IChampion)
            {
                attackerId = (int)_game.PlayerManager.GetClientInfoByChampion((IChampion)attackerMinion.Owner).PlayerId;
            }

            if (attacker.Team != Team)
            {
                _game.PacketNotifier.NotifyUnitApplyDamage(attacker, this, postMitigationDamage, type, damageText,
                    _game.Config.IsDamageTextGlobal, attackerId, targetId);
            }

            // Get health from lifesteal/spellvamp
            if (regain > 0)
            {
                attackerStats.CurrentHealth = Math.Min
                (
                    attackerStats.HealthPoints.Total,
                    attackerStats.CurrentHealth + regain * postMitigationDamage
                );
            }
        }

        public void TakeDamage(IDamageData damageData, bool isCrit)
        {
            var text = DamageResultType.RESULT_NORMAL;

            if (isCrit)
            {
                text = DamageResultType.RESULT_CRITICAL;
            }

            TakeDamage(damageData, text);
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
        /// Function called when this unit's health drops to 0 or less.
        /// </summary>
        /// <param name="data">Data of the death.</param>
        public virtual void Die(IDeathData data)
        {
            _game.ObjectManager.StopTargeting(this);

            if (!IsToRemove())
            {
                _game.PacketNotifier.NotifyS2C_NPC_Die_MapView(data);
            }

            SetToRemove();

            ApiEventManager.OnDeath.Publish(data);
            if (data.Unit is IObjAiBase obj)
            {
                if (!(obj is IMonster))
                {
                    var champs = _game.ObjectManager.GetChampionsInRangeFromTeam(Position, _game.Map.MapScript.MapScriptMetadata.ExpRange, Team, true);
                    if (champs.Count > 0)
                    {
                        var expPerChamp = obj.Stats.ExpGivenOnDeath.Total / champs.Count;
                        foreach (var c in champs)
                        {
                            c.AddExperience(expPerChamp);
                        }
                    }
                }
            }

            if (data.Killer != null && data.Killer is IChampion champion)
            {
                //Monsters give XP exclusively to the killer
                if (data.Unit is IMonster)
                {
                    champion.AddExperience(data.Unit.Stats.ExpGivenOnDeath.Total);
                }

                champion.OnKill(data);
            }

            _game.PacketNotifier.NotifyDeath(data);
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
            Model = model;
            IsModelUpdated = true;
            return true;
        }

        /// <summary>
        /// Gets the movement speed stat of this unit (units/sec).
        /// </summary>
        /// <returns>Float units/sec.</returns>
        public float GetMoveSpeed()
        {
            if (MovementParameters != null)
            {
                return MovementParameters.PathSpeedOverride;
            }

            return Stats.MoveSpeed.Total;
        }

        /// <summary>
        /// Enables or disables the given status on this unit.
        /// </summary>
        /// <param name="status">StatusFlag to enable/disable.</param>
        /// <param name="enabled">Whether or not to enable the flag.</param>
        public void SetStatus(StatusFlags status, bool enabled)
        {
            // Loop over all possible status flags and set them individually.
            for (int i = 0; i < Enum.GetNames(typeof(StatusFlags)).Length - 1; i++)
            {
                StatusFlags currentFlag = (StatusFlags)(1 << i);

                if (status.HasFlag(currentFlag))
                {
                    if (enabled)
                    {
                        Status |= currentFlag;
                    }
                    else
                    {
                        Status &= ~currentFlag;
                    }

                    switch (currentFlag)
                    {
                        // CallForHelpSuppressor
                        case StatusFlags.CanAttack:
                        {
                            Stats.SetActionState(ActionState.CAN_ATTACK, enabled);
                            break;
                        }
                        case StatusFlags.CanCast:
                        {
                            Stats.SetActionState(ActionState.CAN_CAST, enabled);
                            break;
                        }
                        case StatusFlags.CanMove:
                        {
                            Stats.SetActionState(ActionState.CAN_MOVE, enabled);
                            break;
                        }
                        case StatusFlags.CanMoveEver:
                        {
                            Stats.SetActionState(ActionState.CAN_NOT_MOVE, !enabled);
                            break;
                        }
                        case StatusFlags.Charmed:
                        {
                            Stats.SetActionState(ActionState.CHARMED, enabled);
                            break;
                        }
                        // DisableAmbientGold
                        case StatusFlags.Feared:
                        {
                            Stats.SetActionState(ActionState.FEARED, enabled);
                            // TODO: Verify
                            Stats.SetActionState(ActionState.IS_FLEEING, enabled);
                            break;
                        }
                        case StatusFlags.ForceRenderParticles:
                        {
                            Stats.SetActionState(ActionState.FORCE_RENDER_PARTICLES, enabled);
                            break;
                        }
                        // GhostProof
                        case StatusFlags.Ghosted:
                        {
                            Stats.SetActionState(ActionState.IS_GHOSTED, enabled);
                            break;
                        }
                        // IgnoreCallForHelp
                        // Immovable
                        // Invulnerable
                        // MagicImmune
                        case StatusFlags.NearSighted:
                        {
                            Stats.SetActionState(ActionState.IS_NEAR_SIGHTED, enabled);
                            break;
                        }
                        // Netted
                        case StatusFlags.NoRender:
                        {
                            Stats.SetActionState(ActionState.NO_RENDER, enabled);
                            break;
                        }
                        // PhysicalImmune
                        case StatusFlags.RevealSpecificUnit:
                        {
                            Stats.SetActionState(ActionState.REVEAL_SPECIFIC_UNIT, enabled);
                            break;
                        }
                        // Rooted
                        // Silenced
                        case StatusFlags.Sleep:
                        {
                            Stats.SetActionState(ActionState.IS_ASLEEP, enabled);
                            break;
                        }
                        case StatusFlags.Stealthed:
                        {
                            Stats.SetActionState(ActionState.STEALTHED, enabled);
                            break;
                        }
                        // SuppressCallForHelp
                        case StatusFlags.Targetable:
                        {
                            Stats.IsTargetable = enabled;
                            // TODO: Refactor this.
                            if (this is IObjAiBase obj && !obj.CharData.IsUseable)
                            {
                                Stats.SetActionState(ActionState.TARGETABLE, enabled);
                            }
                            break;
                        }
                        case StatusFlags.Taunted:
                        {
                            Stats.SetActionState(ActionState.TAUNTED, enabled);
                            break;
                        }
                    }
                }
            }

            if (!Status.HasFlag(StatusFlags.CanMove)
                || Status.HasFlag(StatusFlags.Charmed)
                || Status.HasFlag(StatusFlags.Feared)
                || Status.HasFlag(StatusFlags.Immovable)
                || Status.HasFlag(StatusFlags.Netted)
                || Status.HasFlag(StatusFlags.Rooted)
                || Status.HasFlag(StatusFlags.Sleep)
                || Status.HasFlag(StatusFlags.Stunned)
                || Status.HasFlag(StatusFlags.Suppressed)
                || Status.HasFlag(StatusFlags.Taunted))
            {
                Stats.SetActionState(ActionState.CAN_NOT_MOVE, true);
            }
            else if (Stats.GetActionState(ActionState.CAN_NOT_MOVE))
            {
                Stats.SetActionState(ActionState.CAN_NOT_MOVE, false);
            }

            if (!(Status.HasFlag(StatusFlags.CanAttack)
                    && !Status.HasFlag(StatusFlags.Charmed)
                    && !Status.HasFlag(StatusFlags.Disarmed)
                    && !Status.HasFlag(StatusFlags.Feared)
                    // TODO: Verify
                    && !Status.HasFlag(StatusFlags.Pacified)
                    && !Status.HasFlag(StatusFlags.Sleep)
                    && !Status.HasFlag(StatusFlags.Stunned)
                    && !Status.HasFlag(StatusFlags.Suppressed)))
            {
                Stats.SetActionState(ActionState.CAN_NOT_ATTACK, true);
            }
            else if (Stats.GetActionState(ActionState.CAN_NOT_ATTACK))
            {
                Stats.SetActionState(ActionState.CAN_NOT_ATTACK, false);
            }
        }

        public void UpdateStatus()
        {
            // Combine the status effects of all the buffs
            Dictionary<StatusFlags, bool> finalEffects = new Dictionary<StatusFlags, bool>();
            foreach (IBuff buff in GetBuffs())
            {
                foreach (KeyValuePair<StatusFlags, bool> effect in buff.StatusEffects)
                {
                    if (finalEffects.ContainsKey(effect.Key))
                    {
                        // If the effect should be enabled, it overrides disable.
                        if (finalEffects[effect.Key])
                        {
                            continue;
                        }
                        else
                        {
                            finalEffects[effect.Key] = effect.Value;
                        }
                    }
                    else
                    {
                        finalEffects.Add(effect.Key, effect.Value);
                    }
                }
            }

            // Set the status effects of this unit.
            foreach (KeyValuePair<StatusFlags, bool> effect in finalEffects)
            {
                SetStatus(effect.Key, effect.Value);
            }
        }

        /// <summary>
        /// Teleports this unit to the given position, and optionally repaths from the new position.
        /// </summary>
        /// <param name="x">X coordinate to teleport to.</param>
        /// <param name="y">Y coordinate to teleport to.</param>
        /// <param name="repath">Whether or not to repath from the new position.</param>
        public void TeleportTo(float x, float y, bool repath = false)
        {
            TeleportTo(new Vector2(x, y), repath);
        }

        /// <summary>
        /// Teleports this unit to the given position, and optionally repaths from the new position.
        /// </summary>
        public void TeleportTo(Vector2 position, bool repath = false)
        {
            position = _game.Map.NavigationGrid.GetClosestTerrainExit(position, PathfindingRadius + 1.0f);

            if (repath)
            {
                SetPosition(position, true);
            }
            else
            {
                Position = position;
                ResetWaypoints();
            }

            TeleportID++;
            _game.PacketNotifier.NotifyWaypointGroup(this, useTeleportID: true);
            _movementUpdated = false;
        }

        /// <summary>
        /// Moves this unit to its specified waypoints, updating its position along the way.
        /// </summary>
        /// <param name="diff">The amount of milliseconds the unit is supposed to move</param>
        /// TODO: Implement interpolation (assuming all other desync related issues are already fixed).
        public virtual bool Move(float diff)
        {
            // current -> next positions
            var cur = new Vector2(Position.X, Position.Y);
            var next = CurrentWaypoint.Value;

            var goingTo = next - cur;

            var dirTemp = Vector2.Normalize(goingTo);

            // usually doesn't happen
            if (float.IsNaN(dirTemp.X) || float.IsNaN(dirTemp.Y))
            {
                dirTemp = new Vector2(0, 0);
            }

            Direction = new Vector3(dirTemp.X, 0.0f, dirTemp.Y);

            //TODO: Turns in the direction of travel automatically, no need to call.
            if (MovementParameters != null && !MovementParameters.KeepFacingDirection)
            {
                FaceDirection(Direction, false);
            }

            var moveSpeed = GetMoveSpeed();

            var distSqr = MathF.Abs(Vector2.DistanceSquared(cur, next));

            var deltaMovement = moveSpeed * 0.001f * diff;

            // Prevent moving past the next waypoint.
            if (deltaMovement * deltaMovement > distSqr)
            {
                deltaMovement = MathF.Sqrt(distSqr);
            }

            var xx = Direction.X * deltaMovement;
            var yy = Direction.Z * deltaMovement;

            Vector2 nextPos = new Vector2(Position.X + xx, Position.Y + yy);
            // TODO: Implement ForceMovementType so this specifically applies to dashes that can't move past walls.
            if (MovementParameters == null)
            {
                // Prevent moving past obstacles. TODO: Verify if works at high speeds.
                // TODO: Implement range based (CollisionRadius) pathfinding so we don't keep getting stuck because of IsAnythingBetween.
                // TODO: After the above, implement repathing if our position within the next tick or two will intersect with another GameObject.
                KeyValuePair<bool, Vector2> pathBlocked = _game.Map.NavigationGrid.IsAnythingBetween(Position, nextPos);
                if (pathBlocked.Key)
                {
                    nextPos = _game.Map.NavigationGrid.GetClosestTerrainExit(pathBlocked.Value, PathfindingRadius + 1.0f);
                }
            }

            Position = nextPos;

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
                    ResetWaypoints();

                    if (MovementParameters != null)
                    {
                        SetDashingState(false);
                        return true;
                    }

                    return true;
                }
                // start moving to our next waypoint
                else
                {
                    CurrentWaypoint = new KeyValuePair<int, Vector2>(nextIndex, Waypoints[nextIndex]);
                }
            }

            return true;
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
        /// Resets this unit's waypoints.
        /// </summary>
        public void ResetWaypoints()
        {
            Waypoints = new List<Vector2> { Position };
            CurrentWaypoint = new KeyValuePair<int, Vector2>(1, Position);
        }

        /// <summary>
        /// Returns whether this unit has reached the last waypoint in its path of waypoints.
        /// </summary>
        public bool IsPathEnded()
        {
            return CurrentWaypoint.Key >= Waypoints.Count;
        }

        /// <summary>
        /// Sets this unit's movement path to the given waypoints. *NOTE*: Requires current position to be prepended.
        /// </summary>
        /// <param name="newWaypoints">New path of Vector2 coordinates that the unit will move to.</param>
        /// <param name="networked">Whether or not clients should be notified of this change in waypoints at the next ObjectManager.Update.</param>
        public void SetWaypoints(List<Vector2> newWaypoints, bool networked = true)
        {
            // Waypoints should always have an origin at the current position.
            // Dashes are excluded as their paths should be set before being applied.
            // TODO: Find out the specific cases where we shouldn't be able to set our waypoints. Perhaps CC?
            // Setting waypoints during auto attacks is allowed.
            if (newWaypoints == null || newWaypoints.Count <= 1 || newWaypoints[0] != Position || !CanChangeWaypoints())
            {
                return;
            }

            if (networked)
            {
                _movementUpdated = true;
            }
            Waypoints = newWaypoints;
            CurrentWaypoint = new KeyValuePair<int, Vector2>(1, Waypoints[1]);
        }

        /// <summary>
        /// Forces this unit to stop moving.
        /// </summary>
        public virtual void StopMovement()
        {
            // Stop movements are always networked.
            _movementUpdated = true;

            if (MovementParameters != null)
            {
                SetDashingState(false);
                return;
            }

            ResetWaypoints();
        }

        /// <summary>
        /// Returns whether this unit's waypoints will be networked to clients the next update. Movement updates do not occur for dash based movements.
        /// </summary>
        /// <returns>True/False</returns>
        /// TODO: Refactor movement update logic so this can be applied to any kind of movement.
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
        /// Adds the given buff instance to this unit.
        /// </summary>
        /// <param name="b">Buff instance to add.</param>
        /// TODO: Probably needs a refactor to lessen thread usage. Make sure to stick very closely to the current method; just optimize it.
        public void AddBuff(IBuff b)
        {
            if (ApiEventManager.OnAllowAddBuff.Publish(this, b.SourceUnit, b))
            {
                // If this is the first buff of this name to be added, then add it to the parent buffs list (regardless of its add type).
                if (!ParentBuffs.ContainsKey(b.Name))
                {
                    // If the parent buff has ended, make the next oldest buff the parent buff.
                    if (HasBuff(b.Name))
                    {
                        var buff = GetBuffsWithName(b.Name)[0];
                        ParentBuffs.Add(b.Name, buff);
                        return;
                    }
                    // If there is no other buffs of this name, make it the parent and add it normally.
                    ParentBuffs.Add(b.Name, b);
                    BuffList.Add(b);
                    // Add the buff to the visual hud.
                    if (!b.IsHidden)
                    {
                        _game.PacketNotifier.NotifyNPC_BuffAdd2(b);
                    }
                    // Activate the buff for BuffScripts
                    b.ActivateBuff();
                }
                // If the buff is supposed to replace any existing buff instances of the same name
                else if (b.BuffAddType == BuffAddType.REPLACE_EXISTING)
                {
                    // Removing the previous buff of the same name.
                    var prevbuff = ParentBuffs[b.Name];

                    prevbuff.DeactivateBuff();
                    RemoveBuff(b.Name, false);

                    // Clear the newly given buff's slot since we will move it into the previous buff's slot.
                    RemoveBuffSlot(b);

                    // Adding the newly given buff instance into the slot of the previous buff.
                    BuffSlots[prevbuff.Slot] = b;
                    b.SetSlot(prevbuff.Slot);

                    // Add the buff as a parent and normally.
                    ParentBuffs.Add(b.Name, b);
                    BuffList.Add(b);

                    // Update the visual buff in-game (usually just resets the buff time of the visual icon).
                    if (!b.IsHidden)
                    {
                        _game.PacketNotifier.NotifyNPC_BuffReplace(b);
                    }

                    // New buff means new script, so we need to activate it.
                    b.ActivateBuff();
                }
                else if (b.BuffAddType == BuffAddType.RENEW_EXISTING)
                {
                    // Clear the newly created buff's slot so we aren't wasting slots.
                    if (b != ParentBuffs[b.Name])
                    {
                        RemoveBuffSlot(b);
                    }

                    // Reset the already existing buff's timer.
                    ParentBuffs[b.Name].ResetTimeElapsed();

                    // Update the visual buff in-game (just resets the time on the icon).
                    if (!b.IsHidden)
                    {
                        _game.PacketNotifier.NotifyNPC_BuffReplace(ParentBuffs[b.Name]);
                    }
                }
                // If the buff is supposed to be a single stackable buff with a timer = Duration * StackCount
                else if (b.BuffAddType == BuffAddType.STACKS_AND_CONTINUE)
                {
                    // If we've hit the max stacks count for this buff add type
                    if (ParentBuffs[b.Name].StackCount >= ParentBuffs[b.Name].MaxStacks)
                    {
                        ParentBuffs[b.Name].ResetTimeElapsed();

                        if (!b.IsHidden)
                        {
                            // If the buff is a counter buff (ex: Nasus Q stacks), then use a packet specialized for big buff stack counts (int.MaxValue).
                            if (ParentBuffs[b.Name].BuffType == BuffType.COUNTER)
                            {
                                _game.PacketNotifier.NotifyNPC_BuffUpdateNumCounter(ParentBuffs[b.Name]);
                            }
                            // Otherwise, use the normal buff stack (254) update (usually just adds one to the number on the icon and refreshes the time of the icon).
                            else
                            {
                                _game.PacketNotifier.NotifyNPC_BuffUpdateCount(ParentBuffs[b.Name], ParentBuffs[b.Name].Duration, ParentBuffs[b.Name].TimeElapsed);
                            }
                        }

                        return;
                    }

                    ParentBuffs[b.Name].IncrementStackCount();

                    if (!b.IsHidden)
                    {
                        _game.PacketNotifier.NotifyNPC_BuffUpdateCount(ParentBuffs[b.Name], ParentBuffs[b.Name].Duration - ParentBuffs[b.Name].TimeElapsed, ParentBuffs[b.Name].TimeElapsed);
                    }
                }
                // If the buff is supposed to be applied alongside any existing buff instances of the same name.
                else if (b.BuffAddType == BuffAddType.STACKS_AND_OVERLAPS)
                {
                    // If we've hit the max stacks count for this buff add type (usually 254 for this BuffAddType).
                    if (ParentBuffs[b.Name].StackCount >= ParentBuffs[b.Name].MaxStacks)
                    {
                        // Get and remove the oldest buff of the same name so we can free up space for the newly given buff instance.
                        var oldestbuff = ParentBuffs[b.Name];

                        oldestbuff.DeactivateBuff();
                        RemoveBuff(b.Name, true);

                        // Move the next oldest buff of the same name into the position of the removed oldest buff.
                        var tempbuffs = GetBuffsWithName(b.Name);

                        BuffSlots[oldestbuff.Slot] = tempbuffs[0];
                        ParentBuffs.Add(oldestbuff.Name, tempbuffs[0]);
                        BuffList.Add(b);

                        if (!b.IsHidden)
                        {
                            // If the buff is a counter buff (ex: Nasus Q stacks), then use a packet specialized for big buff stack counts (int.MaxValue).
                            if (ParentBuffs[b.Name].BuffType == BuffType.COUNTER)
                            {
                                _game.PacketNotifier.NotifyNPC_BuffUpdateNumCounter(ParentBuffs[b.Name]);
                            }
                            // Otherwise, use the normal buff stack (254) update (usually just adds one to the number on the icon and refreshes the time of the icon).
                            else
                            {
                                _game.PacketNotifier.NotifyNPC_BuffUpdateCount(b, b.Duration, b.TimeElapsed);
                            }
                        }
                        b.ActivateBuff();

                        return;
                    }
                    // If we haven't hit the max stack count (usually 254).
                    BuffList.Add(b);

                    // Increment the number of stacks on the parent buff, which is the buff instance which is used for packets.
                    ParentBuffs[b.Name].IncrementStackCount();

                    // Increment the number of stacks on every buff of the same name (so if any of them become the parent, there is no problem).
                    GetBuffsWithName(b.Name).ForEach(buff => buff.SetStacks(ParentBuffs[b.Name].StackCount));

                    if (!b.IsHidden)
                    {
                        if (b.BuffType == BuffType.COUNTER)
                        {
                            _game.PacketNotifier.NotifyNPC_BuffUpdateNumCounter(ParentBuffs[b.Name]);
                        }
                        else
                        {
                            _game.PacketNotifier.NotifyNPC_BuffUpdateCount(b, b.Duration, b.TimeElapsed);
                        }
                    }
                    b.ActivateBuff();
                }
                // If the buff is supposed to add a stack to any existing buffs of the same name and refresh their timer.
                // Essentially the method is: have one parent buff which has the stacks, and just refresh its time, this means no overlapping buff instances, but functionally it is the same.
                else if (ParentBuffs[b.Name].BuffAddType == BuffAddType.STACKS_AND_RENEWS)
                {
                    // Don't need the newly added buff instance as we already have a parent who we can add stacks to.
                    RemoveBuffSlot(b);

                    // Refresh the time of the parent buff and adds a stack if Max Stacks wasn't reached.
                    ParentBuffs[b.Name].ResetTimeElapsed();
                    if (ParentBuffs[b.Name].IncrementStackCount())
                    {
                        ParentBuffs[b.Name].ActivateBuff();
                    }

                    if (!b.IsHidden)
                    {
                        if (ParentBuffs[b.Name].BuffType == BuffType.COUNTER)
                        {
                            _game.PacketNotifier.NotifyNPC_BuffUpdateNumCounter(ParentBuffs[b.Name]);
                        }
                        else
                        {
                            _game.PacketNotifier.NotifyNPC_BuffUpdateCount(ParentBuffs[b.Name], ParentBuffs[b.Name].Duration, ParentBuffs[b.Name].TimeElapsed);
                        }
                    }
                    // TODO: Unload and reload all data of buff script here.
                }
            }
        }

        /// <summary>
        /// Whether or not this unit has the given buff instance.
        /// </summary>
        /// <param name="buff">Buff instance to check.</param>
        /// <returns>True/False.</returns>
        public bool HasBuff(IBuff buff)
        {
            if (BuffList == null)
            {
                return false;
            }

            return !(BuffList.Find(b => b == buff) == null);
        }

        /// <summary>
        /// Whether or not this unit has a buff of the given name.
        /// </summary>
        /// <param name="buffName">Internal buff name to check for.</param>
        /// <returns>True/False.</returns>
        public bool HasBuff(string buffName)
        {
            if (BuffList == null)
            {
                return false;
            }

            return !(BuffList.Find(b => b.IsBuffSame(buffName)) == null);
        }

        /// <summary>
        /// Whether or not this unit has a buff of the given type.
        /// </summary>
        /// <param name="type">BuffType to check for.</param>
        /// <returns>True/False.</returns>
        public bool HasBuffType(BuffType type)
        {
            if (BuffList == null)
            {
                return false;
            }

            return !(BuffList.Find(b => b.BuffType == type) == null);
        }

        /// <summary>
        /// Gets a new buff slot for the given buff instance.
        /// </summary>
        /// <param name="b">Buff instance to add.</param>
        /// <returns>Byte buff slot of the given buff.</returns>
        public byte GetNewBuffSlot(IBuff b)
        {
            var slot = GetBuffSlot();
            BuffSlots[slot] = b;
            return slot;
        }

        /// <summary>
        /// Gets the slot of the given buff instance, or an open slot if no buff is given.
        /// </summary>
        /// <param name="buffToLookFor">Buff to check. Leave empty to get an empty slot.</param>
        /// <returns>Slot of the given buff or an empty slot.</returns>
        private byte GetBuffSlot(IBuff buffToLookFor = null)
        {
            for (byte i = 1; i < BuffSlots.Length; i++) // Find the first open slot or the slot corresponding to buff
            {
                if (BuffSlots[i] == buffToLookFor)
                {
                    return i;
                }
            }

            throw new Exception("No slot found with requested value"); // If no open slot or no corresponding slot
        }

        /// <summary>
        /// Gets the list of parent buffs applied to this unit.
        /// </summary>
        /// <returns>List of parent buffs.</returns>
        public Dictionary<string, IBuff> GetParentBuffs()
        {
            return ParentBuffs;
        }

        /// <summary>
        /// Gets the parent buff instance of the buffs of the given name. Parent buffs control stack count for buffs of the same name.
        /// </summary>
        /// <param name="name">Internal buff name to check.</param>
        /// <returns>Parent buff instance.</returns>
        public IBuff GetBuffWithName(string name)
        {
            lock (_buffsLock)
            {
                if (ParentBuffs.ContainsKey(name))
                {
                    return ParentBuffs[name];
                }
                return null;
            }
        }

        /// <summary>
        /// Gets a list of all buffs applied to this unit (parent and children).
        /// </summary>
        /// <returns>List of buff instances.</returns>
        public List<IBuff> GetBuffs()
        {
            return BuffList;
        }

        /// <summary>
        /// Gets the number of parent buffs applied to this unit.
        /// </summary>
        /// <returns>Number of parent buffs.</returns>
        public int GetBuffsCount()
        {
            return ParentBuffs.Count;
        }

        /// <summary>
        /// Gets a list of all buff instances of the given name (parent and children).
        /// </summary>
        /// <param name="buffName">Internal buff name to check.</param>
        /// <returns>List of buff instances.</returns>
        public List<IBuff> GetBuffsWithName(string buffName)
        {
            lock (_buffsLock)
            {
                return BuffList.FindAll(b => b.IsBuffSame(buffName));
            }
        }

        /// <summary>
        /// Removes the given buff from this unit. Called automatically when buff timers have finished.
        /// Buffs with BuffAddType.STACKS_AND_OVERLAPS are removed incrementally, meaning one instance removed per RemoveBuff call.
        /// Other BuffAddTypes are removed entirely, regardless of stacks. DecrementStackCount can be used as an alternative.
        /// </summary>
        /// <param name="b">Buff to remove.</param>
        public void RemoveBuff(IBuff b)
        {
            if (!HasBuff(b))
            {
                return;
            }

            lock (_buffsLock)
            {
                // If the buff is supposed to be a single stackable buff with a timer = Duration * StackCount, and their are more than one already present.
                if (b.BuffAddType == BuffAddType.STACKS_AND_CONTINUE && b.StackCount > 1)
                {
                    b.DecrementStackCount();

                    IBuff tempBuff = new Buff(_game, b.Name, b.Duration, b.StackCount, b.OriginSpell, b.TargetUnit, b.SourceUnit, b.IsBuffInfinite());

                    RemoveBuff(b.Name, true);

                    if (!b.IsHidden)
                    {
                        _game.PacketNotifier.NotifyNPC_BuffRemove2(b);
                    }

                    // Next oldest buff takes the place of the removed oldest buff; becomes parent buff.
                    BuffSlots[b.Slot] = tempBuff;
                    ParentBuffs.Add(b.Name, tempBuff);
                    BuffList.Add(tempBuff);

                    // Add the buff to the visual hud.
                    if (!b.IsHidden)
                    {
                        _game.PacketNotifier.NotifyNPC_BuffAdd2(tempBuff);
                    }
                    // Activate the buff for BuffScripts
                    tempBuff.ActivateBuff();
                }
                else if (b.BuffAddType == BuffAddType.STACKS_AND_RENEWS && b.StackCount > 1 && !b.Elapsed())
                {
                    b.DecrementStackCount();

                    if (!b.IsHidden)
                    {
                        _game.PacketNotifier.NotifyNPC_BuffUpdateCount(b, b.Duration - b.TimeElapsed, b.TimeElapsed);
                    }
                }
                // If the buff is supposed to be applied alongside other buffs of the same name, and their are more than one already present.
                else if (b.BuffAddType == BuffAddType.STACKS_AND_OVERLAPS && b.StackCount > 1)
                {
                    // Remove one stack and update the other buff instances of the same name
                    b.DecrementStackCount();

                    // TODO: Unload and reload all data of buff scripts here.

                    RemoveBuff(b.Name, true);

                    var tempbuffs = GetBuffsWithName(b.Name);

                    tempbuffs.ForEach(tempbuff => tempbuff.SetStacks(b.StackCount));

                    // Next oldest buff takes the place of the removed oldest buff; becomes parent buff.
                    BuffSlots[b.Slot] = tempbuffs[0];
                    ParentBuffs.Add(b.Name, tempbuffs[0]);

                    // Used in packets to maintain the visual buff icon's timer, as removing a stack from the icon can reset the timer.
                    var newestBuff = tempbuffs[tempbuffs.Count - 1];

                    if (!b.IsHidden)
                    {
                        if (b.BuffType == BuffType.COUNTER)
                        {
                            _game.PacketNotifier.NotifyNPC_BuffUpdateNumCounter(ParentBuffs[b.Name]);
                        }
                        else
                        {
                            if (b.StackCount == 1)
                            {
                                _game.PacketNotifier.NotifyNPC_BuffUpdateCount(newestBuff, b.Duration - newestBuff.TimeElapsed, newestBuff.TimeElapsed);
                            }
                            else
                            {
                                _game.PacketNotifier.NotifyNPC_BuffUpdateCountGroup(this, tempbuffs, b.Duration - newestBuff.TimeElapsed, newestBuff.TimeElapsed);
                            }
                        }
                    }
                }
                // Only other case where RemoveBuff should be called is when there is one stack remaining on the buff.
                else
                {
                    if (!b.Elapsed())
                    {
                        b.DeactivateBuff();
                    }

                    RemoveBuff(b.Name, true);
                    BuffList.RemoveAll(buff => buff.Elapsed());
                    if (!b.IsHidden)
                    {
                        _game.PacketNotifier.NotifyNPC_BuffRemove2(b);
                    }
                }
            }
        }

        /// <summary>
        /// Removes the given buff instance from the buff slots of this unit.
        /// Called automatically by RemoveBuff().
        /// </summary>
        /// <param name="b">Buff instance to check for.</param>
        private void RemoveBuffSlot(IBuff b)
        {
            var slot = GetBuffSlot(b);
            BuffSlots[slot] = null;
        }

        /// <summary>
        /// Removes the parent buff of the given internal name from this unit.
        /// </summary>
        /// <param name="b">Internal buff name to remove.</param>
        private void RemoveBuff(string b, bool removeSlot)
        {
            if (removeSlot && ParentBuffs[b] != null)
            {
                RemoveBuffSlot(ParentBuffs[b]);
            }
            lock (_buffsLock)
            {
                BuffList.Remove(ParentBuffs[b]);
                ParentBuffs.Remove(b);
            }
        }

        /// <summary>
        /// Removes all buffs of the given internal name from this unit regardless of stack count.
        /// Intended mainly for buffs with BuffAddType.STACKS_AND_OVERLAPS.
        /// </summary>
        /// <param name="buffName">Internal buff name to remove.</param>
        public void RemoveBuffsWithName(string buffName)
        {
            lock (_buffsLock)
            {
                BuffList.FindAll(b =>
                b.IsBuffSame(buffName)).ForEach(b =>
                b.DeactivateBuff());
            }
        }

        /// <summary>
        /// Forces this unit to perform a dash which ends at the given position.
        /// </summary>
        /// <param name="endPos">Position to end the dash at.</param>
        /// <param name="dashSpeed">Amount of units the dash should travel in a second (movespeed).</param>
        /// <param name="animation">Internal name of the dash animation.</param>
        /// <param name="leapGravity">Optionally how much gravity the unit will experience when above the ground while dashing.</param>
        /// <param name="keepFacingLastDirection">Whether or not the AI unit should face the direction they were facing before the dash.</param>
        /// <param name="consideredCC">Whether or not to prevent movement, casting, or attacking during the duration of the movement.</param>
        /// TODO: Find a good way to grab these variables from spell data.
        /// TODO: Verify if we should count Dashing as a form of Crowd Control.
        /// TODO: Implement Dash class which houses these parameters, then have that as the only parameter to this function (and other Dash-based functions).
        public void DashToLocation(Vector2 endPos, float dashSpeed, string animation = "", float leapGravity = 0.0f, bool keepFacingLastDirection = true, bool consideredCC = true)
        {
            var newCoords = _game.Map.NavigationGrid.GetClosestTerrainExit(endPos, PathfindingRadius + 1.0f);

            // False because we don't want this to be networked as a normal movement.
            SetWaypoints(new List<Vector2> { Position, newCoords }, false);

            // TODO: Take into account the rest of the arguments
            MovementParameters = new ForceMovementParameters
            {
                SetStatus = StatusFlags.None,
                ElapsedTime = 0,
                PathSpeedOverride = dashSpeed,
                ParabolicGravity = leapGravity,
                ParabolicStartPoint = Position,
                KeepFacingDirection = keepFacingLastDirection,
                FollowNetID = 0,
                FollowDistance = 0,
                FollowBackDistance = 0,
                FollowTravelTime = 0
            };

            if (consideredCC)
            {
                MovementParameters.SetStatus = StatusFlags.CanAttack | StatusFlags.CanCast | StatusFlags.CanMove;
            }

            SetDashingState(true);

            if (animation != null && animation != "")
            {
                var animPairs = new Dictionary<string, string> { { "RUN", animation } };
                SetAnimStates(animPairs);
            }

            // Movement is networked this way instead.
            // TODO: Verify if we want to use NotifyWaypointListWithSpeed instead as it does not require conversions.
            //_game.PacketNotifier.NotifyWaypointListWithSpeed(this, dashSpeed, leapGravity, keepFacingLastDirection, null, 0, 0, 20000.0f);
            _game.PacketNotifier.NotifyWaypointGroupWithSpeed(this);
            _movementUpdated = false;
        }

        /// <summary>
        /// Sets this unit's current dash state to the given state.
        /// </summary>
        /// <param name="state">State to set. True = dashing, false = not dashing.</param>
        /// <param name="setStatus">Whether or not to modify movement, casting, and attacking states.</param>
        /// TODO: Implement ForcedMovement methods and enumerators to handle different kinds of dashes.
        public virtual void SetDashingState(bool state, MoveStopReason reason = MoveStopReason.Finished)
        {
            // TODO: Implement this as a parameter.
            SetStatus(MovementParameters.SetStatus, !state);

            if (MovementParameters != null && state == false)
            {
                MovementParameters = null;

                var animPairs = new Dictionary<string, string> { { "RUN", "" } };
                SetAnimStates(animPairs);

                ApiEventManager.OnMoveEnd.Publish(this);

                if (reason == MoveStopReason.Finished)
                {
                    ApiEventManager.OnMoveSuccess.Publish(this);
                }
                else if (reason != MoveStopReason.Finished)
                {
                    ApiEventManager.OnMoveFailure.Publish(this);
                }
            }
        }

        /// <summary>
        /// Sets this unit's animation states to the given set of states.
        /// Given state pairs are expected to follow a specific structure:
        /// First string is the animation to override, second string is the animation to play in place of the first.
        /// <param name="animPairs">Dictionary of animations to set.</param>
        public void SetAnimStates(Dictionary<string, string> animPairs)
        {
            if (animPairs != null)
            {
                _game.PacketNotifier.NotifyS2C_SetAnimStates(this, animPairs);
            }
        }
    }
}
