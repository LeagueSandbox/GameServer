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
        protected Dictionary<uint, GameObject> objects;
        protected Dictionary<uint, Champion> champions;
        protected Dictionary<TeamId, Dictionary<uint, Unit>> visionUnits; //array of 3
        protected List<int> expToLevelUp;
        protected int waveNumber;
        protected long firstSpawnTime;
        protected long spawnInterval;
        protected long gameTime;
        protected long nextSpawnTime;
        protected long firstGoldTime; // Time that gold should begin to generate
        protected long nextSyncTime;
        protected List<GameObjects.Announce> _announcerEvents;
        protected Game game;
        protected bool firstBlood;
        protected bool killReduction;
        protected bool hasFountainHeal;
        protected LeagueSandbox.GameServer.Logic.RAF.AIMesh mesh;
        protected int id;

        protected CollisionHandler collisionHandler;
        protected Dictionary<TeamId, Fountain> _fountains;
        private readonly List<TeamId> TeamsIterator;


        public Map(Game game, long firstSpawnTime, long spawnInterval, long firstGoldTime, bool hasFountainHeal, int id)
        {
            this.objects = new Dictionary<uint, GameObject>();
            this.champions = new Dictionary<uint, Champion>();
            this.visionUnits = new Dictionary<TeamId, Dictionary<uint, Unit>>();
            this.expToLevelUp = new List<int>();
            this.waveNumber = 0;
            this.firstSpawnTime = firstSpawnTime;
            this.firstGoldTime = firstGoldTime;
            this.spawnInterval = spawnInterval;
            this.gameTime = 0;
            this.nextSpawnTime = firstSpawnTime;
            this.nextSyncTime = 10 * 1000;
            _announcerEvents = new List<GameObjects.Announce>();
            this.game = game;
            this.firstBlood = true;
            this.killReduction = true;
            this.hasFountainHeal = hasFountainHeal;
            this.collisionHandler = new CollisionHandler(this);
            _fountains = new Dictionary<TeamId, Fountain>();
            _fountains.Add(TeamId.TEAM_BLUE, new Fountain(TeamId.TEAM_BLUE, 11, 250, 1000));
            _fountains.Add(TeamId.TEAM_PURPLE, new Fountain(TeamId.TEAM_PURPLE, 13950, 14200, 1000));
            this.id = id;

            TeamsIterator = Enum.GetValues(typeof(TeamId)).Cast<TeamId>().ToList();

            foreach (var team in TeamsIterator)
                visionUnits.Add(team, new Dictionary<uint, Unit>());

        }

        public virtual void update(long diff)
        {
            var temp = objects.ToList();

            foreach (var kv in temp)
            {
                if (kv.Value.isToRemove())
                {
                    if (kv.Value.getAttackerCount() == 0)
                    {
                        //collisionHandler.stackChanged(kv.Value);
                        collisionHandler.removeObject(kv.Value);
                        lock (objects)
                            objects.Remove(kv.Key);
                    }
                    continue;
                }

                if (kv.Value.isMovementUpdated())
                {
                    PacketNotifier.notifyMovement(kv.Value);
                    kv.Value.clearMovementUpdated();
                }

                var u = kv.Value as Unit;

                if (u == null)
                {
                    kv.Value.update(diff);
                    continue;
                }

                foreach (var team in TeamsIterator)
                {
                    if (u.getTeam() == team || team == TeamId.TEAM_NEUTRAL)
                        continue;

                    var visionUnitsTeam = visionUnits[u.getTeam()];
                    if (visionUnitsTeam.ContainsKey(u.getNetId()))
                    {
                        if (teamHasVisionOn(team, u))
                        {
                            u.setVisibleByTeam(team, true);
                            PacketNotifier.notifySpawn(u);
                            visionUnitsTeam.Remove(u.getNetId());
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

                if (u.buffs.Count != 0)
                {
                    var tempBuffs = u.buffs.ToList();
                    for (int i = tempBuffs.Count; i > 0; i--)
                    {
                        if (tempBuffs[i - 1].needsToRemove())
                        {
                            u.buffs.Remove(tempBuffs[i - 1]);
                            //todo move this to Buff.cpp and add every stat
                            u.getStats().addMovementSpeedPercentageModifier(-tempBuffs[i - 1].getMovementSpeedPercentModifier());
                            continue;
                        }
                        tempBuffs[i - 1].update(diff);
                    }
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

                kv.Value.update(diff);
            }

            collisionHandler.update(diff);

            foreach (var announce in _announcerEvents)
                if (!announce.IsAnnounced())
                    if (gameTime >= announce.GetEventTime())
                        announce.Execute();

            gameTime += diff;
            nextSyncTime += diff;

            // By default, synchronize the game time every 10 seconds
            if (nextSyncTime >= 10 * 1000)
            {
                PacketNotifier.notifyGameTimer();
                nextSyncTime = 0;
            }

            if (waveNumber > 0)
            {
                if (gameTime >= nextSpawnTime + waveNumber * 8 * 100)
                { // Spawn new wave every 0.8s
                    if (spawn())
                    {
                        waveNumber = 0;
                        nextSpawnTime += spawnInterval;
                    }
                    else
                    {
                        ++waveNumber;
                    }
                }
            }
            else if (gameTime >= nextSpawnTime)
            {
                spawn();
                ++waveNumber;
            }

            if (hasFountainHeal)
            {
                foreach (var fountain in _fountains.Values)
                    fountain.Update(this, diff);
            }
        }

        public CollisionHandler getCollisionHandler()
        {
            return collisionHandler;
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

        public GameObject getObjectById(uint id)
        {
            if (!objects.ContainsKey(id))
                return null;

            return objects[id];
        }
        public void addObject(GameObject o)
        {
            if (o == null)
                return;

            lock (objects)
            {
                if (objects.ContainsKey(o.getNetId()))
                    objects[o.getNetId()] = o;
                else
                    objects.Add(o.getNetId(), o);
            }

            var u = o as Unit;
            if (u == null)
                return;

            collisionHandler.addObject(o);
            var teamVision = visionUnits[o.getTeam()];
            if (teamVision.ContainsKey(o.getNetId()))
                teamVision[o.getNetId()] = u;
            else
                teamVision.Add(o.getNetId(), u);

            var m = u as Minion;
            if (m != null)
                PacketNotifier.notifyMinionSpawned(m, m.getTeam());

            var mo = u as Monster;
            if (mo != null)
                PacketNotifier.notifySpawn(mo);

            var c = o as Champion;
            if (c != null)
            {
                champions[c.getNetId()] = c;
                PacketNotifier.notifyChampionSpawned(c, c.getTeam());
            }
        }
        public void removeObject(GameObject o)
        {
            var c = o as Champion;

            if (c != null)
                champions.Remove(c.getNetId());

            lock (objects)
                objects.Remove(o.getNetId());
            visionUnits[o.getTeam()].Remove(o.getNetId());
        }

        public Dictionary<uint, Unit> getVisionUnits(TeamId team)
        {
            return visionUnits[team];
        }

        public List<int> getExperienceToLevelUp()
        {
            return expToLevelUp;
        }

        public long getGameTime()
        {
            return gameTime;
        }
        public int getId()
        {
            return id;
        }

        public long getFirstGoldTime()
        {
            return firstGoldTime;
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

        public Game getGame()
        {
            return game;
        }

        public Dictionary<uint, GameObject> getObjects()
        {
            return objects;
        }

        public void stopTargeting(Unit target)
        {
            foreach (var kv in objects)
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

        public List<Champion> getChampionsInRange(float x, float y, float range, bool onlyAlive = false)
        {
            return getChampionsInRange(new Target(x, y), range, onlyAlive);
        }

        public List<Champion> getChampionsInRange(GameObjects.Target t, float range, bool onlyAlive = false)
        {
            var champs = new List<Champion>();
            foreach (var kv in champions)
            {
                var c = kv.Value;
                if (t.distanceWith(c) <= range)
                    if (onlyAlive && !c.isDead() || !onlyAlive)
                        champs.Add(c);
            }
            return champs;
        }

        public List<Unit> getUnitsInRange(GameObjects.Target t, float range, bool isAlive = false)
        {
            var units = new List<Unit>();
            foreach (var kv in objects)
            {
                var u = kv.Value as Unit;
                if (u != null && t.distanceWith(u) <= range)
                    if (isAlive && !u.isDead() || !isAlive)
                        units.Add(u);
            }
            return units;
        }

        public bool getFirstBlood()
        {
            return firstBlood;
        }

        public void setFirstBlood(bool state)
        {
            firstBlood = state;
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

        public bool getKillReduction()
        {
            return killReduction;
        }
        public void setKillReduction(bool state)
        {
            killReduction = state;
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

            lock (objects)
            {
                foreach (var kv in objects)
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
