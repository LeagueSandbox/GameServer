namespace GameServerCore.Domain.GameObjects
{
    public interface IMonster : IMinion
    {
        IMonsterCamp Camp { get; }
        string SpawnAnimation { get; }
        new int InitialLevel { get; set; }
    }
}
