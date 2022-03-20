using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using System.Collections.Generic;
using System.Numerics;

namespace GameServerCore.Handlers
{
    /// <summary>
    /// Class which calls to collision based functions for GameObjects.
    /// </summary>
    public interface IPathingHandler : IUpdate
    {
        /// <summary>
        /// Adds the specified GameObject to the list of GameObjects to check for collisions. *NOTE*: Will fail to fully add the GameObject if it is out of the map's bounds.
        /// </summary>
        /// <param name="obj">GameObject to add.</param>
        void AddPathfinder(IAttackableUnit obj);
        /// <summary>
        /// GameObject to remove from the list of GameObjects to check for collisions.
        /// </summary>
        /// <param name="obj">GameObject to remove.</param>
        /// <returns>true if item is successfully removed; false otherwise.</returns>
        bool RemovePathfinder(IAttackableUnit obj);
        /// <summary>
        /// Updates pathing for the specified object.
        /// </summary>
        /// <param name="obj">GameObject to check for incorrect paths.</param>
        void UpdatePaths(IAttackableUnit obj);
        /// <summary>
        /// Checks if the given position can be pathed on.
        /// </summary>
        bool IsWalkable(Vector2 pos, float radius = 0, bool checkObjects = false);
        /// <summary>
        /// Returns a path to the given target position from the given unit's position.
        /// </summary>
        List<Vector2> GetPath(IAttackableUnit obj, Vector2 target, bool usePathingRadius = false);
        /// <summary>
        /// Returns a path to the given target position from the given start position.
        /// </summary>
        List<Vector2> GetPath(Vector2 start, Vector2 target, float checkRadius = 0);
    }
}