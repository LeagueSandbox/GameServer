using GameServerCore.Enums;

namespace GameServerCore.Domain.GameObjects
{
    public interface IInhibitor : IObjAnimatedBuilding
    {
        bool RespawnAnnounced { get; }
        InhibitorState InhibitorState { get; }
        LaneID Lane { get; }
        void SetState(InhibitorState state, IDeathData data = null);
        double GetRespawnTimer();
    }
}
