using System.Numerics;

namespace GameServerCore.Domain.GameObjects
{
    public interface ITarget
    {
        float X { get; }
        float Y { get; }
        bool IsSimpleTarget { get; }

        float GetDistanceTo(ITarget target);
        Vector2 GetPosition();
        bool WithinRange(Vector2 from, Vector2 to, float range);
    }
}
