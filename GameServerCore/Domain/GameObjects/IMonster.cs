namespace GameServerCore.Domain.GameObjects
{
    public interface IMonster : IMinion
    {
        IMonsterCamp Camp { get; }
        string SpawnAnimation { get; }
        int InitialLevel { get; set; }
    }
}
