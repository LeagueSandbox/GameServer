using GameServerCore.Enums;

namespace GameServerCore.Domain
{
    public interface ICrowdControl
    {
        CrowdControlType Type { get; }
        float Duration { get; }
        float CurrentTime { get; }
        bool IsRemoved { get; }
        void Update(float diff);
        bool IsTypeOf(CrowdControlType type);
    }
}