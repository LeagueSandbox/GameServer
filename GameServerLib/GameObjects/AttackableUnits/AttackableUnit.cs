using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using GameServerCore;
using GameServerCore.Content;
using GameServerCore.Enums;
using GameServerLib.Content;
using GameServerLib.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.Logging;
using log4net;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.Content;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Missile;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Sector;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.Buildings;

namespace LeagueSandbox.GameServer.GameObjects.AttackableUnits
{
    /// <summary>
    /// Base class for all attackable units.
    /// AttackableUnits normally follow these guidelines of functionality: Death state, forced movements, Crowd Control, Stats (including modifiers and basic replication), Buffs (and their scripts), and Call for Help.
    /// </summary>
    public class AttackableUnit : GameObject
    {
        // Crucial Vars.
        private float _statUpdateTimer;
        private object _buffsLock;
        private DeathData _death;
        private static ILog _logger = LoggerProvider.GetLogger();

        //TODO: Find out where this variable came from and if it can be unhardcoded
        internal const float DETECT_RANGE = 475.0f;

        /// <summary>
        /// Variable containing all data about the this unit's current character such as base health, base mana, whether or not they are melee, base movespeed, per level stats, etc.
        /// </summary>
        public CharData CharData { get; }
        /// <summary>
        /// Whether or not this Unit is dead. Refer to TakeDamage() and Die().
        /// </summary>
        public bool IsDead { get; protected set; }
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
        public Replication Replication { get; protected set; }
        /// <summary>
        /// Variable housing all of this Unit's stats such as health, mana, armor, magic resist, ActionState, etc.
        /// Currently these are only initialized manually by ObjAIBase and ObjBuilding.
        /// </summary>
        public Stats Stats { get; protected set; }
        /// <summary>
        /// Variable which stores the number of times a unit has teleported. Used purely for networking.
        /// Resets when reaching byte.MaxValue (255).
        /// </summary>
        public byte TeleportID { get; set; }
        /// <summary>
        /// Array of buff slots which contains all parent buffs (oldest buff of a given name) applied to this AI.
        /// Maximum of 256 slots, hard limit due to packets.
        /// </summary>
        private Buff[] BuffSlots { get; }
        /// <summary>
        /// Dictionary containing all parent buffs (oldest buff of a given name). Used for packets and assigning stacks if a buff of the same name is added.
        /// </summary>
        private Dictionary<string, Buff> ParentBuffs { get; }
        /// <summary>
        /// List of all buffs applied to this AI. Used for easier indexing of buffs.
        /// </summary>
        /// TODO: Verify if we can remove this in favor of BuffSlots while keeping the functions which allow for easy accessing of individual buff instances.
        private List<Buff> BuffList { get; }

        /// <summary>
        /// Waypoints that make up the path a game object is walking in.
        /// </summary>
        public List<Vector2> Waypoints { get; protected set; }
        /// <summary>
        /// Index of the waypoint in the list of waypoints that the object is currently on.
        /// </summary>
        public int CurrentWaypointKey { get; protected set; }
        public Vector2 CurrentWaypoint
        {
            get { return Waypoints[CurrentWaypointKey]; }
        }
        public bool PathHasTrueEnd { get; private set; } = false;
        public Vector2 PathTrueEnd { get; private set; }

        /// <summary>
        /// Status effects enabled on this unit. Refer to StatusFlags enum.
        /// </summary>
        public StatusFlags Status { get; private set; }
        private StatusFlags _statusBeforeApplyingBuffEfects = 0;
        private StatusFlags _buffEffectsToEnable = 0;
        private StatusFlags _buffEffectsToDisable = 0;
        private StatusFlags _dashEffectsToDisable = 0;

        /// <summary>
        /// Parameters of any forced movements (dashes) this unit is performing.
        /// </summary>
        public ForceMovementParameters MovementParameters { get; protected set; }
        /// <summary>
        /// Information about this object's icon on the minimap.
        /// </summary>
        /// TODO: Move this to GameObject.
        public IconInfo IconInfo { get; protected set; }
        public override bool IsAffectedByFoW => true;
        public override bool SpawnShouldBeHidden => true;

        private bool _teleportedDuringThisFrame = false;

        public AttackableUnit(
            Game game,
            string model,
            int collisionRadius = 40,
            Vector2 position = new Vector2(),
            int visionRadius = 0,
            uint netId = 0,
            TeamId team = TeamId.TEAM_NEUTRAL,
            Stats stats = null
        ) : base(game, position, collisionRadius, collisionRadius, visionRadius, netId, team)

        {
            Model = model;
            CharData = _game.Config.ContentManager.GetCharData(Model);
            if (stats == null)
            {
                var charStats = new Stats();
                charStats.LoadStats(CharData);
                Stats = charStats;
            }
            else
            {
                Stats = stats;
            }

            Waypoints = new List<Vector2> { Position };
            CurrentWaypointKey = 1;
            SetStatus(
                StatusFlags.CanAttack | StatusFlags.CanCast |
                StatusFlags.CanMove | StatusFlags.CanMoveEver |
                StatusFlags.Targetable, true
            );
            MovementParameters = null;
            Stats.AttackSpeedMultiplier.BaseValue = 1.0f;

            _buffsLock = new object();
            BuffSlots = new Buff[256];
            ParentBuffs = new Dictionary<string, Buff>();
            BuffList = new List<Buff>();
            IconInfo = new IconInfo(_game, this);
        }

        /// <summary>
        /// Gets the HashString for this unit's model. Used for packets so clients know what data to load.
        /// </summary>
        /// <returns>Hashed string of this unit's model.</returns>
        public uint GetObjHash()
        {
            var gobj = "[Character]" + Model;

            // TODO: Account for any other units that have skins (requires skins to be implemented for those units)
            if (this is Champion c)
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

            // TODO: Verify how dashes are affected by teleports.
            //       Typically follow dashes are unaffected, but there may be edge cases e.g. LeeSin
            if (MovementParameters != null)
            {
                SetDashingState(false);
            }
            else if (IsPathEnded())
            {
                ResetWaypoints();
            }
            else
            {
                // Reevaluate our current path to account for the starting position being changed.
                if (repath)
                {
                    Vector2 safeExit = _game.Map.NavigationGrid.GetClosestTerrainExit(Waypoints.Last(), PathfindingRadius);
                    List<Vector2> safePath = _game.Map.PathingHandler.GetPath(Position, safeExit, PathfindingRadius);

                    // TODO: When using this safePath, sometimes we collide with the terrain again, so we use an unsafe path the next collision, however,
                    // sometimes we collide again before we can finish the unsafe path, so we end up looping collisions between safe and unsafe paths, never actually escaping (ex: sharp corners).
                    // This is a more fundamental issue where the pathfinding should be taking into account collision radius, rather than simply pathing from center of an object.
                    if (safePath != null)
                    {
                        SetWaypoints(safePath);
                    }
                    else
                    {
                        ResetWaypoints();
                    }
                }
                else
                {
                    Waypoints[0] = Position;
                }
            }
        }

        public override void Update(float diff)
        {
            UpdateBuffs(diff);

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

            if (CanMove())
            {
                float remainingFrameTime = diff;
                if (MovementParameters != null)
                {
                    remainingFrameTime = DashMove(diff);
                }
                if (MovementParameters == null)
                {
                    Move(remainingFrameTime);
                }
            }

            if (IsDead && _death != null)
            {
                Die(_death);
                _death = null;
            }
        }

        /// <summary>
        /// Called when this unit collides with the terrain or with another GameObject. Refer to CollisionHandler for exact cases.
        /// </summary>
        /// <param name="collider">GameObject that collided with this AI. Null if terrain.</param>
        /// <param name="isTerrain">Whether or not this AI collided with terrain.</param>
        public override void OnCollision(GameObject collider, bool isTerrain = false)
        {
            // We do not want to teleport out of missiles, sectors, owned regions, or buildings. Buildings in particular are already baked into the Navigation Grid.
            if (collider is SpellMissile || collider is SpellSector || collider is ObjBuilding || (collider is Region region && region.CollisionUnit == this))
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
                    || (collider is AttackableUnit unit &&
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

        public override void Sync(int userId, TeamId team, bool visible, bool forceSpawn = false)
        {
            base.Sync(userId, team, visible, forceSpawn);
            IconInfo.Sync(userId, visible, forceSpawn);
        }

        protected override void OnSync(int userId, TeamId team)
        {
            if (Replication.Changed)
            {
                _game.PacketNotifier.HoldReplicationDataUntilOnReplicationNotification(this, userId, true);
            }
            if (_movementUpdated)
            {
                _game.PacketNotifier.HoldMovementDataUntilWaypointGroupNotification(this, userId, _teleportedDuringThisFrame);
            }
        }

        public override void OnAfterSync()
        {
            Replication.MarkAsUnchanged();
            _teleportedDuringThisFrame = false;
            _movementUpdated = false;
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
        public void AddStatModifier(StatsModifier statModifier)
        {
            Stats.AddModifier(statModifier);
        }

        /// <summary>
        /// Removes the given stat modifier instance from this unit.
        /// </summary>
        /// <param name="statModifier">Stat modifier instance to remove.</param>
        public void RemoveStatModifier(StatsModifier statModifier)
        {
            Stats.RemoveModifier(statModifier);
        }

        public virtual void TakeHeal(AttackableUnit caster, float amount, IEventSource sourceScript = null)
        {
            Stats.CurrentHealth = Math.Clamp(Stats.CurrentHealth + amount, 0, Stats.HealthPoints.Total);
        }

        /// <summary>
        /// Applies damage to this unit.
        /// </summary>
        /// <param name="attacker">Unit that is dealing the damage.</param>
        /// <param name="damage">Amount of damage to deal.</param>
        /// <param name="type">Whether the damage is physical, magical, or true.</param>
        /// <param name="source">What the damage came from: attack, spell, summoner spell, or passive.</param>
        /// <param name="damageText">Type of damage the damage text should be.</param>
        public DamageData TakeDamage(AttackableUnit attacker, float damage, DamageType type, DamageSource source, DamageResultType damageText, IEventSource sourceScript = null)
        {
            //TODO: Make all TakeDamage functions return DamageData
            DamageData damageData = new DamageData
            {
                IsAutoAttack = source == DamageSource.DAMAGE_SOURCE_ATTACK,
                Attacker = attacker,
                Target = this,
                Damage = damage,
                PostMitigationDamage = Stats.GetPostMitigationDamage(damage, type, attacker),
                DamageSource = source,
                DamageType = type,
                DamageResultType = damageText
            };
            this.TakeDamage(damageData, damageText, sourceScript);
            return damageData;
        }

        DamageResultType Bool2Crit(bool isCrit)
        {
            if (isCrit)
            {
                return DamageResultType.RESULT_CRITICAL;
            }
            return DamageResultType.RESULT_NORMAL;
        }

        /// <summary>
        /// Applies damage to this unit.
        /// </summary>
        /// <param name="attacker">Unit that is dealing the damage.</param>
        /// <param name="damage">Amount of damage to deal.</param>
        /// <param name="type">Whether the damage is physical, magical, or true.</param>
        /// <param name="source">What the damage came from: attack, spell, summoner spell, or passive.</param>
        /// <param name="isCrit">Whether or not the damage text should be shown as a crit.</param>
        public DamageData TakeDamage(AttackableUnit attacker, float damage, DamageType type, DamageSource source, bool isCrit, IEventSource sourceScript = null)
        {
            return TakeDamage(attacker, damage, type, source, Bool2Crit(isCrit), sourceScript);
        }

        public void TakeDamage(DamageData damageData, bool isCrit, IEventSource sourceScript = null)
        {
            this.TakeDamage(damageData, Bool2Crit(isCrit));
        }

        /// <summary>
        /// Applies damage to this unit.
        /// </summary>
        /// <param name="attacker">Unit that is dealing the damage.</param>
        /// <param name="damage">Amount of damage to deal.</param>
        /// <param name="type">Whether the damage is physical, magical, or true.</param>
        /// <param name="source">What the damage came from: attack, spell, summoner spell, or passive.</param>
        /// <param name="damageText">Type of damage the damage text should be.</param>
        public virtual void TakeDamage(DamageData damageData, DamageResultType damageText, IEventSource sourceScript = null)
        {
            float healRatio = 0.0f;
            var attacker = damageData.Attacker;
            var attackerStats = damageData.Attacker.Stats;
            var type = damageData.DamageType;
            var source = damageData.DamageSource;
            var postMitigationDamage = damageData.PostMitigationDamage;

            ApiEventManager.OnPreTakeDamage.Publish(damageData.Target, damageData);

            if (GlobalData.SpellVampVariables.SpellVampRatios.TryGetValue(source, out float ratio) || source == DamageSource.DAMAGE_SOURCE_ATTACK)
            {
                switch (source)
                {
                    case DamageSource.DAMAGE_SOURCE_SPELL:
                    case DamageSource.DAMAGE_SOURCE_SPELLAOE:
                    case DamageSource.DAMAGE_SOURCE_SPELLPERSIST:
                    case DamageSource.DAMAGE_SOURCE_PERIODIC:
                    case DamageSource.DAMAGE_SOURCE_PROC:
                    case DamageSource.DAMAGE_SOURCE_REACTIVE:
                    case DamageSource.DAMAGE_SOURCE_ONDEATH:
                    case DamageSource.DAMAGE_SOURCE_PET:
                        healRatio = attackerStats.SpellVamp.Total * ratio;
                        break;
                    case DamageSource.DAMAGE_SOURCE_ATTACK:
                        healRatio = attackerStats.LifeSteal.Total;
                        break;
                }
            }

            if (!CanTakeDamage(type))
            {
                return;
            }

            Stats.CurrentHealth = Math.Max(0.0f, Stats.CurrentHealth - postMitigationDamage);

            ApiEventManager.OnTakeDamage.Publish(damageData.Target, damageData);

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

            if (attacker.Team != Team)
            {
                _game.PacketNotifier.NotifyUnitApplyDamage(damageData, _game.Config.IsDamageTextGlobal);
            }

            // Get health from lifesteal/spellvamp
            if (healRatio > 0)
            {
                attackerStats.CurrentHealth = Math.Min
                (
                    attackerStats.HealthPoints.Total,
                    attackerStats.CurrentHealth + healRatio * postMitigationDamage
                );
            }
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
        public virtual void Die(DeathData data)
        {
            _game.ObjectManager.StopTargeting(this);

            if (!IsToRemove())
            {
                _game.PacketNotifier.NotifyS2C_NPC_Die_MapView(data);
            }

            SetToRemove();

            ApiEventManager.OnDeath.Publish(data.Unit, data);
            if (data.Unit is ObjAIBase obj)
            {
                if (!(obj is Monster))
                {
                    var champs = _game.ObjectManager.GetChampionsInRangeFromTeam(Position, GlobalData.ObjAIBaseVariables.ExpRadius2, CustomConvert.GetEnemyTeam(Team), true);
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

            if (data.Killer != null && data.Killer is Champion champion)
            {
                //Monsters give XP exclusively to the killer
                if (data.Unit is Monster)
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
            _game.PacketNotifier.NotifyS2C_ChangeCharacterData(this);
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

            return Stats.GetTrueMoveSpeed();
        }

        /// <summary>
        /// Enables or disables the given status on this unit.
        /// </summary>
        /// <param name="status">StatusFlag to enable/disable.</param>
        /// <param name="enabled">Whether or not to enable the flag.</param>
        public void SetStatus(StatusFlags status, bool enabled)
        {
            if (enabled)
            {
                _statusBeforeApplyingBuffEfects |= status;
            }
            else
            {
                _statusBeforeApplyingBuffEfects &= ~status;
            }
            Status = (
                (
                    _statusBeforeApplyingBuffEfects
                    & ~_buffEffectsToDisable
                )
                | _buffEffectsToEnable
            )
            & ~_dashEffectsToDisable;

            UpdateActionState();
        }

        void UpdateActionState()
        {
            // CallForHelpSuppressor
            Stats.SetActionState(ActionState.CAN_ATTACK, Status.HasFlag(StatusFlags.CanAttack));
            Stats.SetActionState(ActionState.CAN_CAST, Status.HasFlag(StatusFlags.CanCast));
            Stats.SetActionState(ActionState.CAN_MOVE, Status.HasFlag(StatusFlags.CanMove));
            Stats.SetActionState(ActionState.CAN_NOT_MOVE, !Status.HasFlag(StatusFlags.CanMoveEver));
            Stats.SetActionState(ActionState.CHARMED, Status.HasFlag(StatusFlags.Charmed));
            // DisableAmbientGold

            bool feared = Status.HasFlag(StatusFlags.Feared);
            Stats.SetActionState(ActionState.FEARED, feared);
            // TODO: Verify
            Stats.SetActionState(ActionState.IS_FLEEING, feared);

            Stats.SetActionState(ActionState.FORCE_RENDER_PARTICLES, Status.HasFlag(StatusFlags.ForceRenderParticles));
            // GhostProof
            Stats.SetActionState(ActionState.IS_GHOSTED, Status.HasFlag(StatusFlags.Ghosted));
            // IgnoreCallForHelp
            // Immovable
            // Invulnerable
            // MagicImmune
            Stats.SetActionState(ActionState.IS_NEAR_SIGHTED, Status.HasFlag(StatusFlags.NearSighted));
            // Netted
            Stats.SetActionState(ActionState.NO_RENDER, Status.HasFlag(StatusFlags.NoRender));
            // PhysicalImmune
            Stats.SetActionState(ActionState.REVEAL_SPECIFIC_UNIT, Status.HasFlag(StatusFlags.RevealSpecificUnit));
            // Rooted
            // Silenced
            Stats.SetActionState(ActionState.IS_ASLEEP, Status.HasFlag(StatusFlags.Sleep));
            Stats.SetActionState(ActionState.STEALTHED, Status.HasFlag(StatusFlags.Stealthed));
            // SuppressCallForHelp

            bool targetable = Status.HasFlag(StatusFlags.Targetable);
            Stats.IsTargetable = targetable;
            // TODO: Refactor this.
            if (!CharData.IsUseable)
            {
                Stats.SetActionState(ActionState.TARGETABLE, targetable);
            }

            Stats.SetActionState(ActionState.TAUNTED, Status.HasFlag(StatusFlags.Taunted));

            Stats.SetActionState(
                ActionState.CAN_NOT_MOVE,
                !Status.HasFlag(StatusFlags.CanMove)
                || Status.HasFlag(StatusFlags.Charmed)
                || Status.HasFlag(StatusFlags.Feared)
                || Status.HasFlag(StatusFlags.Immovable)
                || Status.HasFlag(StatusFlags.Netted)
                || Status.HasFlag(StatusFlags.Rooted)
                || Status.HasFlag(StatusFlags.Sleep)
                || Status.HasFlag(StatusFlags.Stunned)
                || Status.HasFlag(StatusFlags.Suppressed)
                || Status.HasFlag(StatusFlags.Taunted)
            );

            Stats.SetActionState(
                ActionState.CAN_NOT_ATTACK,
                !Status.HasFlag(StatusFlags.CanAttack)
                || Status.HasFlag(StatusFlags.Charmed)
                || Status.HasFlag(StatusFlags.Disarmed)
                || Status.HasFlag(StatusFlags.Feared)
                // TODO: Verify
                || Status.HasFlag(StatusFlags.Pacified)
                || Status.HasFlag(StatusFlags.Sleep)
                || Status.HasFlag(StatusFlags.Stunned)
                || Status.HasFlag(StatusFlags.Suppressed)
            );
        }

        void UpdateBuffs(float diff)
        {
            // Combine the status effects of all the buffs
            _buffEffectsToEnable = 0;
            _buffEffectsToDisable = 0;

            var tempBuffs = new List<Buff>(BuffList);
            foreach (Buff buff in tempBuffs)
            {
                if (buff.Elapsed())
                {
                    RemoveBuff(buff);
                }
                else
                {
                    buff.Update(diff);

                    _buffEffectsToEnable |= buff.StatusEffectsToEnable;
                    _buffEffectsToDisable |= buff.StatusEffectsToDisable;
                }
            }

            // If the effect should be enabled, it overrides disable.
            _buffEffectsToDisable &= ~_buffEffectsToEnable;

            // Set the status effects of this unit.
            SetStatus(StatusFlags.None, true);
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
            TeleportID++;
            _movementUpdated = true;
            _teleportedDuringThisFrame = true;

            position = _game.Map.NavigationGrid.GetClosestTerrainExit(position, PathfindingRadius + 1.0f);

            if (repath)
            {
                SetPosition(position, true);
            }
            else
            {
                Position = position;
                StopMovement();
            }
        }

        private float DashMove(float frameTime)
        {
            var MP = MovementParameters;
            Vector2 dir;
            float distToDest;
            float distRemaining = float.PositiveInfinity;
            float timeRemaining = float.PositiveInfinity;
            if (MP.FollowNetID > 0)
            {
                GameObject unitToFollow = _game.ObjectManager.GetObjectById(MP.FollowNetID);
                if (unitToFollow == null)
                {
                    SetDashingState(false, MoveStopReason.LostTarget);
                    return frameTime;
                }
                dir = unitToFollow.Position - Position;
                float combinedRadius = PathfindingRadius + unitToFollow.PathfindingRadius;
                distToDest = Math.Max(0, dir.Length() - combinedRadius);
                if (MP.FollowDistance > 0)
                {
                    distRemaining = MP.FollowDistance - MP.PassedDistance;
                }
                if (MP.FollowTravelTime > 0)
                {
                    timeRemaining = MP.FollowTravelTime - MP.ElapsedTime;
                }
            }
            else
            {
                dir = Waypoints[1] - Position;
                distToDest = dir.Length();
            }
            distRemaining = Math.Min(distToDest, distRemaining);

            float time = Math.Min(frameTime, timeRemaining);
            float speed = MP.PathSpeedOverride * 0.001f;
            float distPerFrame = speed * time;
            float dist = Math.Min(distPerFrame, distRemaining);
            if (dir != Vector2.Zero)
            {
                Position += Vector2.Normalize(dir) * dist;
            }

            if (distRemaining <= distPerFrame)
            {
                SetDashingState(false);
                return (distPerFrame - distRemaining) / speed;
            }
            if (timeRemaining <= frameTime)
            {
                SetDashingState(false);
                return frameTime - timeRemaining;
            }
            MP.PassedDistance += dist;
            MP.ElapsedTime += time;

            return 0;
        }

        /// <summary>
        /// Moves this unit to its specified waypoints, updating its position along the way.
        /// </summary>
        /// <param name="diff">The amount of milliseconds the unit is supposed to move</param>
        /// TODO: Implement interpolation (assuming all other desync related issues are already fixed).
        public virtual bool Move(float delta)
        {
            if (CurrentWaypointKey < Waypoints.Count)
            {
                float speed = GetMoveSpeed() * 0.001f;
                var maxDist = speed * delta;

                while (true)
                {
                    var dir = CurrentWaypoint - Position;
                    var dist = dir.Length();

                    if (maxDist < dist)
                    {
                        Position += dir / dist * maxDist;
                        return true;
                    }
                    else
                    {
                        Position = CurrentWaypoint;
                        maxDist -= dist;

                        CurrentWaypointKey++;
                        if (CurrentWaypointKey == Waypoints.Count || maxDist == 0)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool PathTrueEndIs(Vector2 location)
        {
            return PathHasTrueEnd && PathTrueEnd == location;
        }

        public bool SetPathTrueEnd(Vector2 location)
        {
            if (PathTrueEndIs(location))
            {
                return true;
            }

            PathHasTrueEnd = true;
            PathTrueEnd = location;

            if (CanChangeWaypoints())
            {
                var nav = _game.Map.NavigationGrid;
                var path = nav.GetPath(Position, location, PathfindingRadius);
                if (path != null)
                {
                    SetWaypoints(path); // resets `PathHasTrueEnd`
                    PathHasTrueEnd = true;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Resets this unit's waypoints.
        /// </summary>
        private void ResetWaypoints()
        {
            Waypoints = new List<Vector2> { Position };
            CurrentWaypointKey = 1;

            PathHasTrueEnd = false;
        }

        /// <summary>
        /// Returns whether this unit has reached the last waypoint in its path of waypoints.
        /// </summary>
        public bool IsPathEnded()
        {
            return CurrentWaypointKey >= Waypoints.Count;
        }

        /// <summary>
        /// Sets this unit's movement path to the given waypoints. *NOTE*: Requires current position to be prepended.
        /// </summary>
        /// <param name="newWaypoints">New path of Vector2 coordinates that the unit will move to.</param>
        /// <param name="networked">Whether or not clients should be notified of this change in waypoints at the next ObjectManager.Update.</param>
        public bool SetWaypoints(List<Vector2> newWaypoints)
        {
            // Waypoints should always have an origin at the current position.
            // Dashes are excluded as their paths should be set before being applied.
            // TODO: Find out the specific cases where we shouldn't be able to set our waypoints. Perhaps CC?
            // Setting waypoints during auto attacks is allowed.
            if (newWaypoints == null || newWaypoints.Count <= 1 || newWaypoints[0] != Position || !CanChangeWaypoints())
            {
                return false;
            }

            _movementUpdated = true;
            Waypoints = newWaypoints;
            CurrentWaypointKey = 1;

            PathHasTrueEnd = false;

            return true;
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
        /// Adds the given buff instance to this unit.
        /// </summary>
        /// <param name="b">Buff instance to add.</param>
        /// TODO: Probably needs a refactor to lessen thread usage. Make sure to stick very closely to the current method; just optimize it.
        public virtual bool AddBuff(Buff b)
        {
            if (ApiEventManager.OnAllowAddBuff.Publish(this, (b.SourceUnit, b)))
            {
                // If this is the first buff of this name to be added, then add it to the parent buffs list (regardless of its add type).
                if (!ParentBuffs.ContainsKey(b.Name))
                {
                    // If the parent buff has ended, make the next oldest buff the parent buff.
                    if (HasBuff(b.Name))
                    {
                        var buff = GetBuffsWithName(b.Name)[0];
                        ParentBuffs.Add(b.Name, buff);
                    }
                    else
                    {
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
                    }
                    else
                    {
                        ParentBuffs[b.Name].IncrementStackCount();

                        if (!b.IsHidden)
                        {
                            _game.PacketNotifier.NotifyNPC_BuffUpdateCount(ParentBuffs[b.Name], ParentBuffs[b.Name].Duration - ParentBuffs[b.Name].TimeElapsed, ParentBuffs[b.Name].TimeElapsed);
                        }
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
                    }
                    else
                    {
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
                return true;
            }
            return false;
        }

        /// <summary>
        /// Whether or not this unit has the given buff instance.
        /// </summary>
        /// <param name="buff">Buff instance to check.</param>
        /// <returns>True/False.</returns>
        public bool HasBuff(Buff buff)
        {
            return BuffList != null && BuffList.Contains(buff);
        }

        /// <summary>
        /// Whether or not this unit has a buff of the given name.
        /// </summary>
        /// <param name="buffName">Internal buff name to check for.</param>
        /// <returns>True/False.</returns>
        public bool HasBuff(string buffName)
        {
            return BuffList.Find(b => b.IsBuffSame(buffName)) != null;
        }

        /// <summary>
        /// Whether or not this unit has a buff of the given type.
        /// </summary>
        /// <param name="type">BuffType to check for.</param>
        /// <returns>True/False.</returns>
        public bool HasBuffType(BuffType type)
        {
            return BuffList != null && BuffList.Find(b => b.BuffType == type) != null;
        }

        /// <summary>
        /// Gets a new buff slot for the given buff instance.
        /// </summary>
        /// <param name="b">Buff instance to add.</param>
        /// <returns>Byte buff slot of the given buff.</returns>
        public byte GetNewBuffSlot(Buff b)
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
        private byte GetBuffSlot(Buff buffToLookFor = null)
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
        public Dictionary<string, Buff> GetParentBuffs()
        {
            return ParentBuffs;
        }

        /// <summary>
        /// Gets the parent buff instance of the buffs of the given name. Parent buffs control stack count for buffs of the same name.
        /// </summary>
        /// <param name="name">Internal buff name to check.</param>
        /// <returns>Parent buff instance.</returns>
        public Buff GetBuffWithName(string name)
        {
            Buff buff;
            if (ParentBuffs.TryGetValue(name, out buff))
            {
                return buff;
            }
            return null;
        }

        /// <summary>
        /// Gets a list of all buffs applied to this unit (parent and children).
        /// </summary>
        /// <returns>List of buff instances.</returns>
        public List<Buff> GetBuffs()
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
        public List<Buff> GetBuffsWithName(string buffName)
        {
            return BuffList.FindAll(b => b.IsBuffSame(buffName));
        }

        /// <summary>
        /// Removes the given buff from this unit. Called automatically when buff timers have finished.
        /// Buffs with BuffAddType.STACKS_AND_OVERLAPS are removed incrementally, meaning one instance removed per RemoveBuff call.
        /// Other BuffAddTypes are removed entirely, regardless of stacks. DecrementStackCount can be used as an alternative.
        /// </summary>
        /// <param name="b">Buff to remove.</param>
        public void RemoveBuff(Buff b)
        {
            if (!HasBuff(b))
            {
                return;
            }

            // If the buff is supposed to be a single stackable buff with a timer = Duration * StackCount, and their are more than one already present.
            if (b.BuffAddType == BuffAddType.STACKS_AND_CONTINUE && b.StackCount > 1)
            {
                b.DecrementStackCount();

                Buff tempBuff = new Buff(_game, b.Name, b.Duration, b.StackCount, b.OriginSpell, b.TargetUnit, b.SourceUnit, b.IsBuffInfinite(), b.ParentScript);

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

                if (!b.IsHidden)
                {
                    _game.PacketNotifier.NotifyNPC_BuffRemove2(b);
                }
            }
        }

        /// <summary>
        /// Removes the given buff instance from the buff slots of this unit.
        /// Called automatically by RemoveBuff().
        /// </summary>
        /// <param name="b">Buff instance to check for.</param>
        private void RemoveBuffSlot(Buff b)
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
            Buff parentBuff = ParentBuffs[b];
            if (removeSlot && parentBuff != null)
            {
                RemoveBuffSlot(parentBuff);
            }
            BuffList.Remove(parentBuff);
            ParentBuffs.Remove(b);
        }

        /// <summary>
        /// Removes all buffs of the given internal name from this unit regardless of stack count.
        /// Intended mainly for buffs with BuffAddType.STACKS_AND_OVERLAPS.
        /// </summary>
        /// <param name="buffName">Internal buff name to remove.</param>
        public void RemoveBuffsWithName(string buffName)
        {
            foreach (Buff b in BuffList)
            {
                if (b.IsBuffSame(buffName))
                {
                    b.DeactivateBuff();
                }
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
            SetWaypoints(new List<Vector2> { Position, newCoords });

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
            _dashEffectsToDisable = 0;
            if (state)
            {
                _dashEffectsToDisable = MovementParameters.SetStatus;
            }
            SetStatus(StatusFlags.None, true);

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

                ResetWaypoints();
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
