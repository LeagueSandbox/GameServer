﻿using System.Collections.Generic;
using GameServerCore;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Maps;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.Buildings.AnimatedBuildings;
using LeagueSandbox.GameServer.Maps;
using UltimateQuadTree;

namespace LeagueSandbox.GameServer.GameObjects.Other
{
    public class CollisionHandler: ICollisionHandler
    {
        private Game _game; //TODO: get rid of this thing YAGNI!

        private readonly List<IGameObject> _objects = new List<IGameObject>();
        // This is the 'static map'
        private readonly QuadTree<IGameObject> _quadMap = new QuadTree<IGameObject>(20000,20000,new CollisonObject()); //TODO: initialize the proper values of width and height for every map

        public CollisionHandler(Game game, IMap map)
        {
            _game = game;
            //Pathfinder.setMap(map);
            // Initialise the pathfinder.
        }

        public void AddObject(IGameObject obj)
        {
            _objects.Add(obj);
            if (obj is IBaseTurret || obj is IInhibitor || obj is INexus)
            {
                _quadMap.Insert(obj);
            }
            
        }

        public void RemoveObject(IGameObject obj)
        {
            _objects.Remove(obj);
            if (obj is IBaseTurret || obj is IInhibitor || obj is INexus)
            {
                _quadMap.Remove(obj);
            }
        }

        public void Update()
        {
            QuadTree<IGameObject> _quadDynamic = new QuadTree<IGameObject>(20000, 20000, new CollisonObject());
            _quadDynamic.InsertRange(_objects);
            foreach (var obj in _objects)
            {
                // static objects, skip collison as they are static
                if (obj is IBaseTurret || obj is IInhibitor || obj is INexus)
                {
                    continue;
                }

                if (!_game.Map.NavGrid.IsWalkable(obj.X, obj.Y))
                {
                    obj.OnCollision(null); // TODO: Change this to something more readable like `bool terrain`
                }

                foreach (var obj2 in _quadDynamic.GetNearestObjects(obj))
                {
                    if (obj.IsCollidingWith(obj2))
                    {
                        obj.OnCollision(obj2);
                    }
                }
            }
        }
    }
}
