using GameServerCore.Enums;

namespace GameServerCore.Domain.GameObjects
{
    public interface ILaneMinion : IMinion
    {
        string BarracksName { get; }
        MinionSpawnType MinionSpawnType { get; }

        void WalkToDestination();
    }
}
