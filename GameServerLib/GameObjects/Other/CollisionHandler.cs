using System.Collections.Generic;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.Buildings.AnimatedBuildings;
using LeagueSandbox.GameServer.Maps;

namespace LeagueSandbox.GameServer.GameObjects.Other
{
    public class CollisionHandler
    {
        private Game _game;

        private List<GameObject> _objects = new List<GameObject>();

        public CollisionHandler(Game game, Map map)
        {
            _game = game;
            //Pathfinder.setMap(map);
            // Initialise the pathfinder.
        }

        public void AddObject(GameObject obj)
        {
            _objects.Add(obj);
        }

        public void RemoveObject(GameObject obj)
        {
            _objects.Remove(obj);
        }

        public void Update()
        {
            foreach (var obj in _objects)
            {
                if (obj is BaseTurret || obj is Inhibitor || obj is Nexus)
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
