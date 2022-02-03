using GameServerCore.Domain.GameObjects;
using System.Activities.Presentation.View;

namespace GameServerCore
{
    /// <summary>
    /// Class which calls to collision based functions for GameObjects.
    /// </summary>
    public interface ICollisionHandler
    {
        QuadTree<IGameObject> QuadDynamic { get; }

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
        /// Function called every tick of the game by Map.cs.
        /// </summary>
        void Update();
    }
}