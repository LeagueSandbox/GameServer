namespace GameServerCore.Domain.GameObjects
{
    public interface IParticle : IGameObject
    {
        IGameObject Owner { get; }
        string Name { get; }
        string BoneName { get; }
        float Size { get; }
    }
}
