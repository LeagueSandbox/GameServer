using GameServerCore.Domain.GameObjects;
using System;
using System.Activities.Presentation.View;
using System.Collections.Generic;
using System.Numerics;

namespace GameServerCore.Handlers
{
    /// <summary>
    /// Class which calls to collision based functions for GameObjects.
    /// </summary>
    public interface ICollisionHandler
    {
        /// <summary>
        /// Adds the specified GameObject to the list of GameObjects to check for collisions. *NOTE*: Will fail to fully add the GameObject if it is out of the map's bounds.
        /// </summary>
        /// <param name="obj">GameObject to add.</param>
        void AddObject(IGameObject obj);
        /// <summary>
        /// GameObject to remove from the list of GameObjects to check for collisions.
        /// </summary>
        /// <param name="obj">GameObject to remove.</param>
        /// <returns>true if item is successfully removed; false otherwise.</returns>
        bool RemoveObject(IGameObject obj);
        /// <summary>
        /// Gets the nearest GameObjects to the given GameObject.
        /// </summary>
        /// <param name="obj">GameObject which will be the origin of the check.</param>
        /// <returns>List of GameObjects. Null if GameObject is not present in the QuadTree.</returns>
        List<IGameObject> GetNearestObjects(IGameObject obj);
        /// <summary>
        /// Gets the nearest GameObjects to the given temporary circle object.
        /// </summary>
        /// <param name="circle"></param>
        /// <returns>List of GameObjects.</returns>
        List<IGameObject> GetNearestObjects(Circle circle);
        /// <summary>
        /// Function called every tick of the game by Map.cs.
        /// </summary>
        void Update();
        /// <summary>
        /// Updates collision for the specified object.
        /// </summary>
        /// <param name="obj">GameObject to check if colliding with other objects.</param>
        void UpdateCollision(IGameObject obj);
    }
}