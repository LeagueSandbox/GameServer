namespace GameServerCore.Domain.GameObjects
{
    public interface IMonster : IMinion
    {
        IMonsterCamp Camp { get; }
        string SpawnAnimation { get; }
        void UpdateInitialLevel(int level);
    }
}
