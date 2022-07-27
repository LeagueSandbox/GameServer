using System;
using System.Collections.Generic;
using System.Numerics;
using GameServerCore;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;

namespace LeagueSandbox.GameServer.Handlers
{
    /// <summary>
    /// Class which calls path based functions for GameObjects.
    /// </summary>
    public class PathingHandler
    {
        private MapScriptHandler _map;
        private readonly List<AttackableUnit> _pathfinders = new List<AttackableUnit>();
        private float pathUpdateTimer;

        public PathingHandler(MapScriptHandler map)
        {
            _map = map;
        }

        /// <summary>
        /// Adds the specified GameObject to the list of GameObjects to check for pathfinding. *NOTE*: Will fail to fully add the GameObject if it is out of the map's bounds.
        /// </summary>
        /// <param name="obj">GameObject to add.</param>
        public void AddPathfinder(AttackableUnit obj)
        {
            _pathfinders.Add(obj);
        }

        /// <summary>
        /// GameObject to remove from the list of GameObjects to check for pathfinding.
        /// </summary>
        /// <param name="obj">GameObject to remove.</param>
        /// <returns>true if item is successfully removed; false otherwise.</returns>
        public bool RemovePathfinder(AttackableUnit obj)
        {
            return _pathfinders.Remove(obj);
        }

        /// <summary>
        /// Function called every tick of the game by Map.cs.
        /// </summary>
        public void Update(float diff)
        {
            // TODO: Verify if this is the proper time between path updates.
            if (pathUpdateTimer >= 3000.0f)
            {
                // we iterate over a copy of _pathfinders because the original gets modified
                var objectsCopy = new List<AttackableUnit>(_pathfinders);
                foreach (var obj in objectsCopy)
                {
                    UpdatePaths(obj);
                }

                pathUpdateTimer = 0;
            }

            pathUpdateTimer += diff;
        }

        /// <summary>
        /// Updates pathing for the specified object.
        /// </summary>
        /// <param name="obj">GameObject to check for incorrect paths.</param>
        public void UpdatePaths(AttackableUnit obj)
        {
            var path = obj.Waypoints;
            if (path.Count == 0)
            {
                return;
            }

            var lastWaypoint = path[path.Count - 1];
            if (obj.CurrentWaypoint.Equals(lastWaypoint) && lastWaypoint.Equals(obj.Position))
            {
                return;
            }

            var newPath = new List<Vector2>();
            newPath.Add(obj.Position);
            
            foreach (Vector2 waypoint in path)
            {
                if (IsWalkable(waypoint, obj.PathfindingRadius))
                {
                    newPath.Add(waypoint);
                }
                else
                {
                    break;
                }
            }

            obj.SetWaypoints(newPath);
        }

        /// <summary>
        /// Checks if the given position can be pathed on.
        /// </summary>
        public bool IsWalkable(Vector2 pos, float radius = 0, bool checkObjects = false)
        {
            bool walkable = true;
            
            if (!_map.NavigationGrid.IsWalkable(pos, radius))
            {
                walkable = false;
            }

            if (checkObjects && _map.CollisionHandler.GetNearestObjects(new System.Activities.Presentation.View.Circle(pos, radius)).Count > 0)
            {
                walkable = false;
            }

            return walkable;
        }

        /// <summary>
        /// Returns a path to the given target position from the given unit's position.
        /// </summary>
        public List<Vector2> GetPath(AttackableUnit obj, Vector2 target, bool usePathingRadius = true)
        {
            if (usePathingRadius)
            {
                return GetPath(obj.Position, target, obj.PathfindingRadius);
            }
            return GetPath(obj.Position, target, 0);
        }

        /// <summary>
        /// Returns a path to the given target position from the given start position.
        /// </summary>
        public List<Vector2> GetPath(Vector2 start, Vector2 target, float checkRadius = 0)
        {
            return _map.NavigationGrid.GetPath(start, target, checkRadius);
        }
    }
}
