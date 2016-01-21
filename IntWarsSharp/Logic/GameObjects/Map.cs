using IntWarsSharp.Core.Logic;
using IntWarsSharp.Logic.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntWarsSharp.Logic
{
    class Map
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
        protected List<Pair<bool, Tuple<long, short, bool>>> announcerEvents;
        protected Game game;
        protected bool firstBlood;
        protected bool killReduction;
        protected bool hasFountainHeal;
        protected AIMesh mesh;

        protected CollisionHandler collisionHandler;
        protected Fountain fountain;

        public publicCollisionHandler getCollisionHandler()
        {
            return collisionHandler;
        }
        public Map(Game game, long firstSpawnTime, long spawnInterval, long firstGoldTime, bool hasFountainHeal)
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
            this.announcerEvents = new List<Pair<bool, Tuple<long, short, bool>>>();
            this.game = game;
            this.firstBlood = true;
            this.killReduction = true;
            this.hasFountainHeal = hasFountainHeal;
            this.mesh = new AIMesh();
            this.collisionHandler = new CollisionHandler(this);
            this.fountain = new Fountain();
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
                    game.notifyMovement(kv.Value);
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
                    if (u.getTeam() == i)
                        continue;

                    if (visionUnits[u.getTeam()].find(u.getNetId()) != visionUnits[u.getTeam()].end() && teamHasVisionOn(i, u))
                    {
                        u.setVisibleByTeam(i, true);
                        game.notifySpawn(u);
                        visionUnits[u.getTeam()].Remove(u.getNetId());
                        game.notifyUpdatedStats(u, false);
                        continue;
                    }

                    if (!u.isVisibleByTeam(i) && teamHasVisionOn(i, u))
                    {
                        game.notifyEnterVision(u, i);
                        u.setVisibleByTeam(i, true);
                        game.notifyUpdatedStats(u, false);
                    }
                    else if (u.isVisibleByTeam(i) && !teamHasVisionOn(i, u))
                    {
                        game.notifyLeaveVision(u, i);
                        u.setVisibleByTeam(i, false);
                    }
                }

                if (u.buffs.size() != 0)
                {

                    for (int i = u.buffs.size(); i > 0; i--)
                    {

                        if (u.buffs[i - 1].needsToRemove())
                        {
                            u.buffs.erase(u->getBuffs().begin() + (i - 1));
                            //todo move this to Buff.cpp and add every stat
                            u.getStats().addMovementSpeedPercentageModifier(-u->getBuffs()[i - 1]->getMovementSpeedPercentModifier());
                            continue;
                        }
                        u->buffs[i - 1]->update(diff);
                    }
                }

                if (!u.getStats().getUpdatedStats().empty())
                {
                    game.notifyUpdatedStats(u);
                    u->getStats().clearUpdatedStats();
                }

                if (u.getStats().isUpdatedHealth())
                {
                    game.notifySetHealth(u);
                    u.getStats().clearUpdatedHealth();
                }

                if (u.isModelUpdated())
                {
                    game.notifyModelUpdate(u);
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
                    long eventTime = i.Item2.Item1;
                    short messageId = i.Item2.Item2;
                    bool isMapSpecific = i.Item2.Item3;

                    if (gameTime >= eventTime)
                    {
                        game.notifyAnnounceEvent(messageId, isMapSpecific);
                        i.Item1 = true;
                    }
                }
            }

            gameTime += diff;
            nextSyncTime += diff;

            // By default, synchronize the game time every 10 seconds
            if (nextSyncTime >= 10 * 1000000)
            {
                game->notifyGameTimer();
                nextSyncTime = 0;
            }

            if (waveNumber)
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
                fountain->healChampions(this, diff);
        }
        public virtual float getGoldPerSecond() = 0;
  public virtual bool spawn() = 0;

	publicvirtual std::pair<int, Vector2> getMinionSpawnPosition(uint32 spawnPosition) const = 0;
        public virtual void setMinionStats(Minion* minion) const = 0;

        public Object* getObjectById(uint32 id);
        public void addObject(Object* o);
        public void removeObject(Object* o);
        public const std::vector<uint32>& getExperienceToLevelUp() { return expToLevelUp; }
        public uint64 getGameTime() { return gameTime; }
        public uint64 getFirstGoldTime() { return firstGoldTime; }
        public virtual const Target getRespawnLocation(int team) const = 0;
        public virtual float getGoldFor(Unit* u) const = 0;
        public virtual float getExperienceFor(Unit* u) const = 0;

        public Game getGame() const { return game; }

    public const std::map<uint32, Object*>& getObjects() { return objects; }
    public void stopTargeting(Unit* target);

    public std::vector<Champion*> getChampionsInRange(Target* t, float range, bool isAlive = false);
    public std::vector<Unit*> getUnitsInRange(Target* t, float range, bool isAlive = false);

    public bool getFirstBlood() { return firstBlood; }
    public void setFirstBlood(bool state) { firstBlood = state; }

    public AIMesh* getAIMesh() { return &mesh; }
    public float getHeightAtLocation(float x, float y) { return mesh.getY(x, y); }
    public bool isWalkable(float x, float y) { return mesh.isWalkable(x, y); }

    public bool getKillReduction() { return killReduction; }
    public void setKillReduction(bool state) { killReduction = state; }

    public MovementVector toMovementVector(float x, float y);

    public bool teamHasVisionOn(int team, Object* o);

    public virtual const int getMapId() const = 0;
}
}
