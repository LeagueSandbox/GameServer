﻿using System;
using System.Collections.Generic;
using System.IO;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.GameObjects.Other;
using LeagueSandbox.GameServer.Logic.Scripting.CSharp;

namespace LeagueSandbox.GameServer.Logic.Maps
{
    public class Map
    {
        protected Game Game;

        public List<Announce> AnnouncerEvents { get; private set; }
        public NavGrid NavGrid { get; private set; }
        public CollisionHandler CollisionHandler { get; private set; }
        public int Id { get; private set; }
        public IMapGameScript MapGameScript { get; private set; }

        public Map(Game game)
        {
            Game = game;
            Id = Game.Config.GameConfig.Map;
            var path = Path.Combine(
                Program.ExecutingDirectory,
                "Content",
                "Data",
                Game.Config.ContentManager.GameModeName,
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
                Logger.LogCoreError("Failed to load navigation graph. Aborting map load.");
                return;
            }

            AnnouncerEvents = new List<Announce>();
            CollisionHandler = new CollisionHandler(this);
            MapGameScript = GetMapScript(Id);
        }

        public IMapGameScript GetMapScript(int mapId)
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
                return new SummonersRift();
            }

            return (IMapGameScript)Activator.CreateInstance(dict[mapId]);
        }

        public void Init()
        {
            MapGameScript.Init();
        }

        public void Update(float diff)
        {
            CollisionHandler.Update();
            foreach (var announce in AnnouncerEvents)
            {
                if (!announce.IsAnnounced && Game.GameTime >= announce.EventTime)
                {
                    announce.Execute();
                }
            }

            MapGameScript.Update(diff);
        }
    }
}
