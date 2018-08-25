namespace GameServerCore.Domain.GameObjects
{
    public interface IPlaceable : IObjAiBase
    {
        string Name { get; }
        IObjAiBase Owner { get; }
    }
}
