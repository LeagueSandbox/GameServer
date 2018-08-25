namespace GameServerCore.Domain.GameObjects
{
    public interface ILevelProp : IGameObject
    {
        string Name { get; }
        string Model { get; }
        float Z { get; }
        float DirX { get; }
        float DirY { get; }
        float DirZ { get; }
        float Unk1 { get; }
        float Unk2 { get; }
        byte SkinId { get; }
    }
}
