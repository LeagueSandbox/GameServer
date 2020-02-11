using System.Collections.Generic;
using System.Numerics;
using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Content
{
    public interface INavigationGrid
    {
        Vector3 MinGridPosition { get; }
        Vector3 MaxGridPosition { get; }
        Vector3 TranslationMaxGridPosition { get; }
        float CellSize { get; }
        uint CellCountX { get; }
        uint CellCountY { get; }
        Vector2 SampledHeightsDistance { get; }
        float OffsetX { get; }
        float OffsetZ { get; }
        float MapWidth { get; }
        float MapHeight { get; }
        Vector2 MiddleOfMap { get; }

        float GetHeightAtLocation(Vector2 coords);
        float GetHeightAtLocation(float x, float y);
        bool IsWalkable(Vector2 coords);
        bool IsWalkable(float x, float y);
        List<Vector2> GetPath(Vector2 start, Vector2 end);
        bool IsSeeThrough(Vector2 coords);
        bool IsSeeThrough(float x, float y);
        bool IsAnythingBetween(IGameObject a, IGameObject b);
        Vector2 GetClosestTerrainExit(Vector2 location);
    }
}
