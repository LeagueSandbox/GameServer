using System.Numerics;

namespace GameServerCore.Domain.GameObjects
{
    public interface ILevelProp : IGameObject
    {
        string Name { get; }
        string Model { get; }
        float Height { get; }
        float Unk1 { get; }
        float Unk2 { get; }
        byte SkinId { get; }
    }
}
