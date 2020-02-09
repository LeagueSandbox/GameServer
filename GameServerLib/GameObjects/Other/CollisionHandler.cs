using System.Collections.Generic;
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

        public CollisionHandler(Game game, IMap map)
        {
            _game = game;
            //Pathfinder.setMap(map);
            // Initialise the pathfinder.
        }

        public void AddObject(IGameObject obj)
        {
            _objects.Add(obj);
        }

        public void RemoveObject(IGameObject obj)
        {
            _objects.Remove(obj);
        }

        public void Update()
        {
            foreach (var obj in _objects)
            {
                if (obj is IBaseTurret || obj is IInhibitor || obj is INexus)
                {
                    continue;
                }

                if (!_game.Map.NavGrid.IsWalkable(obj.X, obj.Y))
                {
                    obj.OnCollision(null);
                }

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
