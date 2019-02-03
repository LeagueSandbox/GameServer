namespace GameServerCore.Domain.GameObjects
{
    public interface IBaseTurret : IObjAiBase
    {
        uint ParentNetId { get; }
        string Name { get; }
        
        void CheckForTargets();
    }
}
