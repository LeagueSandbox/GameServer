using InibinSharp;
using IntWarsSharp.Core.Logic;
using IntWarsSharp.Logic.Enet;
using IntWarsSharp.Logic.GameObjects;
using IntWarsSharp.Logic.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace IntWarsSharp.Logic.Maps
{
    public class Map
    {
        protected Dictionary<int, GameObject> objects;
        protected Dictionary<int, Champion> champions;
        protected Dictionary<int, Unit>[] visionUnits; //array of 3
        protected List<int> expToLevelUp;
        protected int waveNumber;
        protected long firstSpawnTime;
        protected long spawnInterval;
        protected long gameTime;
        protected long nextSpawnTime;
        protected long firstGoldTime; // Time that gold should begin to generate
        protected long nextSyncTime;
        protected List<Pair<bool, Tuple<long, byte, bool>>> announcerEvents;
        protected Game game;
        protected bool firstBlood;
        protected bool killReduction;
        protected bool hasFountainHeal;
        protected IntWarsSharp.Logic.RAF.AIMesh mesh;
        protected int id;

        protected CollisionHandler collisionHandler;
        protected Fountain fountain;


        public Map(Game game, long firstSpawnTime, long spawnInterval, long firstGoldTime, bool hasFountainHeal, int id)
        {
            this.objects = new Dictionary<int, GameObject>();
            this.champions = new Dictionary<int, Champion>();
            this.visionUnits = new Dictionary<int, Unit>[3];
            this.expToLevelUp = new List<int>();
            this.waveNumber = 0;
            this.firstSpawnTime = firstSpawnTime;
            this.firstGoldTime = firstGoldTime;
            this.spawnInterval = spawnInterval;
            this.gameTime = 0;
            this.nextSpawnTime = firstSpawnTime;
            this.nextSyncTime = 10 * 1000000;
            this.announcerEvents = new List<Pair<bool, Tuple<long, byte, bool>>>();
            this.game = game;
            this.firstBlood = true;
            this.killReduction = true;
            this.hasFountainHeal = hasFountainHeal;
            this.collisionHandler = new CollisionHandler(this);
            this.fountain = new Fountain();
            this.id = id;

            for (var i = 0; i < visionUnits.Length; i++)
                visionUnits[i] = new Dictionary<int, Unit>();
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

                for (var i = 0; i < 2; ++i)
                {
                    var teamId = (TeamId)i;
                    if (u.getTeam() == teamId)
                        continue;

                    var visionUnitsTeam = visionUnits[(int)u.getTeam()];
                    if (visionUnitsTeam.ContainsKey(u.getNetId()))
                    {
                        var s = visionUnitsTeam[u.getNetId()];
                        if (s != visionUnitsTeam.Last().Value && teamHasVisionOn(teamId, u))
                        {
                            u.setVisibleByTeam(i, true);
                            PacketNotifier.notifySpawn(u);
                            visionUnitsTeam.Remove(u.getNetId());
                            PacketNotifier.notifyUpdatedStats(u, false);
                            continue;
                        }
                    }

                    if (!u.isVisibleByTeam(teamId) && teamHasVisionOn(teamId, u))
                    {
                        PacketNotifier.notifyEnterVision(u, teamId);
                        u.setVisibleByTeam(i, true);
                        PacketNotifier.notifyUpdatedStats(u, false);
                    }
                    else if (u.isVisibleByTeam(teamId) && !teamHasVisionOn(teamId, u))
                    {
                        PacketNotifier.notifyLeaveVision(u, teamId);
                        u.setVisibleByTeam(i, false);
                    }
                }

                if (u.buffs.Count != 0)
                {
                    var toRemove = new List<Buff>();
                    for (int i = u.buffs.Count; i > 0; i--)
                    {
                        if (u.buffs[i - 1].needsToRemove())
                        {
                            toRemove.Add(u.buffs[i - 1]);
                            //todo move this to Buff.cpp and add every stat
                            u.getStats().addMovementSpeedPercentageModifier(-u.getBuffs()[i - 1].getMovementSpeedPercentModifier());
                            continue;
                        }
                        u.buffs[i - 1].update(diff);
                    }
                    foreach (var i in toRemove)
                        u.buffs.Remove(i);
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

            foreach (var i in announcerEvents)
            {
                bool isCompleted = i.Item1;

                if (!isCompleted)
                {
                    var eventTime = i.Item2.Item1;
                    var messageId = i.Item2.Item2;
                    var isMapSpecific = i.Item2.Item3;

                    if (gameTime >= eventTime)
                    {
                        PacketNotifier.notifyAnnounceEvent(messageId, isMapSpecific);
                        i.Item1 = true;
                    }
                }
            }

            gameTime += diff;
            nextSyncTime += diff;

            // By default, synchronize the game time every 10 seconds
            if (nextSyncTime >= 10 * 1000000)
            {
                PacketNotifier.notifyGameTimer();
                nextSyncTime = 0;
            }

            if (waveNumber > 0)
            {
                if (gameTime >= nextSpawnTime + waveNumber * 8 * 100000)
                { // Spawn new wave every 0.8s
                    if (spawn())
                    {
                        waveNumber = 0;
                        nextSpawnTime += spawnInterval;
                    }
                    else {
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
                fountain.healChampions(this, diff);
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

        public virtual Tuple<int, Vector2> getMinionSpawnPosition(MinionSpawnPosition spawnPosition)
        {
            return null;
        }

        public virtual void setMinionStats(Minion minion)
        {

        }

        public GameObject getObjectById(int id)
        {
            if (!objects.ContainsKey(id))
                return null;

            return objects[id];
        }
        public void addObject(GameObject o)
        {
            if (o == null)
                return;

            objects.Add(o.getNetId(), o);

            var u = o as Unit;
            if (u == null)
                return;

            collisionHandler.addObject(o);

            var teamVision = visionUnits[(int)o.getTeam()];
            if (teamVision.ContainsKey(o.getNetId()))
                teamVision[o.getNetId()] = u;
            else
                teamVision.Add(o.getNetId(), u);

            var m = u as Minion;

            if (m != null)
                PacketNotifier.notifyMinionSpawned(m, m.getTeam());

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

            objects.Remove(o.getNetId());
            visionUnits[(int)o.getTeam()].Remove(o.getNetId());
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

        public Dictionary<int, GameObject> getObjects()
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

        public List<Champion> getChampionsInRange(GameObjects.Target t, float range, bool isAlive = false)
        {
            var champs = new List<Champion>();
            foreach (var kv in champions)
            {
                var c = kv.Value;
                if (t.distanceWith(c) <= range)
                    if (isAlive && !c.isDead() || !isAlive) //TODO: check
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

        public IntWarsSharp.Logic.RAF.AIMesh getAIMesh()
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

            foreach (var kv in objects)
            {
                if (kv.Value.getTeam() == team && kv.Value.distanceWith(o) < kv.Value.getVisionRadius() && !mesh.isAnythingBetween(kv.Value, o))
                {
                    var unit = kv.Value as Unit;
                    if (unit != null && unit.isDead())
                        continue;
                    return true;
                }
            }

            return false;
        }

        public virtual int getMapId()
        {
            return 0;
        }
    }
}
