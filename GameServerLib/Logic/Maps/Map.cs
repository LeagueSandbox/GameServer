using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace LeagueSandbox.GameServer.Logic.Maps
{
    public class Map
    {
        private Dictionary<uint, GameObject> _objects;
        private Dictionary<uint, Champion> _champions;
        private Dictionary<uint, Inhibitor> _inhibitors;
        private Dictionary<TeamId, Dictionary<uint, Unit>> _visionUnits;

        private object _objectsLock = new object();
        private object _inhibitorsLock = new object();
        private object _championsLock = new object();
        private object _visionLock = new object();

        public List<int> ExpToLevelUp { get; protected set; }
        protected int _minionNumber;
        protected long _firstSpawnTime;
        protected long _spawnInterval;
        public float GameTime { get; private set; }
        protected long _nextSpawnTime;
        public long FirstGoldTime { get; protected set; } // Time that gold should begin to generate
        protected float _nextSyncTime;
        protected List<Announce> _announcerEvents;
        protected Game _game;
        public bool HasFirstBloodHappened { get; set; }
        public bool IsKillGoldRewardReductionActive { get; set; }
        protected bool _hasFountainHeal;
        protected bool _spawnEnabled;
        public RAF.AIMesh AIMesh { get; protected set; }
        public int Id { get; private set; }

        protected CollisionHandler _collisionHandler;
        protected Dictionary<TeamId, Fountain> _fountains;
        private readonly List<TeamId> _teamsIterator;


        public Map(Game game, long firstSpawnTime, long spawnInterval, long firstGoldTime, bool hasFountainHeal, int id)
        {
            _objects = new Dictionary<uint, GameObject>();
            _inhibitors = new Dictionary<uint, Inhibitor>();
            _champions = new Dictionary<uint, Champion>();
            _visionUnits = new Dictionary<TeamId, Dictionary<uint, Unit>>();
            ExpToLevelUp = new List<int>();
            _minionNumber = 0;
            _firstSpawnTime = firstSpawnTime;
            FirstGoldTime = firstGoldTime;
            _spawnInterval = spawnInterval;
            GameTime = 0;
            _nextSpawnTime = firstSpawnTime;
            _nextSyncTime = 10 * 1000;
            _announcerEvents = new List<Announce>();
            _game = game;
            HasFirstBloodHappened = false;
            IsKillGoldRewardReductionActive = true;
            _spawnEnabled = _game.Config.MinionSpawnsEnabled;
            _hasFountainHeal = hasFountainHeal;
            _collisionHandler = new CollisionHandler(this);
            _fountains = new Dictionary<TeamId, Fountain>
            {
                { TeamId.TEAM_BLUE, new Fountain(TeamId.TEAM_BLUE, 11, 250, 1000) },
                { TeamId.TEAM_PURPLE, new Fountain(TeamId.TEAM_PURPLE, 13950, 14200, 1000) }
            };
            Id = id;

            _teamsIterator = Enum.GetValues(typeof(TeamId)).Cast<TeamId>().ToList();

            foreach (var team in _teamsIterator)
                _visionUnits.Add(team, new Dictionary<uint, Unit>());
        }

        public int GetNumberOfObjects()
        {
            return GetObjects().Count;
        }

        public virtual void Update(float diff)
        {
            var temp = GetObjects();
            foreach (var obj in temp.Values)
            {
                if (obj.isToRemove())
                {
                    if (obj.AttackerCount == 0)
                        RemoveObject(obj);
                    continue;
                }

                obj.update(diff);

                if (!(obj is Unit))
                    continue;

                var u = obj as Unit;
                foreach (var team in _teamsIterator)
                {
                    if (u.Team == team || team == TeamId.TEAM_NEUTRAL)
                        continue;

                    var visionUnitsTeam = GetVisionUnits(u.Team);
                    if (visionUnitsTeam.ContainsKey(u.NetId))
                    {
                        if (TeamHasVisionOn(team, u))
                        {
                            u.SetVisibleByTeam(team, true);
                            _game.PacketNotifier.NotifySpawn(u);
                            RemoveVisionUnit(u);
                            _game.PacketNotifier.NotifyUpdatedStats(u, false);
                            continue;
                        }
                    }

                    if (!u.IsVisibleByTeam(team) && TeamHasVisionOn(team, u))
                    {
                        u.SetVisibleByTeam(team, true);
                        _game.PacketNotifier.NotifyEnterVision(u, team);
                        _game.PacketNotifier.NotifyUpdatedStats(u, false);
                    }
                    else if (u.IsVisibleByTeam(team) && !TeamHasVisionOn(team, u))
                    {
                        u.SetVisibleByTeam(team, false);
                        _game.PacketNotifier.NotifyLeaveVision(u, team);
                    }
                }

                var tempBuffs = u.GetBuffs();
                foreach (var buff in tempBuffs.Values)
                {
                    if (buff.NeedsToRemove())
                    {
                        if (buff.Name != "")
                        {
                            _game.PacketNotifier.NotifyRemoveBuff(buff.TargetUnit, buff.Name, buff.Slot);
                        }
                        u.RemoveBuff(buff);
                        continue;
                    }
                    buff.Update(diff);
                }

                if (u.GetStats().GetUpdatedStats().Count > 0)
                {
                    _game.PacketNotifier.NotifyUpdatedStats(u, false);
                    u.GetStats().ClearUpdatedStats();
                }

                if (u.IsModelUpdated)
                {
                    _game.PacketNotifier.NotifyModelUpdate(u);
                    u.IsModelUpdated = false;
                }

                if (obj.isMovementUpdated())
                {
                    _game.PacketNotifier.NotifyMovement(obj);
                    obj.clearMovementUpdated();
                }
            }
            _collisionHandler.Update();

            foreach (var announce in _announcerEvents)
            {
                if (!announce.IsAnnounced)
                {
                    if (GameTime >= announce.EventTime)
                    {
                        announce.Execute();
                    }
                }
            }

            GameTime += diff;
            _nextSyncTime += diff;

            // By default, synchronize the game time every 10 seconds
            if (_nextSyncTime >= 10 * 1000)
            {
                _game.PacketNotifier.NotifyGameTimer();
                _nextSyncTime = 0;
            }

            if (_spawnEnabled)
            {
                if (_minionNumber > 0)
                {
                    if (GameTime >= _nextSpawnTime + _minionNumber * 8 * 100)
                    { // Spawn new wave every 0.8s
                        if (Spawn())
                        {
                            _minionNumber = 0;
                            _nextSpawnTime += _spawnInterval;
                        }
                        else
                        {
                            _minionNumber++;
                        }
                    }
                }
                else if (GameTime >= _nextSpawnTime)
                {
                    Spawn();
                    _minionNumber++;
                }
            }

            if (_hasFountainHeal)
            {
                foreach (var fountain in _fountains.Values)
                    fountain.Update(diff);
            }
        }

        public CollisionHandler GetCollisionHandler()
        {
            return _collisionHandler;
        }

        public virtual float GetGoldPerSecond()
        {
            return 0;
        }

        public void SetSpawnState(bool state)
        {
            _spawnEnabled = state;
        }

        public virtual bool Spawn()
        {
            return false;
        }

        public virtual Tuple<TeamId, Vector2> GetMinionSpawnPosition(MinionSpawnPosition spawnPosition)
        {
            return null;
        }

        public virtual float GetWidth()
        {
            return AIMesh.getWidth();
        }

        public virtual float GetHeight()
        {
            return AIMesh.getHeight();
        }

        public virtual Vector2 GetSize()
        {
            return new Vector2(GetWidth() / 2, GetHeight() / 2);
        }

        public virtual void SetMinionStats(Minion m)
        {

        }

        public GameObject GetObjectById(uint id)
        {
            if (!_objects.ContainsKey(id))
                return null;

            return _objects[id];
        }

        public Inhibitor GetInhibitorById(uint id)
        {
            if (!_inhibitors.ContainsKey(id))
                return null;

            return _inhibitors[id];
        }

        public bool AllInhibitorsDestroyedFromTeam(TeamId team)
        {
            foreach (var inhibitor in _inhibitors.Values)
            {
                if (inhibitor.Team == team && inhibitor.getState() == InhibitorState.Alive)
                {
                    return false;
                }
            }
            return true;
        }

        public void AddObject(GameObject o)
        {
            if (o == null)
                return;

            // If it crashes here the problem is most likely somewhere else
            lock (_objectsLock)
            {
                // (_objects.ContainsKey(o.NetId))
                //    _objects[o.NetId] = o;
                //else
                _objects.Add(o.NetId, o);
            }

            if (o is Inhibitor)
            {
                var i = o as Inhibitor;
                lock (_inhibitorsLock)
                {
                    _inhibitors.Add(i.NetId, i);
                }
            }

            _collisionHandler.AddObject(o);

            if (!(o is Unit))
                return;

            AddVisionUnit(o as Unit);

            if (o is Minion)
                _game.PacketNotifier.NotifyMinionSpawned(o as Minion, o.Team);
            else if (o is Monster)
                _game.PacketNotifier.NotifySpawn(o as Monster);
            else if (o is Champion)
                AddChampion(o as Champion);
            else if (o is Placeable)
                _game.PacketNotifier.NotifySpawn(o as Placeable);
            else if (o is AzirTurret)
                _game.PacketNotifier.NotifySpawn(o as AzirTurret);
        }

        public void RemoveObject(GameObject o)
        {
            lock (_objectsLock)
                _objects.Remove(o.NetId);

            if (o is Inhibitor)
            {
                lock (_inhibitorsLock)
                {
                    _inhibitors.Remove(o.NetId);
                }
            }
            //collisionHandler.stackChanged(o);
            _collisionHandler.RemoveObject(o);

            if (o is Unit)
                RemoveVisionUnit(o as Unit);

            if (o is Champion)
                RemoveChampion(o as Champion);
        }

        public void AddChampion(Champion champion)
        {
            lock (_championsLock)
                _champions.Add(champion.NetId, champion);

            _game.PacketNotifier.NotifyChampionSpawned(champion, champion.Team);
        }

        public void RemoveChampion(Champion champion)
        {
            lock (_championsLock)
                _champions.Remove(champion.NetId);
        }

        public Dictionary<uint, Unit> GetVisionUnits(TeamId team)
        {
            var ret = new Dictionary<uint, Unit>();
            lock (_visionLock)
            {
                var visionUnitsTeam = _visionUnits[team];
                foreach (var unit in visionUnitsTeam)
                    ret.Add(unit.Key, unit.Value);

                return ret;
            }
        }

        public void AddVisionUnit(Unit unit)
        {
            lock (_visionLock)
            {
                _visionUnits[unit.Team].Add(unit.NetId, unit);
            }

        }

        public void RemoveVisionUnit(Unit unit)
        {
            RemoveVisionUnit(unit.Team, unit.NetId);
        }

        public void RemoveVisionUnit(TeamId team, uint netId)
        {
            lock (_visionLock)
                _visionUnits[team].Remove(netId);
        }

        public virtual Target GetRespawnLocation(TeamId team)
        {
            return null;
        }

        public virtual float GetGoldFor(Unit u)
        {
            return 0;
        }

        public virtual float GetExperienceFor(Unit u)
        {
            return 0;
        }

        public Dictionary<uint, GameObject> GetObjects()
        {
            var ret = new Dictionary<uint, GameObject>();
            lock (_objectsLock)
                foreach (var obj in _objects)
                    ret.Add(obj.Key, obj.Value);

            return ret;
        }

        public Dictionary<uint, Inhibitor> GetInhibitors()
        {
            var ret = new Dictionary<uint, Inhibitor>();
            lock (_inhibitorsLock)
                foreach (var inhib in _inhibitors)
                    ret.Add(inhib.Key, inhib.Value);

            return ret;
        }

        public void StopTargeting(Unit target)
        {
            lock (_objectsLock)
            {
                foreach (var kv in _objects)
                {
                    var u = kv.Value as Unit;
                    if (u == null)
                        continue;

                    if (u.TargetUnit == target)
                    {
                        u.TargetUnit = null;
                        u.AutoAttackTarget = null;
                        _game.PacketNotifier.NotifySetTarget(u, null);
                    }
                }
            }
        }

        public List<Champion> GetAllChampionsFromTeam(TeamId team)
        {
            var champs = new List<Champion>();
            foreach (var kv in _champions)
            {
                Champion c = kv.Value;
                if (c.Team == team)
                    champs.Add(c);
            }
            return champs;
        }

        public List<Champion> GetChampionsInRange(float x, float y, float range, bool onlyAlive = false)
        {
            return GetChampionsInRange(new Target(x, y), range, onlyAlive);
        }

        public List<Champion> GetChampionsInRange(Target t, float range, bool onlyAlive = false)
        {
            var champs = new List<Champion>();
            lock (_championsLock)
            {
                foreach (var kv in _champions)
                {
                    var c = kv.Value;
                    if (t.GetDistanceTo(c) <= range)
                        if (onlyAlive && !c.IsDead || !onlyAlive)
                            champs.Add(c);
                }
            }
            return champs;
        }

        public List<Unit> GetUnitsInRange(float x, float y, float range, bool onlyAlive = false)
        {
            return GetUnitsInRange(new Target(x, y), range, onlyAlive);
        }

        public List<Unit> GetUnitsInRange(Target t, float range, bool onlyAlive = false)
        {
            var units = new List<Unit>();
            lock (_objectsLock)
            {
                foreach (var kv in _objects)
                {
                    var u = kv.Value as Unit;
                    if (u != null && t.GetDistanceTo(u) <= range)
                        if ((onlyAlive && !u.IsDead) || !onlyAlive)
                            units.Add(u);
                }
            }
            return units;
        }

        public float GetHeightAtLocation(float x, float y)
        {
            return AIMesh.getY(x, y);
        }

        public float GetHeightAtLocation(Vector2 loc)
        {
            return AIMesh.getY(loc.X, loc.Y);
        }

        public bool IsWalkable(float x, float y)
        {
            return AIMesh.isWalkable(x, y);
        }

        public MovementVector ToMovementVector(float x, float y)
        {
            return new MovementVector((short)((x - AIMesh.getWidth() / 2) / 2), (short)((y - AIMesh.getHeight() / 2) / 2));
        }

        public bool TeamHasVisionOn(TeamId team, GameObject o)
        {
            if (o == null)
                return false;

            if (o.Team == team)
                return true;

            lock (_objectsLock)
            {
                foreach (var kv in _objects)
                {//TODO: enable mesh as soon as it works again
                    if (kv.Value.Team == team && kv.Value.GetDistanceTo(o) < kv.Value.VisionRadius /*&& !mesh.isAnythingBetween(kv.Value, o)*/)
                    {
                        var unit = kv.Value as Unit;
                        if (unit != null && unit.IsDead)
                            continue;
                        return true;
                    }
                }
            }
            return false;
        }

        public virtual int GetMapId()
        {
            return 0;
        }

        public virtual int GetBluePillId()
        {
            return 0;
        }

        public virtual float[] GetEndGameCameraPosition(TeamId team)
        {
            return new float[] { 0, 0, 0 };
        }
    }
}
