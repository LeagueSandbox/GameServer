namespace GameServerCore.Domain.GameObjects
{
    public interface ITarget
    {
        float X { get; }
        float Y { get; }
        bool IsSimpleTarget { get; }
    }
}
