using GameServerCore.Enums;

namespace GameServerCore.Domain.GameObjects
{
    public interface ILaneMinion : IMinion
    {
        /// <summary>
        /// Name of the Barracks that spawned this lane minion.
        /// </summary>
        string BarracksName { get; }
        MinionSpawnType MinionSpawnType { get; }

        void WalkToDestination();
    }
}
