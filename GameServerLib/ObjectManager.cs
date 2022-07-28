using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using GameServerCore;
using GameServerCore.Enums;
using GameServerCore.NetInfo;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.Buildings.AnimatedBuildings;

namespace LeagueSandbox.GameServer
{
    // TODO: refactor this class

    /// <summary>
    /// Class that manages addition, removal, and updating of all GameObjects, their visibility, and buffs.
    /// </summary>
    public class ObjectManager
    {
        // Crucial Vars
        private Game _game;

        // Dictionaries of GameObjects.
        private Dictionary<uint, GameObject> _objects;
        private List<GameObject> _objectsToAdd = new List<GameObject>();
        private List<GameObject> _objectsToRemove = new List<GameObject>();
        // For the initial spawning (networking) of newly added objects.
        private Dictionary<uint, Champion> _champions;
        private Dictionary<uint, BaseTurret> _turrets;
        private Dictionary<uint, Inhibitor> _inhibitors;
        private Dictionary<TeamId, List<GameObject>> _visionProviders;

        private bool _currentlyInUpdate = false;

        // Locks for each dictionary. Depricated since #1302.
        //private object _objectsLock = new object();
        //private object _turretsLock = new object();
        //private object _inhibitorsLock = new object();
        //private object _championsLock = new object();
        //private object _visionLock = new object();

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
            Teams = Enum.GetValues(typeof(TeamId)).Cast<TeamId>().ToList();

            _game = game;
            _objects = new Dictionary<uint, GameObject>();
            _turrets = new Dictionary<uint, BaseTurret>();
            _inhibitors = new Dictionary<uint, Inhibitor>();
            _champions = new Dictionary<uint, Champion>();
            _visionProviders = new Dictionary<TeamId, List<GameObject>>();
            foreach (var team in Teams)
            {
                _visionProviders.Add(team, new List<GameObject>());
            }
        }

        /// <summary>
        /// Function called every tick of the game.
        /// </summary>
        /// <param name="diff">Number of milliseconds since this tick occurred.</param>
        public void Update(float diff)
        {
            _currentlyInUpdate = true;

            // For all existing objects
            foreach (var obj in _objects.Values)
            {
                obj.Update(diff);
            }

            // It is now safe to call RemoveObject at any time,
            // but compatibility with the older remove method remains.
            foreach (var obj in _objects.Values)
            {
                if (obj.IsToRemove())
                {
                    RemoveObject(obj);
                }
            }

            foreach (var obj in _objectsToRemove)
            {
                _objects.Remove(obj.NetId);
            }
            _objectsToRemove.Clear();

            int oldObjectsCount = _objects.Count;

            foreach (var obj in _objectsToAdd)
            {
                _objects.Add(obj.NetId, obj);
            }
            _objectsToAdd.Clear();

            var players = _game.PlayerManager.GetPlayers(includeBots: false);

            int i = 0;
            foreach (GameObject obj in _objects.Values)
            {
                UpdateTeamsVision(obj);
                if(i++ < oldObjectsCount)
                {
                    obj.LateUpdate(diff);
                }

                foreach (var kv in players)
                {
                    UpdateVisionSpawnAndSync(obj, kv);
                }

                obj.OnAfterSync();
            }

            _game.PacketNotifier.NotifyOnReplication();
            _game.PacketNotifier.NotifyWaypointGroup();

            _currentlyInUpdate = false;
        }

        /// <summary>
        /// Normally, objects will spawn at the end of the frame, but calling this function will force the teams' and players' vision of that object to update and send out a spawn notification.
        /// </summary>
        /// <param name="obj">Object to spawn.</param>
        public void SpawnObject(GameObject obj)
        {
            UpdateTeamsVision(obj);

            var players = _game.PlayerManager.GetPlayers(includeBots: false);
            foreach (var kv in players)
            {
                UpdateVisionSpawnAndSync(obj, kv, forceSpawn: true);
            }

            obj.OnAfterSync();
        }

        public void OnReconnect(int userId, TeamId team)
        {
            foreach (GameObject obj in _objects.Values)
            {
                obj.OnReconnect(userId, team);
            }
        }

        public void SpawnObjects(ClientInfo clientInfo)
        {
            foreach (GameObject obj in _objects.Values)
            {
                UpdateVisionSpawnAndSync(obj, clientInfo, forceSpawn: true);
            }
        }

        /// <summary>
        /// Updates the vision of the teams on the object.
        /// </summary>
        void UpdateTeamsVision(GameObject obj)
        {
            foreach (var team in Teams)
            {
                obj.SetVisibleByTeam(team, !obj.IsAffectedByFoW || TeamHasVisionOn(team, obj));
            }
        }

        /// <summary>
        /// Updates the player's vision, which may not be tied to the team's vision, sends a spawn notification or updates if the object is already spawned.
        /// </summary>
        public void UpdateVisionSpawnAndSync(GameObject obj, ClientInfo clientInfo, bool forceSpawn = false)
        {
            int cid = clientInfo.ClientId;
            TeamId team = clientInfo.Team;
            Champion champion = clientInfo.Champion;

            bool nearSighted = champion.Status.HasFlag(StatusFlags.NearSighted);
            bool shouldBeVisibleForPlayer = !obj.IsAffectedByFoW || (
                nearSighted ?
                    UnitHasVisionOn(champion, obj) :
                    obj.IsVisibleByTeam(champion.Team)
            );

            obj.Sync(cid, team, shouldBeVisibleForPlayer, forceSpawn);
        }

        /// <summary>
        /// Adds a GameObject to the dictionary of GameObjects in ObjectManager.
        /// </summary>
        /// <param name="o">GameObject to add.</param>
        public void AddObject(GameObject o)
        {
            if (o != null)
            {
                _objectsToRemove.Remove(o);

                if(_currentlyInUpdate)
                {
                    _objectsToAdd.Add(o);
                }
                else
                {
                    _objects.Add(o.NetId, o);
                }
                // TODO: This is a hack-fix for units which have packets being sent before spawning (ex: AscWarp minion)
                // Instead, we need a dedicated packet queue system which takes all packets which are not vision/spawn related,
                // and queues them if the object is not spawned yet for clients.
                if (!(o is Champion))
                {
                    SpawnObject(o);
                }
                o.OnAdded();
            }
        }

        /// <summary>
        /// Removes a GameObject from the dictionary of GameObjects in ObjectManager.
        /// </summary>
        /// <param name="o">GameObject to remove.</param>
        public void RemoveObject(GameObject o)
        {
            if (o != null)
            {
                _objectsToAdd.Remove(o);

                if(_currentlyInUpdate)
                {
                    _objectsToRemove.Add(o);
                }
                else
                {
                    _objects.Remove(o.NetId);
                }
                o.OnRemoved();
            }
        }

        /// <summary>
        /// Gets a new Dictionary of all NetID,GameObject pairs present in the dictionary of objects in ObjectManager.
        /// </summary>
        /// <returns>Dictionary of NetIDs and the GameObjects that they refer to.</returns>
        public Dictionary<uint, GameObject> GetObjects()
        {
            var ret = new Dictionary<uint, GameObject>();
            foreach (var obj in _objects)
            {
                ret.Add(obj.Key, obj.Value);
            }

            return ret;
        }

        /// <summary>
        /// Gets a GameObject from the list of objects in ObjectManager that is identified by the specified NetID.
        /// </summary>
        /// <param name="id">NetID to check.</param>
        /// <returns>GameObject instance that has the specified NetID. Null otherwise.</returns>
        public GameObject GetObjectById(uint id)
        {
            GameObject obj = _objectsToAdd.Find(o => o.NetId == id);

            if (obj == null)
            {
                obj = _objects.GetValueOrDefault(id, null);
            }

            return obj;
        }

        /// <summary>
        /// Whether or not a specified GameObject is being networked to the specified team.
        /// </summary>
        /// <param name="team">TeamId.BLUE/PURPLE/NEUTRAL</param>
        /// <param name="o">GameObject to check.</param>
        /// <returns>true/false; networked or not.</returns>
        public bool TeamHasVisionOn(TeamId team, GameObject o)
        {
            if (o != null)
            {
                if(!o.IsAffectedByFoW)
                {
                    return true;
                }

                foreach (var p in _visionProviders[team])
                {
                    if (UnitHasVisionOn(p, o))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        bool UnitHasVisionOn(GameObject observer, GameObject tested)
        {
            if(!tested.IsAffectedByFoW)
            {
                return true;
            }

            if(observer is Region region)
            {
                if(region.VisionTarget != null && region.VisionTarget != tested)
                {
                    return false;
                }
            }
            else if(tested is Particle particle)
            {
                // Default behaviour
                if(particle.SpecificTeam == TeamId.TEAM_NEUTRAL)
                {
                    if(
                        // Globally visible to all teams
                        particle.Team == TeamId.TEAM_NEUTRAL
                        // Globally visible to team of creator
                        || tested.Team == observer.Team
                    ){
                        return true;
                    }
                    // Can become visible for other teams
                }
                // Only visible to specific team
                else if(particle.SpecificTeam != observer.Team)
                {
                    return false;
                }
            }
            else if(tested.Team == observer.Team)
            {
                return true;
            }

            if(
                !(observer is AttackableUnit u && u.IsDead)
                && Vector2.DistanceSquared(observer.Position, tested.Position) < observer.VisionRadius * observer.VisionRadius
                && !_game.Map.NavigationGrid.IsAnythingBetween(observer, tested, true)
            ){
                return true;
            }

            return false;
        }

        /// <summary>
        /// Adds a GameObject to the list of Vision Providers in ObjectManager.
        /// </summary>
        /// <param name="obj">GameObject to add.</param>
        /// <param name="team">The team that GameObject can provide vision to.</param>
        public void AddVisionProvider(GameObject obj, TeamId team)
        {
            //lock (_visionLock)
            {
                _visionProviders[team].Add(obj);
            }
        }

        /// <summary>
        /// Removes a GameObject from the list of Vision Providers in ObjectManager.
        /// </summary>
        /// <param name="obj">GameObject to remove.</param>
        /// <param name="team">The team that GameObject provided vision to.</param>
        public void RemoveVisionProvider(GameObject obj, TeamId team)
        {
            //lock (_visionLock)
            {
                _visionProviders[team].Remove(obj);
            }
        }

        /// <summary>
        /// Gets a list of all GameObjects of type AttackableUnit that are within a certain distance from a specified position.
        /// </summary>
        /// <param name="checkPos">Vector2 position to check.</param>
        /// <param name="range">Distance to check.</param>
        /// <param name="onlyAlive">Whether dead units should be excluded or not.</param>
        /// <returns>List of all AttackableUnits within the specified range and of the specified alive status.</returns>
        public List<AttackableUnit> GetUnitsInRange(Vector2 checkPos, float range, bool onlyAlive = false)
        {
            var units = new List<AttackableUnit>();
            foreach (var kv in _objects)
            {
                if (kv.Value is AttackableUnit u && Vector2.DistanceSquared(checkPos, u.Position) <= range * range && (onlyAlive && !u.IsDead || !onlyAlive))
                {
                    units.Add(u);
                }
            }

            return units;
        }

        /// <summary>
        /// Counts the number of units attacking a specified GameObject of type AttackableUnit.
        /// </summary>
        /// <param name="target">AttackableUnit potentially being attacked.</param>
        /// <returns>Number of units attacking target.</returns>
        public int CountUnitsAttackingUnit(AttackableUnit target)
        {
            return GetObjects().Count(x =>
                x.Value is ObjAIBase aiBase &&
                aiBase.Team == target.Team.GetEnemyTeam() &&
                !aiBase.IsDead &&
                aiBase.TargetUnit != null &&
                aiBase.TargetUnit == target
            );
        }

        /// <summary>
        /// Forces all GameObjects of type ObjAIBase to stop targeting the specified AttackableUnit.
        /// </summary>
        /// <param name="target">AttackableUnit that should be untargeted.</param>
        public void StopTargeting(AttackableUnit target)
        {
            foreach (var kv in _objects)
            {
                var u = kv.Value as AttackableUnit;
                if (u == null)
                {
                    continue;
                }

                var ai = u as ObjAIBase;
                if (ai != null)
                {
                    ai.Untarget(target);
                }
            }
        }

        /// <summary>
        /// Adds a GameObject of type BaseTurret to the list of BaseTurrets in ObjectManager.
        /// </summary>
        /// <param name="turret">BaseTurret to add.</param>
        public void AddTurret(BaseTurret turret)
        {
            _turrets.Add(turret.NetId, turret);
        }

        /// <summary>
        /// Gets a GameObject of type BaseTurret from the list of BaseTurrets in ObjectManager who is identified by the specified NetID.
        /// Unused.
        /// </summary>
        /// <param name="netId"></param>
        /// <returns>BaseTurret instance identified by the specified NetID.</returns>
        public BaseTurret GetTurretById(uint netId)
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
        public void RemoveTurret(BaseTurret turret)
        {
            _turrets.Remove(turret.NetId);
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
        public int GetTurretsDestroyedForTeam(TeamId team, Lane lane)
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
        public void AddInhibitor(Inhibitor inhib)
        {
            _inhibitors.Add(inhib.NetId, inhib);
        }

        /// <summary>
        /// Gets a GameObject of type Inhibitor from the list of Inhibitors in ObjectManager who is identified by the specified NetID.
        /// </summary>
        /// <param name="netId"></param>
        /// <returns>Inhibitor instance identified by the specified NetID.</returns>
        public Inhibitor GetInhibitorById(uint id)
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
        public void RemoveInhibitor(Inhibitor inhib)
        {
            _inhibitors.Remove(inhib.NetId);
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
                if (inhibitor.Team == team && inhibitor.InhibitorState == DampenerState.RespawningState)
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
        public void AddChampion(Champion champion)
        {
            _champions.Add(champion.NetId, champion);
        }

        /// <summary>
        /// Removes a GameObject of type Champion from the list of Champions in ObjectManager.
        /// </summary>
        /// <param name="champion">Champion to remove.</param>
        public void RemoveChampion(Champion champion)
        {
            _champions.Remove(champion.NetId);
        }

        /// <summary>
        /// Gets a new list of all Champions found in the list of Champions in ObjectManager.
        /// </summary>
        /// <returns>List of all valid Champions.</returns>
        public List<Champion> GetAllChampions()
        {
            var champs = new List<Champion>();
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
        public List<Champion> GetAllChampionsFromTeam(TeamId team)
        {
            var champs = new List<Champion>();
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
        public List<Champion> GetChampionsInRange(Vector2 checkPos, float range, bool onlyAlive = false)
        {
            var champs = new List<Champion>();
            foreach (var kv in _champions)
            {
                var c = kv.Value;
                if (Vector2.DistanceSquared(checkPos, c.Position) <= range * range)
                    if (onlyAlive && !c.IsDead || !onlyAlive)
                        champs.Add(c);
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
        public List<Champion> GetChampionsInRangeFromTeam(Vector2 checkPos, float range, TeamId team, bool onlyAlive = false)
        {
            var champs = new List<Champion>();
            foreach (var kv in _champions)
            {
                var c = kv.Value;
                if (Vector2.DistanceSquared(checkPos, c.Position) <= range * range)
                    if (c.Team == team && (onlyAlive && !c.IsDead || !onlyAlive))
                        champs.Add(c);
            }

            return champs;
        }
    }
}
