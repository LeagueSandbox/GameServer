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
        protected Game _game;
        protected static Logger _logger = Program.ResolveDependency<Logger>();

        public List<Announce> AnnouncerEvents { get; private set; }
        public RAF.AIMesh AIMesh { get; private set; }
        public CollisionHandler CollisionHandler { get; private set; }
        public int Id { get; private set; } = 0;

        public List<int> ExpToLevelUp { get; set; }
        public float GoldPerSecond { get; set; }
        public bool HasFirstBloodHappened { get; set; }
        public bool IsKillGoldRewardReductionActive { get; set; }
        public int BluePillId { get; set; } = 0;
        public long FirstGoldTime { get;  set; } 
        public bool SpawnEnabled { get; set; }

        public Map(Game game)
        {
            _game = game;
            Id = _game.Config.GameConfig.Map;
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

            AnnouncerEvents = new List<Announce>();
            CollisionHandler = new CollisionHandler(this);
        }
        public virtual void Init()
        {

        }

        public virtual void Update(float diff)
        {
            CollisionHandler.Update();
            foreach (var announce in AnnouncerEvents)
            {
                if (!announce.IsAnnounced)
                {
                    if (_game.GameTime >= announce.EventTime)
                    {
                        announce.Execute();
                    }
                }
            }
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
