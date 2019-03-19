using GameServerCore.Enums;

namespace GameServerCore.Domain
{
    public interface ICrowdControl: IUpdate
    {
        CrowdControlType Type { get; }
        float Duration { get; }
        float CurrentTime { get; }
        bool IsRemoved { get; }
        bool IsTypeOf(CrowdControlType type);
    }
}