﻿using System;
using System.Collections.Generic;
using GameServerCore;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using GameServerCore.Maps;
using System.Activities.Presentation.View;

namespace LeagueSandbox.GameServer.GameObjects.Other
{
    /// <summary>
    /// Class which calls collision based functions for GameObjects.
    /// </summary>
    public class CollisionHandler : ICollisionHandler
    {
        private IMapScriptHandler _map;

        private readonly List<IGameObject> _objects = new List<IGameObject>();

        public QuadTree<IGameObject> QuadDynamic { get; private set; }

        public CollisionHandler(IMapScriptHandler map)
        {
            _map = map;

            // Initializes a dynamic map using NavigationGrid properties and a CollisionObject which takes into account an object's CollisionRadius (+1 for insurance).
            // It will contain all GameObjects that should be able to collide with eachother, refer to IsCollisionObject.
            QuadDynamic = new QuadTree<IGameObject>(
                _map.NavigationGrid.MinGridPosition.X, // MIN
                _map.NavigationGrid.MaxGridPosition.Z, // yep, MAX
                _map.NavigationGrid.MaxGridPosition.X -_map.NavigationGrid.MinGridPosition.X,
                _map.NavigationGrid.MaxGridPosition.Z - _map.NavigationGrid.MinGridPosition.Z
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
            // CollisionObjects can be any AI units, ObjBuildings, pure AttackableUnits, and pure GameObjects.
            // TODO: Implement static navgrid updates for turrets so we don't have to count them as collision objects.
            return !(obj.IsToRemove() || obj is ILevelProp || obj is IParticle || obj is ISpellMissile || obj is IRegion) && obj.CollisionRadius > 0;
        }

        /// <summary>
        /// Whether or not the specified GameObject is affected by collisions with other GameObjects.
        /// Used to determine if OnCollision functions should be called.
        /// </summary>
        /// <param name="obj">GameObject to check.</param>
        /// <returns>True/False.</returns>
        private bool IsCollisionAffected(IGameObject obj)
        {
            // Collision affected GameObjects are non-turret AI units, AttackableUnits, missiles, and pure GameObjects.
            return !(obj.IsToRemove() || obj is ILevelProp || obj is IParticle || obj is IObjBuilding || obj is IBaseTurret);
        }

        Circle GetBounds(IGameObject obj)
        {
            return new Circle(obj.Position, obj.CollisionRadius);
        }

        /// <summary>
        /// Adds the specified GameObject to the list of GameObjects to check for collisions. *NOTE*: Will fail to fully add the GameObject if it is out of the map's bounds.
        /// </summary>
        /// <param name="obj">GameObject to add.</param>
        public void AddObject(IGameObject obj)
        {
            bool collides = IsCollisionObject(obj);
            bool detects = IsCollisionAffected(obj);
            if(collides || detects)
            {
                _objects.Add(obj);
            }
            if(collides)
            {
                QuadDynamic.Insert(obj, GetBounds(obj));
            }
        }

        /// <summary>
        /// GameObject to remove from the list of GameObjects to check for collisions.
        /// </summary>
        /// <param name="obj">GameObject to remove.</param>
        /// <returns>true if item is successfully removed; false otherwise.</returns>
        public bool RemoveObject(IGameObject obj)
        {
            return _objects.Remove(obj);
            //&& QuadDynamic.Remove(obj); - QuadTree is rebuilt every frame, no need to remove
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
                if (IsCollisionAffected(obj))
                {
                    if (!_map.NavigationGrid.IsWalkable(obj.Position.X, obj.Position.Y))
                    {
                        obj.OnCollision(null, true);
                    }

                    var nearest = QuadDynamic.GetNodesInside(GetBounds(obj));
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

            UpdateQuadTree();
        }

        /// <summary>
        /// Used to reinitialize a QuadTree's sectors when objects may have moved out of sectors, which makes them unable to be removed.
        /// </summary>
        private void UpdateQuadTree()
        {
            QuadDynamic.Clear();
            foreach(var obj in _objects)
            {
                if(IsCollisionObject(obj))
                {
                    QuadDynamic.Insert(obj, GetBounds(obj));
                }
            }
        }
    }
}
