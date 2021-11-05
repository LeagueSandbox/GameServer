using System.Numerics;

namespace GameServerCore.Domain.GameObjects
{
    public interface ILevelProp : IGameObject
    {
        byte NetNodeID { get; }
        int SkinID { get; }
        float Height { get; }
        Vector3 PositionOffset { get; }
        Vector3 Scale { get; }
        byte SkillLevel { get; }
        byte Rank { get; }
        byte Type { get; }
        string Name { get; }
        string Model { get; }
    }
}