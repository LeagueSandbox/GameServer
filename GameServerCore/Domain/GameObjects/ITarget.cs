using System.Numerics;

namespace GameServerCore.Domain.GameObjects
{
    public interface ITarget
    {
        Vector2 Position { get; }
        bool IsSimpleTarget { get; }
    }
}
