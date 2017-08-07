using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;

namespace LeagueSandbox.GameServer.Logic.Maps
{
    public class Map
    {
        public List<int> ExpToLevelUp { get; protected set; }
        protected float _nextSyncTime;
        protected List<Announce> _announcerEvents;
        protected bool _hasFountainHeal;
        
        protected Game _game;
        static protected Logger _logger = Program.ResolveDependency<Logger>();
        public bool HasFirstBloodHappened { get; set; }
        public bool IsKillGoldRewardReductionActive { get; set; }
        public RAF.AIMesh AIMesh { get; protected set; }
        public int Id { get; private set; } = 0;
        public int BluePillId { get; protected set; } = 0;
        public long FirstGoldTime { get; protected set; } // Time that gold should begin to generate
        public float GameTime { get; private set; }
        public bool SpawnEnabled { get; set; }



        public Map(Game game, int id)
        {
            ExpToLevelUp = new List<int>();
            GameTime = 0;
            _nextSyncTime = 10 * 1000;
            _announcerEvents = new List<Announce>();
            _game = game;
            HasFirstBloodHappened = false;
            IsKillGoldRewardReductionActive = true;
            SpawnEnabled = _game.Config.MinionSpawnsEnabled;
            Id = id;
            var path = System.IO.Path.Combine(
                Program.ExecutingDirectory,
                "Content",
                "Data",
                _game.Config.ContentManager.GameModeName,
                "AIMesh",
                "Map" + Id,
                "AIPath.aimesh"
            );

            if (File.Exists(path))
            {
                AIMesh = new RAF.AIMesh(path);
            }
            else
            {
                _logger.LogCoreError("Failed to load Summoner's Rift data.");
                return;
            }
        }

        public float GetWidth()
        {
            return AIMesh.getWidth();
        }

        public float GetHeight()
        {
            return AIMesh.getHeight();
        }

        public Vector2 GetSize()
        {
            return new Vector2(GetWidth() / 2, GetHeight() / 2);
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

        public virtual void Init()
        {

        }

        public virtual void Update(float diff)
        {
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
        }

        public virtual float GetGoldPerSecond()
        {
            return 0;
        }
        public virtual Tuple<TeamId, Vector2> GetMinionSpawnPosition(MinionSpawnPosition spawnPosition)
        {
            return null;
        }

        public virtual void SetMinionStats(Minion m)
        {

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

        public virtual float[] GetEndGameCameraPosition(TeamId team)
        {
            return new float[] { 0, 0, 0 };
        }
    }
}
