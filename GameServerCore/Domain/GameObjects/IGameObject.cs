using System.Collections.Generic;
using System.Numerics;
using GameServerCore.Enums;

namespace GameServerCore.Domain.GameObjects
{
    public interface IGameObject : ITarget, IUpdate
    {
        uint NetId { get; }
        ITarget Target { get; }
        List<Vector2> Waypoints { get; }
        int CurWaypoint { get; }
        TeamId Team { get; }
        float CollisionRadius { get; }
        float VisionRadius { get; }

        float GetZ();
        float GetMoveSpeed();
        void SetPosition(float x, float y);
        void SetWaypoints(List<Vector2> newWaypoints);
        void SetTeam(TeamId team);
        void SetToRemove();
        void OnAdded();
        void OnRemoved();
        bool IsVisibleByTeam(TeamId team);
        void SetVisibleByTeam(TeamId team, bool visible);
        void OnCollision(IGameObject collider);
        bool IsCollidingWith(IGameObject o);
        bool IsToRemove();
        bool IsMovementUpdated();
        void ClearMovementUpdated();
    }
}
