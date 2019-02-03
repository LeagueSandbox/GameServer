using System.Collections.Generic;
using System.Numerics;
using GameServerCore.Enums;

namespace GameServerCore.Domain.GameObjects
{
    public interface IGameObject : ITarget, IUpdate
    {
        /// <summary>
        ///  Identifier unique to this game object.
        /// </summary>
        uint NetId { get; }
        /// <summary>
        /// Waypoints that the game object walking through.
        /// </summary>
        List<Vector2> Waypoints { get; }
        ITarget Target { get; }
        /// <summary>
        /// Team identifier
        /// </summary>
        TeamId Team { get; }
        float CollisionRadius { get; }
        float VisionRadius { get; }
        /// <summary>
        /// Used to synchronize movement
        /// </summary>
        uint SyncId { get; }
        // TODO: Change this to property
        float GetZ();
        float GetMoveSpeed();
        
        void ClearMovementUpdated();
        
        void OnAdded();
        void OnCollision(IGameObject collider);
        void OnRemoved();
        
        void SetPosition(float x, float y);
        void SetPosition(Vector2 vec);
        void SetTeam(TeamId team);
        void SetToRemove();
        void SetVisibleByTeam(TeamId team, bool visible);
        void SetWaypoints(List<Vector2> newWaypoints);
        
        bool IsCollidingWith(IGameObject o);
        bool IsMovementUpdated();
        bool IsToRemove();
        bool IsVisibleByTeam(TeamId team);
    }
}
