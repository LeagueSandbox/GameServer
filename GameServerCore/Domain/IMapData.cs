using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using System.Collections.Generic;
using System.Numerics;

namespace GameServerCore.Domain
{
    public interface IMapData
    {
        int Id { get; }
        /// <summary>
        /// Collection of MapObjects present within a map's room file, with the key being the name present in the room file. Refer to <see cref="MapObject"/>.
        /// </summary>
        Dictionary<string, MapObject> MapObjects { get; }
        /// <summary>
        /// Collection of MapObjects which represent lane minion spawn positions.
        /// Not present within the room file, therefor it is split into its own collection.
        /// </summary>
        Dictionary<string, MapObject> SpawnBarracks { get; }
        /// <summary>
        /// Experience required to level, ordered from 2 and up.
        /// </summary>
        List<float> ExpCurve { get; }
        float BaseExpMultiple { get; set; }
        float LevelDifferenceExpMultiple { get; set; }
        float MinimumExpMultiple { get; set; }
        /// <summary>
        /// Amount of time death should last depending on level.
        /// </summary>
        List<float> DeathTimes { get; }
        /// <summary>
        /// Potential progression of stats per-level of jungle monsters.
        /// </summary>
        /// TODO: Figure out what this is and how to implement it.
        List<float> StatsProgression { get; }
    }
}