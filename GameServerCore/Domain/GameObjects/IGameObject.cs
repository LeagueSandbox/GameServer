using System.Collections.Generic;
using System.Numerics;
using GameServerCore.Enums;

namespace GameServerCore.Domain.GameObjects
{
    public interface IGameObject : ITarget
    {
        uint NetId { get; }
        ITarget Target { get; }
        List<Vector2> Waypoints { get; }
        int CurWaypoint { get; }
        TeamId Team { get; }
        int AttackerCount { get; }
        float CollisionRadius { get; }
        float VisionRadius { get; }
        bool IsDashing { get; }

        float GetZ();
        float GetMoveSpeed();
        void SetPosition(float x, float y);
        void SetWaypoints(List<Vector2> newWaypoints);
        void SetTeam(TeamId team);
        void OnAdded();
        void OnRemoved();
    }
}
