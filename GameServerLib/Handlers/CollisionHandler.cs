using System;
using System.Collections.Generic;
using System.Activities.Presentation.View;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Missile;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.Buildings;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;

namespace LeagueSandbox.GameServer.Handlers
{
    /// <summary>
    /// Class which calls collision based functions for GameObjects.
    /// </summary>
    public class CollisionHandler
    {
        private MapScriptHandler _map;
        private readonly List<GameObject> _objects = new List<GameObject>();
        private QuadTree<GameObject> _quadDynamic;

        public CollisionHandler(MapScriptHandler map)
        {
            _map = map;

            // Initializes a dynamic map using NavigationGrid properties and a CollisionObject which takes into account an object's CollisionRadius (+1 for insurance).
            // It will contain all GameObjects that should be able to collide with eachother, refer to IsCollisionObject.
            _quadDynamic = new QuadTree<GameObject>(
                _map.NavigationGrid.MinGridPosition.X, // MIN
                _map.NavigationGrid.MaxGridPosition.Z, // yep, MAX
                _map.NavigationGrid.MaxGridPosition.X -_map.NavigationGrid.MinGridPosition.X,
                _map.NavigationGrid.MaxGridPosition.Z - _map.NavigationGrid.MinGridPosition.Z
            );
        }

        /// <summary>
        /// Whether or not the specified GameObject is able to collide with other GameObjects.
        /// </summary>
        /// <param name="obj">GameObject to check.</param>
        /// <returns>True/False.</returns>
        private bool IsCollisionObject(GameObject obj)
        {
            // CollisionObjects can be any AI units, ObjBuildings, pure AttackableUnits, and pure GameObjects.
            // TODO: Implement static navgrid updates for turrets so we don't have to count them as collision objects.
            return !(obj.IsToRemove() || obj is LevelProp || obj is Particle || obj is SpellMissile || obj is Region) && obj.CollisionRadius >= 0;
        }

        /// <summary>
        /// Whether or not the specified GameObject is affected by collisions with other GameObjects.
        /// Used to determine if OnCollision functions should be called.
        /// </summary>
        /// <param name="obj">GameObject to check.</param>
        /// <returns>True/False.</returns>
        private bool IsCollisionAffected(GameObject obj)
        {
            // Collision affected GameObjects are non-turret AI units, AttackableUnits, missiles, and pure GameObjects.
            return !(obj.IsToRemove() || obj is LevelProp || obj is Particle || obj is ObjBuilding || obj is BaseTurret);
        }

        Circle GetBounds(GameObject obj)
        {
            return new Circle(obj.Position, Math.Max(0.5f, obj.CollisionRadius));
        }

        /// <summary>
        /// Adds the specified GameObject to the list of GameObjects to check for collisions. *NOTE*: Will fail to fully add the GameObject if it is out of the map's bounds.
        /// </summary>
        /// <param name="obj">GameObject to add.</param>
        public void AddObject(GameObject obj)
        {
            bool collides = IsCollisionObject(obj);
            bool detects = IsCollisionAffected(obj);
            if (collides || detects)
            {
                _objects.Add(obj);
            }
            if (collides)
            {
                _quadDynamic.Insert(obj, GetBounds(obj));
            }
        }

        /// <summary>
        /// GameObject to remove from the list of GameObjects to check for collisions.
        /// </summary>
        /// <param name="obj">GameObject to remove.</param>
        /// <returns>true if item is successfully removed; false otherwise.</returns>
        public bool RemoveObject(GameObject obj)
        {
            return _objects.Remove(obj);
        }

        /// <summary>
        /// Gets the nearest GameObjects to the given GameObject.
        /// </summary>
        /// <param name="obj">GameObject which will be the origin of the check.</param>
        /// <returns>List of GameObjects. Null if GameObject is not present in the QuadTree.</returns>
        public List<GameObject> GetNearestObjects(GameObject obj)
        {
            return GetNearestObjects(GetBounds(obj));
        }

        /// <summary>
        /// Gets the nearest GameObjects to the given temporary circle object.
        /// </summary>
        /// <param name="circle"></param>
        /// <returns>List of GameObjects.</returns>
        public List<GameObject> GetNearestObjects(Circle circle)
        {
            var nearest = new List<GameObject>();

            foreach (var obj in _quadDynamic.GetNodesInside(circle))
            {
                nearest.Add(obj);
            }

            return nearest;
        }

        /// <summary>
        /// Function called every tick of the game by Map.cs.
        /// </summary>
        public void Update()
        {
            // we iterate over a copy of _objects because the original gets modified
            var objectsCopy = new List<GameObject>(_objects);
            foreach (var obj in objectsCopy)
            {
                UpdateCollision(obj);
            }

            UpdateQuadTree();
        }

        /// <summary>
        /// Updates collision for the specified object.
        /// </summary>
        /// <param name="obj">GameObject to check if colliding with other objects.</param>
        public void UpdateCollision(GameObject obj)
        {
            if (IsCollisionAffected(obj))
            {
                if (!_map.PathingHandler.IsWalkable(obj.Position))
                {
                    obj.OnCollision(null, true);
                }

                var nearest = GetNearestObjects(obj);
                foreach (var obj2 in nearest)
                {
                    // TODO: Implement interpolation (or hull tracing) to account for fast moving gameobjects that may go past other gameobjects within one tick, which bypasses collision.
                    if (obj != obj2 && !obj2.IsToRemove() && obj.IsCollidingWith(obj2))
                    {
                        obj.OnCollision(obj2);
                    }
                }
            }
        }

        /// <summary>
        /// Used to reinitialize a QuadTree's sectors when objects may have moved out of sectors, which makes them unable to be removed.
        /// </summary>
        private void UpdateQuadTree()
        {
            _quadDynamic.Clear();
            foreach (var obj in _objects)
            {
                if (IsCollisionObject(obj))
                {
                    _quadDynamic.Insert(obj, GetBounds(obj));
                }
            }
        }
    }
}
