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
        protected LeagueSandbox.GameServer.Logic.RAF.AIMesh mesh;
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

        public virtual void update(long diff)
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
                    PacketNotifier.notifyMovement(obj);
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
                        if (teamHasVisionOn(team, u))
                        {
                            u.setVisibleByTeam(team, true);
                            PacketNotifier.notifySpawn(u);
                            RemoveVisionUnit(u);
                            PacketNotifier.notifyUpdatedStats(u, false);
                            continue;
                        }
                    }

                    if (!u.isVisibleByTeam(team) && teamHasVisionOn(team, u))
                    {
                        PacketNotifier.notifyEnterVision(u, team);
                        u.setVisibleByTeam(team, true);
                        PacketNotifier.notifyUpdatedStats(u, false);
                    }
                    else if (u.isVisibleByTeam(team) && !teamHasVisionOn(team, u))
                    {
                        PacketNotifier.notifyLeaveVision(u, team);
                        u.setVisibleByTeam(team, false);
                    }
                }

                var tempBuffs = u.GetBuffs();
                foreach (var buff in tempBuffs.Values)
                {
                    if (buff.needsToRemove())
                    {
                        u.RemoveBuff(buff);
                        continue;
                    }
                    buff.update(diff);
                }

                if (u.getStats().getUpdatedStats().Count > 0)
                {
                    PacketNotifier.notifyUpdatedStats(u);
                    u.getStats().clearUpdatedStats();
                }

                if (u.getStats().isUpdatedHealth())
                {
                    PacketNotifier.notifySetHealth(u);
                    u.getStats().clearUpdatedHealth();
                }

                if (u.isModelUpdated())
                {
                    PacketNotifier.notifyModelUpdate(u);
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
                PacketNotifier.notifyGameTimer();
                _nextSyncTime = 0;
            }

            if (_waveNumber > 0)
            {
                if (_gameTime >= _nextSpawnTime + _waveNumber * 8 * 100)
                { // Spawn new wave every 0.8s
                    if (spawn())
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
                spawn();
                _waveNumber++;
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

        public virtual float getGoldPerSecond()
        {
            return 0;
        }

        public virtual bool spawn()
        {
            return false;
        }

        public virtual Tuple<TeamId, Vector2> getMinionSpawnPosition(MinionSpawnPosition spawnPosition)
        {
            return null;
        }

        public virtual int getWidth()
        {
            return 0;
        }

        public virtual int getHeight()
        {
            return 0;
        }

        public virtual Vector2 getSize()
        {
            return new Vector2(0, 0);
        }

        public virtual void setMinionStats(Minion minion)
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
                PacketNotifier.notifyMinionSpawned(o as Minion, o.getTeam());
            else if (o is Monster)
                PacketNotifier.notifySpawn(o as Monster);
            else if (o is Champion)
                AddChampion(o as Champion);
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

            PacketNotifier.notifyChampionSpawned(champion, champion.getTeam());
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

        public long getGameTime()
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

        public virtual Target getRespawnLocation(int team)
        {
            return null;
        }

        public virtual float getGoldFor(Unit u)
        {
            return 0;
        }

        public virtual float getExperienceFor(Unit u)
        {
            return 0;
        }

        public Game GetGame()
        {
            return _game;
        }

        public Dictionary<uint, GameObject> GetObjects()
        {
            var ret = new Dictionary<uint, GameObject>();
            lock (_objectsLock)
                foreach (var obj in _objects)
                    ret.Add(obj.Key, obj.Value);

            return ret;
        }

        public void stopTargeting(Unit target)
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
                        PacketNotifier.notifySetTarget(u, null);
                    }
                }
            }
        }

        public List<Champion> getChampionsInRange(float x, float y, float range, bool onlyAlive = false)
        {
            return getChampionsInRange(new Target(x, y), range, onlyAlive);
        }

        public List<Champion> getChampionsInRange(GameObjects.Target t, float range, bool onlyAlive = false)
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

        public List<Unit> getUnitsInRange(GameObjects.Target t, float range, bool isAlive = false)
        {
            var units = new List<Unit>();
            lock (_objectsLock)
            {
                foreach (var kv in _objects)
                {
                    var u = kv.Value as Unit;
                    if (u != null && t.distanceWith(u) <= range)
                        if (isAlive && !u.isDead() || !isAlive)
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

        public LeagueSandbox.GameServer.Logic.RAF.AIMesh getAIMesh()
        {
            return mesh;
        }

        public float getHeightAtLocation(float x, float y)
        {
            return mesh.getY(x, y);
        }
        public bool isWalkable(float x, float y)
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

        public MovementVector toMovementVector(float x, float y)
        {
            return new MovementVector((int)((x - mesh.getWidth() / 2) / 2), (int)((y - mesh.getHeight() / 2) / 2));
        }

        public bool teamHasVisionOn(TeamId team, GameObject o)
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

        public virtual int getMapId()
        {
            return 0;
        }

        public virtual int getBluePillId()
        {
            return 0;
        }

        public virtual float[] GetEndGameCameraPosition(TeamId team)
        {
            return new float[] { 0, 0, 0 };
        }
    }
}
