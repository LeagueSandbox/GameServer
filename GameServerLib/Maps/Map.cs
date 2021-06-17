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
    /// <summary>
    /// Class responsible for all map related game settings such as collision handler, navigation grid, announcer events, and map properties.
    /// </summary>
    public class Map : IMap
    {
        // Crucial Vars
        protected Game _game;
        private readonly ILog _logger;

        /// <summary>
        /// Unique identifier for the Map (ex: 1 = Old SR, 11 = New SR)
        /// </summary>
        public int Id { get; private set; }
        /// <summary>
        /// Collision Handler to be instanced by the map. Used for collisions between GameObjects or GameObjects and terrain.
        /// </summary>
        public ICollisionHandler CollisionHandler { get; private set; }
        /// <summary>
        /// Navigation Grid to be instanced by the map. Used for terrain data.
        /// </summary>
        public INavigationGrid NavigationGrid { get; private set; }
        /// <summary>
        /// MapProperties specific to a Map Id. Contains information about passive gold gen, lane minion spawns, experience to level, etc.
        /// </summary>
        public IMapProperties MapProperties { get; private set; }
        /// <summary>
        /// List of events related to the announcer (ex: first blood)
        /// </summary>
        public List<IAnnounce> AnnouncerEvents { get; private set; }

        /// <summary>
        /// Instantiates map related game settings such as collision handler, navigation grid, announcer events, and map properties.
        /// </summary>
        /// <param name="game">Game instance.</param>
        public Map(Game game)
        {
            _game = game;
            _logger = LoggerProvider.GetLogger();

            Id = _game.Config.GameConfig.Map;

            try
            {
                NavigationGrid = _game.Config.ContentManager.GetNavigationGrid(Id);
            }
            catch (ContentNotFoundException exception)
            {
                _logger.Error(exception.Message);
                return;
            }

            AnnouncerEvents = new List<IAnnounce>();
            CollisionHandler = new CollisionHandler(this);
            MapProperties = GetMapProperties(Id);
        }

        /// <summary>
        /// Function called every tick of the game. Updates CollisionHandler, MapProperties, and executes AnnouncerEvents.
        /// </summary>
        /// <param name="diff">Number of milliseconds since this tick occurred.</param>
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

        /// <summary>
        /// Initializes MapProperties. Usually only occurs once before players are added to Game.
        /// </summary>
        public void Init()
        {
            MapProperties.Init();
        }

        public IMapProperties GetMapProperties(int mapId)
        {
            var dict = new Dictionary<int, Type>
            {
                // [0] = typeof(FlatTestMap),
                [1] = typeof(SummonersRift),
                // [2] = typeof(HarrowingRift),
                // [3] = typeof(ProvingGrounds),
                // [4] = typeof(TwistedTreeline),
                // [6] = typeof(WinterRift),
                // [8] = typeof(CrystalScar),
                // [10] = typeof(NewTwistedTreeline),
                // [11] = typeof(NewSummonersRift),
                //[12] = typeof(HowlingAbyss)
                // [14] = typeof(ButchersBridge)
            };

            if (!dict.ContainsKey(mapId))
            {
                return new SummonersRift(_game);
            }

            return (IMapProperties)Activator.CreateInstance(dict[mapId], _game);
        }
    }
}
