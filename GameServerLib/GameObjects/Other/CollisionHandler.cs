﻿using System.Collections.Generic;
using GameServerCore;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Maps;
using UltimateQuadTree;

namespace LeagueSandbox.GameServer.GameObjects.Other
{
    /// <summary>
    /// Class which calls to collision based functions for GameObjects.
    /// </summary>
    public class CollisionHandler : ICollisionHandler
    {
        private IMap _map;

        private readonly List<IGameObject> _objects = new List<IGameObject>();
        // This is the 'dynamic map', updated every update of the game.
        private readonly QuadTree<IGameObject> _quadDynamic;

        public CollisionHandler(IMap map)
        {
            _map = map;

            // Initializes a dynamic map using NavigationGrid properties and a CollisionObject which takes into account an object's CollisionRadius (+1 for insurance).
            // It will contain all GameObjects that should be able to collide with eachother, refer to IsCollisionObject.
            _quadDynamic = new QuadTree<IGameObject>(
                _map.NavigationGrid.MinGridPosition.X,
                _map.NavigationGrid.MinGridPosition.Z,
                // Subtract one cell's size from the max so we never reach the CellCountX/Y (since Cells is an array).
                _map.NavigationGrid.MaxGridPosition.X + System.MathF.Abs(_map.NavigationGrid.MinGridPosition.X),
                _map.NavigationGrid.MaxGridPosition.Z + System.MathF.Abs(_map.NavigationGrid.MinGridPosition.Z),
                new CollisionObject()
            );

            //Pathfinder.setMap(map);
            // Initialise the pathfinder.
        }

        /// <summary>
        /// Whether or not the specified GameObject is able to collide with other GameObjects.
        /// </summary>
        /// <param name="obj">GameObject to check.</param>
        /// <returns>True/False.</returns>
        private bool IsCollisionObject(IGameObject obj)
        {
            // CollisionObjects can be any AI units, pure AttackableUnits, missiles, and pure GameObjects.
            // TODO: Implement static navgrid updates for turrets so we don't have to count them as collision objects.
            return !(obj is ILevelProp || obj is IParticle || obj is IObjBuilding);
        }

        /// <summary>
        /// Whether or not the specified GameObject is affected by collisions with other GameObjects.
        /// Used to determine if OnCollision functions should be called.
        /// </summary>
        /// <param name="obj">GameObject to check.</param>
        /// <returns>True/False.</returns>
        private bool IsCollisionAffected(IGameObject obj)
        {
            // Collision affected GameObjects are non-turret AI units, pure AttackableUnits, missiles, and pure GameObjects.
            return !(obj is ILevelProp || obj is IParticle || obj is IObjBuilding || obj is IBaseTurret);
        }

        /// <summary>
        /// Adds the specified GameObject to the list of GameObjects to check for collisions.
        /// </summary>
        /// <param name="obj">GameObject to add.</param>
        public void AddObject(IGameObject obj)
        {
            _objects.Add(obj);

            // Add dynamic objects
            if (IsCollisionAffected(obj))
            {
                _quadDynamic.Insert(obj);
            }
        }

        /// <summary>
        /// GameObject to remove from the list of GameObjects to check for collisions.
        /// </summary>
        /// <param name="obj">GameObject to remove.</param>
        public void RemoveObject(IGameObject obj)
        {
            _objects.Remove(obj);

            // Remove dynamic objects
            if (IsCollisionAffected(obj))
            {
                _quadDynamic.Remove(obj);
            }
        }

        /// <summary>
        /// Function called every tick of the game by Map.cs.
        /// </summary>
        public void Update()
        {
            // we iterate over a copy of _objects because the original gets modified
            var objectsCopy = new List<IGameObject>(_objects);
            foreach (var obj in objectsCopy)
            {
                if (!IsCollisionAffected(obj))
                {
                    continue;
                }

                if (!_map.NavigationGrid.IsWalkable(obj.X, obj.Y))
                {
                    obj.OnCollision(null, true);
                }

                foreach (var obj2 in _quadDynamic.GetNearestObjects(obj))
                {
                    if (obj == obj2)
                    {
                        continue;
                    }

                    // TODO: Implement interpolation (or hull tracing) to account for fast moving gameobjects that may go past other gameobjects within one tick, which bypasses collision.
                    if (obj.IsCollidingWith(obj2))
                    {
                        obj.OnCollision(obj2);
                    }

                    if (!IsCollisionObject(obj2))
                    {
                        continue;
                    }

                    // TODO: Implement repathing if our position within the next few ticks intersects with another GameObject (assuming we are moving; !IsPathEnded).
                }
            }
        }
    }
}
