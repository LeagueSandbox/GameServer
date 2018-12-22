namespace GameServerCore.Domain.GameObjects
{
    public interface IMinion : IObjAiBase
    {
        string Name { get; }
        IObjAiBase Owner { get; }
        bool IsWard { get; }
        bool IsPet { get; }
        bool IsBot { get; }
        bool IsLaneMinion { get; }
        bool IsClone { get; }
    }
}
