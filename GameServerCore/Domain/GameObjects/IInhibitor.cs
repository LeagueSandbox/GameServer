using GameServerCore.Enums;

namespace GameServerCore.Domain.GameObjects
{
    public interface IInhibitor : IObjAnimatedBuilding
    {
        bool RespawnAnnounced { get; }
        InhibitorState InhibitorState { get; }
        void SetState(InhibitorState state, IGameObject killer);
        double GetRespawnTimer();
    }
}
