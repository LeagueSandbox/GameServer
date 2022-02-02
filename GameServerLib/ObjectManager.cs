using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using GameServerCore;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.Other;
using LeagueSandbox.GameServer.Logging;

namespace LeagueSandbox.GameServer
{
    // TODO: refactor this class

    /// <summary>
    /// Class that manages addition, removal, and updating of all GameObjects, their visibility, and buffs.
    /// </summary>
    public class ObjectManager : IObjectManager
    {
        // Crucial Vars
        private Game _game;

        // Dictionaries of GameObjects.
        private Dictionary<uint, IGameObject> _objects;
        private Dictionary<uint, IChampion> _champions;
        private Dictionary<uint, IBaseTurret> _turrets;
        private Dictionary<uint, IInhibitor> _inhibitors;

        // Locks for each dictionary.
        private object _objectsLock = new object();
        private object _turretsLock = new object();
        private object _inhibitorsLock = new object();
        private object _championsLock = new object();
        private object _visionLock = new object();

        /// <summary>
        /// List of all possible teams in League of Legends. Normally there are only three.
        /// </summary>
        public List<TeamId> Teams { get; private set; }

        /// <summary>
        /// Instantiates all GameObject Dictionaries in ObjectManager.
        /// </summary>
        /// <param name="game">Game instance.</param>
        public ObjectManager(Game game)
        {
            _game = game;
            _objects = new Dictionary<uint, IGameObject>();
            _turrets = new Dictionary<uint, IBaseTurret>();
            _inhibitors = new Dictionary<uint, IInhibitor>();
            _champions = new Dictionary<uint, IChampion>();

            Teams = Enum.GetValues(typeof(TeamId)).Cast<TeamId>().ToList();
        }

        /// <summary>
        /// Function called every tick of the game.
        /// </summary>
        /// <param name="diff">Number of milliseconds since this tick occurred.</param>
        public void Update(float diff)
        {
            foreach (var obj in _objects.Values.ToList())
            {
                if (obj.IsToRemove())
                {
                    RemoveObject(obj);
                    continue;
                }

                obj.Update(diff);
            }

            // Process existing objects and those created during `obj.Update`s
            foreach(var obj in _objects.Values.ToList())
            {
            
                bool alreadySpawned = obj.IsSpawned();

                UpdateVisibilityAndSpawnIfNecessary(obj);
            
                // If the object just spawned, then it is still in sync
                if (alreadySpawned && obj is IAttackableUnit u && !u.IsDead)
                {
                    //if(u.Replication.Changed)
                    _game.PacketNotifier.NotifyUpdatedStats(u, false);

                    if (u.IsModelUpdated)
                    {
                        _game.PacketNotifier.NotifyS2C_ChangeCharacterData(u);
                        u.IsModelUpdated = false;
                    }

                    if (u.IsMovementUpdated())
                    {
                        // TODO: Verify which one we want to use. WaypointList does not require conversions, however WaypointGroup does (and it has TeleportID functionality).
                        //_game.PacketNotifier.NotifyWaypointList(u);
                        // TODO: Verify if we want to use TeleportID.
                        _game.PacketNotifier.NotifyWaypointGroup(u, false);
                        u.ClearMovementUpdated();
                    }
                }
            }
        }

        void UpdateVisibilityAndSpawnIfNecessary(IGameObject obj){

            IParticle particle = null;
            IAttackableUnit u = null;
            ILevelProp lp = null;
            
            //TODO: Implement visibility checks for projectiles here (should be similar to particles below)
            //Make sure to account for server only projectiles, globally visible (everyone sees it) projectiles, and normal projectiles:
            //1. Nidalee Q is affected by visibility checks, but is server only 
            //2. Ezreal R is globally visible, and is server only
            //3. Every other projectile that is not server only, and is affected by visibility checks (normal projectiles)

            if (
                // If it is a visible object
                (lp = obj as ILevelProp) != null
                || (
                    (u = obj as IAttackableUnit) != null
                    && !u.IsDead // Do not update the visibility of the dead for the cool animation of drowning in the ground
                )
                || (
                    (particle = obj as IParticle) != null
                    && particle.VisionAffected // Only if the particle is affected by vision.
                )
            ) {

                
                TeamIdFlags teamsWithVision = 0;

                // If is always visible
                if(lp != null || u is IBaseTurret || u is IObjBuilding)
                {
                    teamsWithVision = TeamIdFlags.TEAMS_ALL;
                }
                else
                {
                    teamsWithVision.SetTeam(obj.Team, true);
                    
                    foreach (var kv in _objects)
                    {
                        var team = kv.Value.Team;
                        if(
                            !teamsWithVision.HasTeam(team) // The team has already got a vision
                            && (
                                particle == null
                                || particle.SpecificTeam == TeamId.TEAM_NEUTRAL
                                || particle.SpecificTeam == team
                            )
                            && !(kv.Value is IAttackableUnit unit && unit.IsDead)
                            && Vector2.DistanceSquared(kv.Value.Position, obj.Position)
                                        <= kv.Value.VisionRadius * kv.Value.VisionRadius
                            && !_game.Map.NavigationGrid.IsAnythingBetween(kv.Value, obj, true))
                        {
                            teamsWithVision.SetTeam(team, true);
                        }
                    }
                }

                if(obj.IsSpawned())
                {
                    foreach (var team in Teams)
                    {
                        bool teamHasVision = teamsWithVision.HasTeam(team);
                        bool isVisibleByTeam = obj.IsVisibleByTeam(team);
                        if (isVisibleByTeam != teamHasVision)
                        {
                            obj.SetVisibleByTeam(team, teamHasVision);
                            _game.PacketNotifier.NotifyTeamVisibilityChange(obj, team, teamHasVision);
                        }
                    }
                }
                else
                {
                    obj.SetAsSpawned();
                    obj.SetVisibleByTeams(teamsWithVision);
                    
                    if (obj is IChampion /*|| obj is IParticle*/)
                    {
                        // They are already spawned,
                        // all we need is to update the visibility
                        // which was not known at the time of creation
                        //TODO: Should everything spawn here?
                        _game.PacketNotifier.NotifyTeamVisibilityChange(obj, obj.Team, true);
                    }
                    else if (obj is ILaneTurret turret)
                    {
                        // Tower initialization requires actions to be performed after it spawns.
                        //TODO: Introduce OnSpawn callback?
                        
                        _game.PacketNotifier.NotifySpawn(turret);

                        new Region(_game, turret.Team, turret.Position, RegionType.Default, turret, turret, true, 800f, true, true, turret.CollisionRadius, lifetime: 25000.0f);

                        foreach (var item in turret.Inventory)
                        {
                            if (item != null)
                            {
                                _game.PacketNotifier.NotifyBuyItem((int)turret.NetId, turret, item as IItem);
                            }
                        }
                    }
                    else
                    {
                        bool visibleByDefault = obj is ILevelProp || obj is ISpellMissile;
                        _game.PacketNotifier.NotifySpawn
                        (
                            obj,
                            userId: 0,
                            doVision: !visibleByDefault,
                            _game.GameTime
                        );
                    }
                }
            }
        }

        /// <summary>
        /// Adds a GameObject to the dictionary of GameObjects in ObjectManager.
        /// </summary>
        /// <param name="o">GameObject to add.</param>
        public void AddObject(IGameObject o)
        {
            if (o == null)
            {
                return;
            }

            // If it crashes here the problem is most likely somewhere else
            //lock (_objectsLock)
            {
                _objects.Add(o.NetId, o);
            }

            o.OnAdded();
        }

        /// <summary>
        /// Removes a GameObject from the dictionary of GameObjects in ObjectManager.
        /// </summary>
        /// <param name="o">GameObject to remove.</param>
        public void RemoveObject(IGameObject o)
        {
            //lock (_objectsLock)
            {
                _objects.Remove(o.NetId);
            }

            o.OnRemoved();
        }

        /// <summary>
        /// Gets a new Dictionary of all NetID,GameObject pairs present in the dictionary of objects in ObjectManager.
        /// </summary>
        /// <returns>Dictionary of NetIDs and the GameObjects that they refer to.</returns>
        public Dictionary<uint, IGameObject> GetObjects()
        {
            return new Dictionary<uint, IGameObject>(_objects); // Just clone
        }

        /// <summary>
        /// Gets a GameObject from the list of objects in ObjectManager that is identified by the specified NetID.
        /// </summary>
        /// <param name="netId">NetID to check.</param>
        /// <returns>GameObject instance that has the specified NetID.</returns>
        public IGameObject GetObjectById(uint id)
        {
            if (!_objects.ContainsKey(id))
            {
                return null;
            }

            return _objects[id];
        }

        /// <summary>
        /// Gets a list of all GameObjects of type AttackableUnit that are within a certain distance from a specified position.
        /// </summary>
        /// <param name="checkPos">Vector2 position to check.</param>
        /// <param name="range">Distance to check.</param>
        /// <param name="onlyAlive">Whether dead units should be excluded or not.</param>
        /// <returns>List of all AttackableUnits within the specified range and of the specified alive status.</returns>
        public List<IAttackableUnit> GetUnitsInRange(Vector2 checkPos, float range, bool onlyAlive = false)
        {
            var units = new List<IAttackableUnit>();
            //lock (_objectsLock)
            {
                foreach (var kv in _objects)
                {
                    if (kv.Value is IAttackableUnit u && Vector2.DistanceSquared(checkPos, u.Position) <= range * range && (onlyAlive && !u.IsDead || !onlyAlive))
                    {
                        units.Add(u);
                    }
                }
            }

            return units;
        }

        /// <summary>
        /// Counts the number of units attacking a specified GameObject of type AttackableUnit.
        /// </summary>
        /// <param name="target">AttackableUnit potentially being attacked.</param>
        /// <returns>Number of units attacking target.</returns>
        public int CountUnitsAttackingUnit(IAttackableUnit target)
        {
            return GetObjects().Count(x =>
                x.Value is IObjAiBase aiBase &&
                aiBase.Team == target.Team.GetEnemyTeam() &&
                !aiBase.IsDead &&
                aiBase.TargetUnit != null &&
                aiBase.TargetUnit == target
            );
        }

        /// <summary>
        /// Forces all GameObjects of type ObjAIBase to stop targeting the specified AttackableUnit.
        /// //TODO: DEPRECATE - not effective. Let the attackers subscribe to the target death event?
        /// </summary>
        /// <param name="target">AttackableUnit that should be untargeted.</param>
        public void StopTargeting(IAttackableUnit target)
        {
            //lock (_objectsLock)
            {
                foreach (var kv in _objects)
                {
                    var u = kv.Value as IAttackableUnit;
                    if (u == null)
                    {
                        continue;
                    }

                    var ai = u as IObjAiBase;
                    if (ai != null)
                    {
                        if (ai.TargetUnit == target)
                        {
                            ai.SetTargetUnit(null, true);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds a GameObject of type BaseTurret to the list of BaseTurrets in ObjectManager.
        /// </summary>
        /// <param name="turret">BaseTurret to add.</param>
        public void AddTurret(IBaseTurret turret)
        {
            //lock (_turretsLock)
            {
                _turrets.Add(turret.NetId, turret);
            }
        }

        /// <summary>
        /// Gets a GameObject of type BaseTurret from the list of BaseTurrets in ObjectManager who is identified by the specified NetID.
        /// Unused.
        /// </summary>
        /// <param name="netId"></param>
        /// <returns>BaseTurret instance identified by the specified NetID.</returns>
        public IBaseTurret GetTurretById(uint netId)
        {
            if (!_turrets.ContainsKey(netId))
            {
                return null;
            }

            return _turrets[netId];
        }

        /// <summary>
        /// Removes a GameObject of type BaseTurret from the list of BaseTurrets in ObjectManager.
        /// Unused.
        /// </summary>
        /// <param name="turret">BaseTurret to remove.</param>
        public void RemoveTurret(IBaseTurret turret)
        {
            //lock (_turretsLock)
            {
                _turrets.Remove(turret.NetId);
            }
        }

        /// <summary>
        /// How many turrets of a specified team are destroyed in the specified lane.
        /// Used for building protection, specifically for cases where new turrets are added after map turrets.
        /// Unused.
        /// </summary>
        /// <param name="team">Team of the BaseTurrets to check.</param>
        /// <param name="lane">Lane to check.</param>
        /// <returns>Number of turrets in the lane destroyed.</returns>
        /// TODO: Implement AzirTurrets so this can be used.
        public int GetTurretsDestroyedForTeam(TeamId team, LaneID lane)
        {
            int destroyed = 0;
            foreach (var turret in _turrets.Values)
            {
                if (turret.Team == team && turret.Lane == lane && turret.IsDead)
                {
                    destroyed++;
                }
            }

            return destroyed;
        }

        /// <summary>
        /// Adds a GameObject of type Inhibitor to the list of Inhibitors in ObjectManager.
        /// </summary>
        /// <param name="inhib">Inhibitor to add.</param>
        public void AddInhibitor(IInhibitor inhib)
        {
            //lock (_inhibitorsLock)
            {
                _inhibitors.Add(inhib.NetId, inhib);
            }
        }

        /// <summary>
        /// Gets a GameObject of type Inhibitor from the list of Inhibitors in ObjectManager who is identified by the specified NetID.
        /// </summary>
        /// <param name="netId"></param>
        /// <returns>Inhibitor instance identified by the specified NetID.</returns>
        public IInhibitor GetInhibitorById(uint id)
        {
            if (!_inhibitors.ContainsKey(id))
            {
                return null;
            }

            return _inhibitors[id];
        }

        /// <summary>
        /// Removes a GameObject of type Inhibitor from the list of Inhibitors in ObjectManager.
        /// </summary>
        /// <param name="inhib">Inhibitor to remove.</param>
        public void RemoveInhibitor(IInhibitor inhib)
        {
            //lock (_inhibitorsLock)
            {
                _inhibitors.Remove(inhib.NetId);
            }
        }

        /// <summary>
        /// Whether or not all of the Inhibitors of a specified team are destroyed.
        /// </summary>
        /// <param name="team">Team of the Inhibitors to check.</param>
        /// <returns>true/false; destroyed or not</returns>
        public bool AllInhibitorsDestroyedFromTeam(TeamId team)
        {
            foreach (var inhibitor in _inhibitors.Values)
            {
                if (inhibitor.Team == team && inhibitor.InhibitorState == InhibitorState.ALIVE)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Adds a GameObject of type Champion to the list of Champions in ObjectManager.
        /// </summary>
        /// <param name="champion">Champion to add.</param>
        public void AddChampion(IChampion champion)
        {
            //lock (_championsLock)
            {
                _champions.Add(champion.NetId, champion);
            }
        }

        /// <summary>
        /// Removes a GameObject of type Champion from the list of Champions in ObjectManager.
        /// </summary>
        /// <param name="champion">Champion to remove.</param>
        public void RemoveChampion(IChampion champion)
        {
            //lock (_championsLock)
            {
                _champions.Remove(champion.NetId);
            }
        }

        /// <summary>
        /// Gets a new list of all Champions found in the list of Champions in ObjectManager.
        /// </summary>
        /// <returns>List of all valid Champions.</returns>
        public List<IChampion> GetAllChampions()
        {
            var champs = new List<IChampion>();
            foreach (var kv in _champions)
            {
                var c = kv.Value;
                if (c != null)
                {
                    champs.Add(c);
                }
            }

            return champs;
        }

        /// <summary>
        /// Gets a new list of all Champions of the specified team found in the list of Champios in ObjectManager.
        /// </summary>
        /// <param name="team">TeamId.BLUE/PURPLE/NEUTRAL</param>
        /// <returns>List of valid Champions of the specified team.</returns>
        public List<IChampion> GetAllChampionsFromTeam(TeamId team)
        {
            var champs = new List<IChampion>();
            foreach (var kv in _champions)
            {
                var c = kv.Value;
                if (c.Team == team)
                {
                    champs.Add(c);
                }
            }

            return champs;
        }

        /// <summary>
        /// Gets a list of all GameObjects of type Champion that are within a certain distance from a specified position.
        /// </summary>
        /// <param name="checkPos">Vector2 position to check.</param>
        /// <param name="range">Distance to check.</param>
        /// <param name="onlyAlive">Whether dead Champions should be excluded or not.</param>
        /// <returns>List of all Champions within the specified range of the position and of the specified alive status.</returns>
        public List<IChampion> GetChampionsInRange(Vector2 checkPos, float range, bool onlyAlive = false)
        {
            var champs = new List<IChampion>();
            //lock (_championsLock)
            {
                foreach (var kv in _champions)
                {
                    var c = kv.Value;
                    if (Vector2.DistanceSquared(checkPos, c.Position) <= range * range)
                        if (onlyAlive && !c.IsDead || !onlyAlive)
                            champs.Add(c);
                }
            }

            return champs;
        }
    }
}