using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameServerCore.Logic.Content
{
    public interface INavGrid
    {
        byte MajorVersion { get; }
        short MinorVersion { get; }
        Vector<float> MinGridPos { get; }
        Vector<float> MaxGridPos { get; }
        Vector<float> TranslationMaxGridPos { get; }
        float CellSize { get; }
        uint XCellCount { get; }
        uint YCellCount { get; }
        ushort[] CellFlags { get; } // Version 7 change
        int XSampledHeightCount { get; }
        int YSampledHeightCount { get; }
        float DirectionX { get; }
        float DirectionY { get; }
        float OffsetX { get; }
        float OffsetZ { get; }
        float MapWidth { get; }
        float MapHeight { get; }
        Vector2 MiddleOfMap { get; }

        float GetHeightAtLocation(Vector2 coords);
        float GetHeightAtLocation(float x, float y);
        bool IsWalkable(Vector2 coords);
        bool IsWalkable(float x, float y);
        Vector2 GetSize();
    }
}
