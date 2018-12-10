namespace GameServerCore.Domain.GameObjects
{
    public interface IMinion : IObjAiBase
    {
        string Name { get; }
        IObjAiBase Owner { get; }
    }
}
