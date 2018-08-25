namespace GameServerCore.Domain
{
    public interface IReplicate
    {
        uint Value { get; }
        bool IsFloat { get; }
        bool Changed { get; }
    }
}
