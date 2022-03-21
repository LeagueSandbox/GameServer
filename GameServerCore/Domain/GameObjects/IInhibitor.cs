using GameServerCore.Enums;

namespace GameServerCore.Domain.GameObjects
{
    public interface IInhibitor : IObjAnimatedBuilding
    {
        float RespawnTime { get; set; }
        bool RespawnAnimationAnnounced { get; set; }
        InhibitorState InhibitorState { get; }
        LaneID Lane { get; }
        void SetState(InhibitorState state);
        void NotifyState(IDeathData data = null);
    }
}
