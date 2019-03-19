using System;
using System.Collections.Generic;
using System.IO;
using GameServerCore;
using GameServerCore.Content;
using GameServerCore.Domain;
using GameServerCore.Maps;
using LeagueSandbox.GameServer.Content;
using LeagueSandbox.GameServer.GameObjects.Other;
using LeagueSandbox.GameServer.Logging;
using LeagueSandbox.GameServer.Scripting.CSharp;
using log4net;

namespace LeagueSandbox.GameServer.Maps
{
    public class Map: IMap
    {
        protected Game _game;
        private readonly ILog _logger;

        public List<IAnnounce> AnnouncerEvents { get; private set; }
        public INavGrid NavGrid { get; private set; }
        public ICollisionHandler CollisionHandler { get; private set; }
        public int Id { get; private set; }
        public IMapProperties MapProperties { get; private set; }

        public Map(Game game)
        {
            _game = game;
            _logger = LoggerProvider.GetLogger();
            Id = _game.Config.GameConfig.Map;
            var path = Path.Combine(
                game.Config.ContentPath,
                _game.Config.ContentManager.GameModeName,
                "AIMesh",
                "Map" + Id,
                "AIPath.aimesh_ngrid"
            );

            if (File.Exists(path))
            {
                NavGrid = NavGridReader.ReadBinary(path);
            }
            else
            {
                _logger.Error("Failed to load navigation graph. Aborting map load.");
                return;
            }

            AnnouncerEvents = new List<IAnnounce>();
            CollisionHandler = new CollisionHandler(_game, this);
            MapProperties = GetMapProperties(Id);
        }

        public IMapProperties GetMapProperties(int mapId)
        {
            var dict = new Dictionary<int, Type>
            {
                // [0] = typeof(FlatTestMap),
                [1] = typeof(SummonersRift)
                // [2] = typeof(HarrowingRift),
                // [3] = typeof(ProvingGrounds),
                // [4] = typeof(TwistedTreeline),
                // [6] = typeof(WinterRift),
                // [8] = typeof(CrystalScar),
                // [10] = typeof(NewTwistedTreeline),
                // [11] = typeof(NewSummonersRift),
                // [12] = typeof(HowlingAbyss),
                // [14] = typeof(ButchersBridge)
            };

            if (!dict.ContainsKey(mapId))
            {
                return new SummonersRift(_game);
            }

            return (IMapProperties)Activator.CreateInstance(dict[mapId], _game);
        }

        public void Init()
        {
            MapProperties.Init();
        }

        public void Update(float diff)
        {
            CollisionHandler.Update();
            foreach (var announce in AnnouncerEvents)
            {
                if (!announce.IsAnnounced && _game.GameTime >= announce.EventTime)
                {
                    announce.Execute();
                }
            }

            MapProperties.Update(diff);
        }
    }
}
