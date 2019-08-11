using System.Collections.Generic;
using System.Numerics;
using GameMaths.Geometry.Polygons;
using GameServerCore;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Maps;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.Buildings.AnimatedBuildings;
using LeagueSandbox.GameServer.Maps;

namespace LeagueSandbox.GameServer.GameObjects.Other
{
    public class CollisionHandler: ICollisionHandler
    {
        private Game _game;

        private readonly List<IGameObject> _objects = new List<IGameObject>();
        private readonly List<IGameObject> _staticObjects = new List<IGameObject>();

        public CollisionHandler(Game game, IMap map)
        {
            _game = game;
            //Pathfinder.setMap(map);
            // Initialise the pathfinder.
        }

        public bool IsOcuupiedByStaticObject(Vector2 pos, float radius)
        {
            var collide = new CirclePoly(pos, radius, 60);
            foreach (var o in _staticObjects)
            {
                var collider = new CirclePoly(o.GetPosition(), o.CollisionRadius);
                if (collider.CheckForOverLaps(collide))
                {
                    return true;
                }
            }
            return false;
        }
        public bool IsOcuupiedByStaticObject(Vector2 pos)
        {
            foreach (var o in _staticObjects)
            {
                var collider = new CirclePoly(o.GetPosition(), o.CollisionRadius);
                if (collider.IsInside(pos))
                {
                    return true;
                }
            }
            return false;
        }

        public void AddObject(IGameObject obj)
        {
            _objects.Add(obj);
            if(obj.IsStatic)
            {
                _staticObjects.Add(obj);
            }
        }

        public void RemoveObject(IGameObject obj)
        {
            _objects.Remove(obj);
            if (obj.IsStatic)
            {
                _staticObjects.Remove(obj);
            }
        }

        public void Update()
        {
            foreach (var obj in _objects)
            {
                if (obj.IsStatic)
                {
                    continue;
                }

                if (!_game.Map.NavGrid.IsWalkable(obj.X, obj.Y))
                {
                    obj.OnCollision(null);
                }
                // TODO: better algos
                foreach (var obj2 in _objects)
                {
                    if (obj == obj2)
                    {
                        continue;
                    }

                    if (obj.IsCollidingWith(obj2))
                    {
                        obj.OnCollision(obj2);
                    }
                }
            }
        }
    }
}
