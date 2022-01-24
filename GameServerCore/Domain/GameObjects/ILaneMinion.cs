using GameServerCore.Enums;
using System.Collections.Generic;
using System.Numerics;

namespace GameServerCore.Domain.GameObjects
{
    public interface ILaneMinion : IMinion
    {
        /// <summary>
        /// Name of the Barracks that spawned this lane minion.
        /// </summary>
        string BarracksName { get; }
        MinionSpawnType MinionSpawnType { get; }
        List<Vector2> PathingWaypoints { get; }
    }
}