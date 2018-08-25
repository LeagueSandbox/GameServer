namespace GameServerCore.Domain.GameObjects
{
    public interface IBaseTurret : IObjAiBase
    {
        string Name { get; }
        uint ParentNetId { get; }
    }
}
