namespace GameServerCore.Domain.GameObjects
{
    public interface IParticle : IGameObject
    {
        IChampion Owner { get; }
        string Name { get; }
        string BoneName { get; }
        float Size { get; }
    }
}
