using System.Collections.Generic;
using System.Numerics;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;

namespace GameServerCore.Content
{
    /// <summary>
    /// Class housing all functions and variables related to the static grid of the current map.
    /// Used for pathing, vision, and collision.
    /// </summary>
    public interface INavigationGrid
    {
        /// <summary>
        /// The minimum position on the NavigationGrid in normal coordinate space (bottom left in 2D).
        /// NavigationGridCells are undefined below these minimums.
        /// </summary>
        Vector3 MinGridPosition { get; }
        /// <summary>
        /// The maximum position on the NavigationGrid in normal coordinate space (top right in 2D).
        /// NavigationGridCells are undefined beyond these maximums.
        /// </summary>
        Vector3 MaxGridPosition { get; }
        /// <summary>
        /// Calculated resolution of the Navigation Grid (percentage of a cell 1 normal unit takes up, not to be confused with 1/CellSize).
        /// Multiple used to convert cell-based coordinates back into normal coordinates (CellCountX/Z / TranslationMaxGridPosition).
        /// </summary>
        Vector3 TranslationMaxGridPosition { get; }
        /// <summary>
        /// Ideal number of normal units a cell takes up (not fully accurate, but mostly, refer to TranslationMaxGridPosition for true size).
        /// </summary>
        float CellSize { get; }
        /// <summary>
        /// Width of the Navigation Grid in cells.
        /// </summary>
        uint CellCountX { get; }
        /// <summary>
        /// Height of the Navigation Grid in cells.
        /// </summary>
        uint CellCountY { get; }
        /// <summary>
        /// Array of region tags where each index represents a cell's index.
        /// </summary>
        public uint[] RegionTags { get; }
        /// <summary>
        /// Number of sampled heights in the X coordinate plane.
        /// </summary>
        uint SampledHeightsCountX { get; }
        /// <summary>
        /// Number of sampled heights in the Y coordinate plane (Z coordinate in 3D space).
        /// </summary>
        uint SampledHeightsCountY { get; }
        /// <summary>
        /// Multiple used to convert from normal coordinates to an index format used to get sampled heights from the Navigation Grid.
        /// </summary>
        /// TODO: Seems to be volatile. If there ever comes a time when Navigation Grid editing becomes easy, that'd be the perfect time to rework the methods for getting sampled heights.
        Vector2 SampledHeightsDistance { get; }
        /// <summary>
        /// Array of sampled heights where each index represents a cell's index (depends on SampledHeightsCountX/Y).
        /// </summary>
        float[] SampledHeights { get; }
        /// <summary>
        /// Width of the map in normal coordinate space, where the origin is at (0, 0).
        /// *NOTE*: Not to be confused with MaxGridPosition.X, whos origin is at MinGridPosition.
        /// </summary>
        float MapWidth { get; }
        /// <summary>
        /// Height of the map in normal coordinate space, where the origin is at (0, 0).
        /// *NOTE*: Not to be confused with MaxGridPosition.Z, whos origin is at MinGridPosition.
        /// </summary>
        float MapHeight { get; }
        /// <summary>
        /// Center of the map in normal coordinate space.
        /// </summary>
        Vector2 MiddleOfMap { get; }

        /// <summary>
        /// Finds a path of waypoints, which are aligned by the cells of the navgrid (A* method), that lead to a set destination.
        /// </summary>
        /// <param name="from">Point that the path starts at.</param>
        /// <param name="to">Point that the path ends at.</param>
        /// <param name="distanceThreshold">Amount of distance away from terrain that the path should be.</param>
        /// <returns>List of points forming a path in order: from -> to</returns>
        List<Vector2> GetPath(Vector2 from, Vector2 to, float distanceThreshold = 0);
        /// <summary>
        /// Translates the given Vector2 into cell format where each unit is a cell.
        /// This is to simplify the calculations required to get cells.
        /// </summary>
        /// <param name="vector">Vector2 to translate.</param>
        /// <returns>Cell formatted Vector2.</returns>
        Vector2 TranslateToNavGrid(Vector2 vector);
        /// <summary>
        /// Translates the given cell formatted Vector2 back into normal coordinate space.
        /// </summary>
        /// <param name="vector">Vector2 to translate.</param>
        /// <returns>Normal coordinate space Vector2.</returns>
        Vector2 TranslateFrmNavigationGrid(Vector2 vector);
        /// <summary>
        /// Gets the index of the cell at the given position.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="z">2D Y coordinate.</param>
        /// <param name="translate">Whether or not the given position should be translated into cell format. False = given position is already in cell format.</param>
        /// <returns>Cell index.</returns>
        int GetCellIndex(float x, float z, bool translate = true);
        /// <summary>
        /// Gets the index of a cell that is closest to the given 2D point.
        /// Usually used when the given point is outside the boundaries of the Navigation Grid.
        /// </summary>
        /// <param name="x">X coordinate to check.</param>
        /// <param name="y">Y coordinate to check.</param>
        /// <param name="translate">Whether or not the given coordinates are in LS form.</param>
        /// <returns>Index of a valid cell.</returns>
        int GetClosestValidCellIndex(float x, float y, bool translate = true);
        /// <summary>
        /// Whether or not the cell at the given position can be pathed on.
        /// </summary>
        /// <param name="x">X coordinate to check.</param>
        /// <param name="y">Y coordinate to check,</param>
        /// <param name="checkRadius">Radius around the given point to check for walkability.</param>
        /// <param name="translate">Whether or not to translate the given position to cell-based format.</param>
        /// <returns>True/False.</returns>
        bool IsWalkable(float x, float y, float checkRadius = 0, bool translate = true);
        /// <summary>
        /// Whether or not the cell at the given position can be pathed on.
        /// </summary>
        /// <param name="coords">Vector2 position to check.</param>
        /// <param name="checkRadius">Radius around the given point to check for walkability.</param>
        /// <param name="translate">Whether or not to translate the given position to cell-based format.</param>
        /// <returns>True/False.</returns>
        bool IsWalkable(Vector2 coords, float checkRadius = 0, bool translate = true);
        /// <summary>
        /// Whether or not the given position is see-through. In other words, if it does not block vision.
        /// </summary>
        /// <param name="x">X coordinate to check.</param>
        /// <param name="y">Y coordinate to check.</param>
        /// <param name="translate">Whether or not to translate the given position to cell-based format.</param>
        /// <returns>True/False.</returns>
        bool IsVisible(float x, float y, bool translate = true);
        /// <summary>
        /// Whether or not the given position is see-through. In other words, if it does not block vision.
        /// </summary>
        /// <param name="coords">Vector2 position to check.</param>
        /// <param name="translate">Whether or not to translate the given position to cell-based format.</param>
        /// <returns>True/False.</returns>
        bool IsVisible(Vector2 coords, bool translate = true);
        /// <summary>
        /// Whether or not the given position has the specified flags.
        /// </summary>
        /// <param name="coords">Vector2 position to check.</param>
        /// <param name="translate">Whether or not to translate the given position to cell-based format.</param>
        /// <returns>True/False.</returns>
        bool HasFlag(Vector2 coords, NavigationGridCellFlags flag, bool translate = true);
        /// <summary>
        /// Gets the height of the ground at the given position. Used purely for packets.
        /// </summary>
        /// <param name="location">Vector2 position to check.</param>
        /// <returns>Height (3D Y coordinate) at the given position.</returns>
        float GetHeightAtLocation(Vector2 location);
        /// <summary>
        /// Gets the height of the ground at the given position. Used purely for packets.
        /// </summary>
        /// <param name="x">X coordinate to check.</param>
        /// <param name="y">Y coordinate to check.</param>
        /// <returns>Height (3D Y coordinate) at the given position.</returns>
        float GetHeightAtLocation(float x, float y);
        /// <summary>
        /// Whether or not there is anything blocking pathing or vision from the starting position to the ending position. (depending on checkVision).
        /// </summary>
        /// <param name="startPos">Position to start the check from.</param>
        /// <param name="endPos">Position to end the check at.</param>
        /// <param name="checkVision">True = Check if vision is blocked. False = Check if pathing is blocked.</param>
        /// <returns>True/False.</returns>
        KeyValuePair<bool, Vector2> IsAnythingBetween(Vector2 startPos, Vector2 endPos, bool checkVision = false);
        /// <summary>
        /// Whether or not there is anything blocking the two given GameObjects from either seeing eachother or pathing straight towards eachother (depending on checkVision).
        /// </summary>
        /// <param name="a">GameObject to start the check from.</param>
        /// <param name="b">GameObject to end the check at.</param>
        /// <param name="checkVision">True = Check for positions that block vision. False = Check for positions that block pathing.</param>
        /// <returns>True/False.</returns>
        bool IsAnythingBetween(IGameObject a, IGameObject b, bool checkVision = false);
        /// <summary>
        /// Gets the closest pathable position to the given position. *NOTE*: Computationally heavy, use sparingly.
        /// </summary>
        /// <param name="location">Vector2 position to start the check at.</param>
        /// <param name="distanceThreshold">Amount of distance away from terrain that the exit should be.</param>
        /// <returns>Vector2 position which can be pathed on.</returns>
        Vector2 GetClosestTerrainExit(Vector2 location, float distanceThreshold = 0);
    }
}
