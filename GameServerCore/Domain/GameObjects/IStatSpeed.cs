namespace GameServerCore.Domain.GameObjects
{
    public interface IStatSpeed : IStat
    {
        float TotalRaw { get; }
        float SlowResist { get; }
    }
}
