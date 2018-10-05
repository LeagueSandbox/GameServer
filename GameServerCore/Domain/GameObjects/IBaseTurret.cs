namespace GameServerCore.Domain.GameObjects
{
    public interface IBaseTurret : IObjAiBase
    {
        string Name { get; }
        void CheckForTargets();
        uint ParentNetId { get; }
    }
}
