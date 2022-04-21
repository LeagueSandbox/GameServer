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
using GameServerCore.NetInfo;

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
        private List<IGameObject> _objectsToAdd = new List<IGameObject>();
        private List<IGameObject> _objectsToRemove = new List<IGameObject>();
        // For the initial spawning (networking) of newly added objects.
        private Dictionary<uint, IChampion> _champions;
        private Dictionary<uint, IBaseTurret> _turrets;
        private Dictionary<uint, IInhibitor> _inhibitors;
        private Dictionary<TeamId, List<IGameObject>> _visionProviders;

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
            _objects = new Dictionary<uint, IGameObject>();
            _turrets = new Dictionary<uint, IBaseTurret>();
            _inhibitors = new Dictionary<uint, IInhibitor>();
            _champions = new Dictionary<uint, IChampion>();
            _visionProviders = new Dictionary<TeamId, List<IGameObject>>();
            foreach (var team in Teams)
            {
                _visionProviders.Add(team, new List<IGameObject>());
            }
        }

        bool IsAffectedByVision(IGameObject obj)
        {
            return obj is IParticle || obj is IAttackableUnit || obj is ISpellMissile;
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

            foreach (var obj in _objects.Values)
            {
                LateUpdate(obj, diff);
            }

            foreach (var obj in _objectsToAdd)
            {
                _objects.Add(obj.NetId, obj);
            }
            _objectsToAdd.Clear();

            var players = _game.PlayerManager.GetPlayers(includeBots: false);
            
            foreach (IGameObject obj in _objects.Values)
            {
                UpdateTeamsVision(obj);

                foreach (var kv in players)
                {
                    UpdateVisionSpawnAndSync(obj, kv.Item2);
                }

                if (obj is IAttackableUnit u)
                {
                    u.Replication.MarkAsUnchanged();
                    u.IsModelUpdated = false;
                    u.ClearMovementUpdated();
                }
            }

            _currentlyInUpdate = false;
        }

        /// <summary>
        /// Normally, objects will spawn at the end of the frame, but calling this function will force the teams' and players' vision of that object to update and send out a spawn notification.
        /// </summary>
        /// <param name="obj">Object to spawn.</param>
        public void SpawnObject(IGameObject obj)
        {
            UpdateTeamsVision(obj);

            var players = _game.PlayerManager.GetPlayers(includeBots: false);
            foreach (var kv in players)
            {
                UpdateVisionSpawnAndSync(obj, kv.Item2, forceSpawn: true);
            }
        }

        /// <summary>
        /// Updates the vision of the teams on the object.
        /// </summary>
        void UpdateTeamsVision(IGameObject obj)
        {
            if (IsAffectedByVision(obj))
            {
                foreach (var team in Teams)
                {
                    obj.SetVisibleByTeam(team, TeamHasVisionOn(team, obj));
                }
            }
        }

        /// <summary>
        /// Updates the player's vision, which may not be tied to the team's vision, sends a spawn notification or updates if the object is already spawned.
        /// </summary>
        public void UpdateVisionSpawnAndSync(IGameObject obj, ClientInfo clientInfo, bool forceSpawn = false)
        {
            int pid = (int)clientInfo.PlayerId;
            TeamId team = clientInfo.Team;
            IChampion champion = clientInfo.Champion;
            
            bool nearSighted = champion.Status.HasFlag(StatusFlags.NearSighted);
            bool isAffectedByVision = IsAffectedByVision(obj);
            bool shouldBeVisibleForPlayer = !isAffectedByVision || (
                nearSighted ?
                    UnitHasVisionOn(champion, obj) :
                    obj.IsVisibleByTeam(champion.Team)
            );
            
            if (!forceSpawn && obj.IsSpawnedForPlayer(pid))
            {
                if (isAffectedByVision && (obj.IsVisibleForPlayer(pid) != shouldBeVisibleForPlayer))
                {
                    _game.PacketNotifier.NotifyVisibilityChange(obj, team, shouldBeVisibleForPlayer, pid);
                    obj.SetVisibleForPlayer(pid, shouldBeVisibleForPlayer);
                }
                else if(shouldBeVisibleForPlayer)
                {
                    Sync(obj, pid);
                }
            }
            else if (shouldBeVisibleForPlayer || !(
                //bool spawnShouldBeHidden = 
                obj is IParticle || obj is ISpellMissile || (obj is IMinion && !(obj is ILaneMinion))
            ))
            {
                _game.PacketNotifier.NotifySpawn(obj, team, pid, _game.GameTime, shouldBeVisibleForPlayer);
                obj.SetVisibleForPlayer(pid, shouldBeVisibleForPlayer);
                obj.SetSpawnedForPlayer(pid);

                if (obj is ILaneTurret turret)
                {
                    foreach (var item in turret.Inventory)
                    {
                        if (item != null)
                        {
                            _game.PacketNotifier.NotifyBuyItem(pid, turret, item as IItem);
                        }
                    }
                }
            }
        }

        void Sync(IGameObject obj, int userId = 0)
        {
            if (obj is IAttackableUnit u)
            {
                if(u.Replication.Changed)
                {
                    _game.PacketNotifier.NotifyOnReplication(u, userId, true);
                }

                if (u.IsModelUpdated)
                {
                    _game.PacketNotifier.NotifyS2C_ChangeCharacterData(u, userId);
                }

                if (u.IsMovementUpdated())
                {
                    // TODO: Verify which one we want to use. WaypointList does not require conversions, however WaypointGroup does (and it has TeleportID functionality).
                    //_game.PacketNotifier.NotifyWaypointList(u);
                    // TODO: Verify if we want to use TeleportID.
                    _game.PacketNotifier.NotifyWaypointGroup(u, userId, false);
                }
            }
        }

        void LateUpdate(IGameObject obj, float diff)
        {
            if (obj is IAttackableUnit u)
            {
                if (u is IObjAiBase ai)
                {
                    var tempBuffs = new List<IBuff>(ai.GetBuffs());
                    for (int i = tempBuffs.Count - 1; i >= 0; i--)
                    {
                        if (tempBuffs[i].Elapsed())
                        {
                            ai.RemoveBuff(tempBuffs[i]);
                        }
                        else
                        {
                            tempBuffs[i].Update(diff);
                        }
                    }

                    // Stop targeting an untargetable unit.
                    if (ai.TargetUnit != null && !ai.TargetUnit.Status.HasFlag(StatusFlags.Targetable))
                    {
                        if(ai.TargetUnit is IObjAiBase aiTar && aiTar.CharData.IsUseable)
                        {
                            return;
                        }
                        StopTargeting(ai.TargetUnit);
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
                if (!(o is IChampion))
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
        public void RemoveObject(IGameObject o)
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
        public Dictionary<uint, IGameObject> GetObjects()
        {
            var ret = new Dictionary<uint, IGameObject>();
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
        public IGameObject GetObjectById(uint id)
        {
            IGameObject obj = _objectsToAdd.Find(o => o.NetId == id);

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
        public bool TeamHasVisionOn(TeamId team, IGameObject o)
        {
            if (o != null)
            {
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

        bool UnitHasVisionOn(IGameObject observer, IGameObject tested)
        {
            if(tested is IBaseTurret || tested is IObjBuilding)
            {
                //return true;
            }

            if(tested is IParticle particle)
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
                !(observer is IAttackableUnit u && u.IsDead)
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
        public void AddVisionProvider(IGameObject obj, TeamId team)
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
        public void RemoveVisionProvider(IGameObject obj, TeamId team)
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
        public List<IAttackableUnit> GetUnitsInRange(Vector2 checkPos, float range, bool onlyAlive = false)
        {
            var units = new List<IAttackableUnit>();
            foreach (var kv in _objects)
            {
                if (kv.Value is IAttackableUnit u && Vector2.DistanceSquared(checkPos, u.Position) <= range * range && (onlyAlive && !u.IsDead || !onlyAlive))
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
        /// </summary>
        /// <param name="target">AttackableUnit that should be untargeted.</param>
        public void StopTargeting(IAttackableUnit target)
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
                    ai.Untarget(target);
                }
            }
        }

        /// <summary>
        /// Adds a GameObject of type BaseTurret to the list of BaseTurrets in ObjectManager.
        /// </summary>
        /// <param name="turret">BaseTurret to add.</param>
        public void AddTurret(IBaseTurret turret)
        {
            _turrets.Add(turret.NetId, turret);
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
            _inhibitors.Add(inhib.NetId, inhib);
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
            _champions.Add(champion.NetId, champion);
        }

        /// <summary>
        /// Removes a GameObject of type Champion from the list of Champions in ObjectManager.
        /// </summary>
        /// <param name="champion">Champion to remove.</param>
        public void RemoveChampion(IChampion champion)
        {
            _champions.Remove(champion.NetId);
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
        public List<IChampion> GetChampionsInRangeFromTeam(Vector2 checkPos, float range, TeamId team, bool onlyAlive = false)
        {
            var champs = new List<IChampion>();
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
