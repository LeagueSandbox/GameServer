using InibinSharp;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Maps
{
    public class Map
    {
        private Dictionary<uint, GameObject> _objects;
        private Dictionary<uint, Champion> _champions;
        private Dictionary<TeamId, Dictionary<uint, Unit>> _visionUnits;

        private object _objectsLock = new object();
        private object _championsLock = new object();
        private object _visionLock = new object();

        protected List<int> _expToLevelUp;
        protected int _waveNumber;
        protected long _firstSpawnTime;
        protected long _spawnInterval;
        protected long _gameTime;
        protected long _nextSpawnTime;
        protected long _firstGoldTime; // Time that gold should begin to generate
        protected long _nextSyncTime;
        protected List<GameObjects.Announce> _announcerEvents;
        protected Game _game;
        protected bool _firstBlood;
        protected bool _killReduction;
        protected bool _hasFountainHeal;
        protected bool _spawnEnabled;
        protected RAF.AIMesh mesh;
        protected int _id;

        protected CollisionHandler _collisionHandler;
        protected Dictionary<TeamId, Fountain> _fountains;
        private readonly List<TeamId> _teamsIterator;


        public Map(Game game, long firstSpawnTime, long spawnInterval, long firstGoldTime, bool hasFountainHeal, int id)
        {
            _objects = new Dictionary<uint, GameObject>();
            _champions = new Dictionary<uint, Champion>();
            _visionUnits = new Dictionary<TeamId, Dictionary<uint, Unit>>();
            _expToLevelUp = new List<int>();
            _waveNumber = 0;
            _firstSpawnTime = firstSpawnTime;
            _firstGoldTime = firstGoldTime;
            _spawnInterval = spawnInterval;
            _gameTime = 0;
            _nextSpawnTime = firstSpawnTime;
            _nextSyncTime = 10 * 1000;
            _announcerEvents = new List<GameObjects.Announce>();
            _game = game;
            _firstBlood = true;
            _killReduction = true;
            _spawnEnabled = false;
            _hasFountainHeal = hasFountainHeal;
            _collisionHandler = new CollisionHandler(this);
            _fountains = new Dictionary<TeamId, Fountain>();
            _fountains.Add(TeamId.TEAM_BLUE, new Fountain(TeamId.TEAM_BLUE, 11, 250, 1000));
            _fountains.Add(TeamId.TEAM_PURPLE, new Fountain(TeamId.TEAM_PURPLE, 13950, 14200, 1000));
            _id = id;

            _teamsIterator = Enum.GetValues(typeof(TeamId)).Cast<TeamId>().ToList();

            foreach (var team in _teamsIterator)
                _visionUnits.Add(team, new Dictionary<uint, Unit>());

        }

        public virtual void Update(long diff)
        {
            var temp = GetObjects();
            foreach (var obj in temp.Values)
            {
                if (obj.isToRemove())
                {
                    if (obj.getAttackerCount() == 0)
                        RemoveObject(obj);
                    continue;
                }

                if (obj.isMovementUpdated())
                {
                    _game.PacketNotifier.notifyMovement(obj);
                    obj.clearMovementUpdated();
                }

                obj.update(diff);

                if (!(obj is Unit))
                    continue;

                var u = obj as Unit;
                foreach (var team in _teamsIterator)
                {
                    if (u.getTeam() == team || team == TeamId.TEAM_NEUTRAL)
                        continue;

                    var visionUnitsTeam = GetVisionUnits(u.getTeam());
                    if (visionUnitsTeam.ContainsKey(u.getNetId()))
                    {
                        if (TeamHasVisionOn(team, u))
                        {
                            u.setVisibleByTeam(team, true);
                            _game.PacketNotifier.notifySpawn(u);
                            RemoveVisionUnit(u);
                            _game.PacketNotifier.notifyUpdatedStats(u, false);
                            continue;
                        }
                    }

                    if (!u.isVisibleByTeam(team) && TeamHasVisionOn(team, u))
                    {
                        _game.PacketNotifier.notifyEnterVision(u, team);
                        u.setVisibleByTeam(team, true);
                        _game.PacketNotifier.notifyUpdatedStats(u, false);
                    }
                    else if (u.isVisibleByTeam(team) && !TeamHasVisionOn(team, u))
                    {
                        _game.PacketNotifier.notifyLeaveVision(u, team);
                        u.setVisibleByTeam(team, false);
                    }
                }

                var tempBuffs = u.GetBuffs();
                foreach (var buff in tempBuffs.Values)
                {
                    if (buff.NeedsToRemove())
                    {
                        if (buff.GetName() != "")
                        {
                            _game.PacketNotifier.notifyRemoveBuff(buff.GetUnit(), buff.GetName());
                        }
                        u.RemoveBuff(buff);
                        continue;
                    }
                    buff.Update(diff);
                }

                if (u.GetStats().GetUpdatedStats().Count > 0)
                {
                    _game.PacketNotifier.notifyUpdatedStats(u);
                    u.GetStats().ClearUpdatedStats();
                }

                if (u.GetStats().IsUpdatedHealth())
                {
                    _game.PacketNotifier.notifySetHealth(u);
                }

                if (u.isModelUpdated())
                {
                    _game.PacketNotifier.notifyModelUpdate(u);
                    u.clearModelUpdated();
                }
            }

            _collisionHandler.update(diff);

            foreach (var announce in _announcerEvents)
                if (!announce.IsAnnounced())
                    if (_gameTime >= announce.GetEventTime())
                        announce.Execute();

            _gameTime += diff;
            _nextSyncTime += diff;

            // By default, synchronize the game time every 10 seconds
            if (_nextSyncTime >= 10 * 1000)
            {
                _game.PacketNotifier.notifyGameTimer();
                _nextSyncTime = 0;
            }

            if (_spawnEnabled)
            {
                if (_waveNumber > 0)
                {
                    if (_gameTime >= _nextSpawnTime + _waveNumber * 8 * 100)
                    { // Spawn new wave every 0.8s
                        if (Spawn())
                        {
                            _waveNumber = 0;
                            _nextSpawnTime += _spawnInterval;
                        }
                        else
                        {
                            _waveNumber++;
                        }
                    }
                }
                else if (_gameTime >= _nextSpawnTime)
                {
                    Spawn();
                    _waveNumber++;
                }
            }

            if (_hasFountainHeal)
            {
                foreach (var fountain in _fountains.Values)
                    fountain.Update(this, diff);
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
            return mesh.getWidth();
        }

        public virtual float GetHeight()
        {
            return mesh.getHeight();
        }

        public virtual Vector2 GetSize()
        {
            return new Vector2(GetWidth() / 2, GetHeight() / 2);
        }

        public virtual void SetMinionStats(Minion minion)
        {

        }

        public GameObject GetObjectById(uint id)
        {
            if (!_objects.ContainsKey(id))
                return null;

            return _objects[id];
        }

        public void AddObject(GameObject o)
        {
            if (o == null)
                return;

            // If it crashes here the problem is most likely somewhere else
            lock (_objectsLock)
            {
                // (_objects.ContainsKey(o.getNetId()))
                //    _objects[o.getNetId()] = o;
                //else
                _objects.Add(o.getNetId(), o);
            }

            _collisionHandler.addObject(o);

            if (!(o is Unit))
                return;

            AddVisionUnit(o as Unit);

            if (o is Minion)
                _game.PacketNotifier.notifyMinionSpawned(o as Minion, o.getTeam());
            else if (o is Monster)
                _game.PacketNotifier.notifySpawn(o as Monster);
            else if (o is Champion)
                AddChampion(o as Champion);
            else if (o is Placeable)
                _game.PacketNotifier.notifySpawn(o as Placeable);
        }

        public void RemoveObject(GameObject o)
        {
            lock (_objectsLock)
                _objects.Remove(o.getNetId());

            //collisionHandler.stackChanged(o);
            _collisionHandler.removeObject(o);

            if (o is Unit)
                RemoveVisionUnit(o as Unit);

            if (o is Champion)
                RemoveChampion(o as Champion);
        }

        public void AddChampion(Champion champion)
        {
            lock (_championsLock)
                _champions.Add(champion.getNetId(), champion);

            _game.PacketNotifier.notifyChampionSpawned(champion, champion.getTeam());
        }

        public void RemoveChampion(Champion champion)
        {
            lock (_championsLock)
                _champions.Remove(champion.getNetId());
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
                _visionUnits[unit.getTeam()].Add(unit.getNetId(), unit);
        }

        public void RemoveVisionUnit(Unit unit)
        {
            RemoveVisionUnit(unit.getTeam(), unit.getNetId());
        }

        public void RemoveVisionUnit(TeamId team, uint netId)
        {
            lock (_visionLock)
                _visionUnits[team].Remove(netId);
        }

        public List<int> GetExperienceToLevelUp()
        {
            return _expToLevelUp;
        }

        public long GetGameTime()
        {
            return _gameTime;
        }

        public int GetId()
        {
            return _id;
        }

        public long GetFirstGoldTime()
        {
            return _firstGoldTime;
        }

        public virtual Target GetRespawnLocation(int team)
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

        public void StopTargeting(Unit target)
        {
            lock (_objectsLock)
            {
                foreach (var kv in _objects)
                {
                    var u = kv.Value as Unit;
                    if (u == null)
                        continue;

                    if (u.getTargetUnit() == target)
                    {
                        u.setTargetUnit(null);
                        u.setAutoAttackTarget(null);
                        _game.PacketNotifier.notifySetTarget(u, null);
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
                if (c.getTeam() == team)
                    champs.Add(c);
            }
            return champs;
        }

        public List<Champion> GetChampionsInRange(float x, float y, float range, bool onlyAlive = false)
        {
            return GetChampionsInRange(new Target(x, y), range, onlyAlive);
        }

        public List<Champion> GetChampionsInRange(GameObjects.Target t, float range, bool onlyAlive = false)
        {
            var champs = new List<Champion>();
            lock (_championsLock)
            {
                foreach (var kv in _champions)
                {
                    var c = kv.Value;
                    if (t.distanceWith(c) <= range)
                        if (onlyAlive && !c.isDead() || !onlyAlive)
                            champs.Add(c);
                }
            }
            return champs;
        }
		
        public List<Unit> GetUnitsInRange(float x, float y, float range, bool onlyAlive = false)
        {
            return GetUnitsInRange(new Target(x, y), range, onlyAlive);
        }

        public List<Unit> GetUnitsInRange(GameObjects.Target t, float range, bool onlyAlive = false)
        {
            var units = new List<Unit>();
            lock (_objectsLock)
            {
                foreach (var kv in _objects)
                {
                    var u = kv.Value as Unit;
                    if (u != null && t.distanceWith(u) <= range)
                        if (onlyAlive && !u.isDead() || !onlyAlive)
                            units.Add(u);
                }
            }
            return units;
        }

        public bool GetFirstBlood()
        {
            return _firstBlood;
        }

        public void SetFirstBlood(bool state)
        {
            _firstBlood = state;
        }

        public RAF.AIMesh getAIMesh()
        {
            return mesh;
        }

        public float GetHeightAtLocation(float x, float y)
        {
            return mesh.getY(x, y);
        }
        public bool IsWalkable(float x, float y)
        {
            return mesh.isWalkable(x, y);
        }

        public bool GetKillReduction()
        {
            return _killReduction;
        }
        public void SetKillReduction(bool state)
        {
            _killReduction = state;
        }

        public MovementVector ToMovementVector(float x, float y)
        {
            return new MovementVector((short)((x - mesh.getWidth() / 2) / 2), (short)((y - mesh.getHeight() / 2) / 2));
        }

        public bool TeamHasVisionOn(TeamId team, GameObject o)
        {
            if (o == null)
                return false;

            if (o.getTeam() == team)
                return true;

            lock (_objectsLock)
            {
                foreach (var kv in _objects)
                {//TODO: enable mesh as soon as it works again
                    if (kv.Value.getTeam() == team && kv.Value.distanceWith(o) < kv.Value.getVisionRadius() /*&& !mesh.isAnythingBetween(kv.Value, o)*/)
                    {
                        var unit = kv.Value as Unit;
                        if (unit != null && unit.isDead())
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
