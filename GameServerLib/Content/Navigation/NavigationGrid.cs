using System;
using System.Collections.Generic;
using System.IO;
using GameServerCore;
using GameServerCore.Content;
using GameServerCore.Domain.GameObjects;
using Priority_Queue;
using Vector2 = System.Numerics.Vector2;
using System.Numerics;
using GameServerLib.Extensions;
using GameServerCore.Enums;

namespace LeagueSandbox.GameServer.Content.Navigation
{
    public class NavigationGrid : INavigationGrid
    {
        /// <summary>
        /// The minimum position on the NavigationGrid in normal coordinate space (bottom left in 2D).
        /// NavigationGridCells are undefined below these minimums.
        /// </summary>
        public Vector3 MinGridPosition { get; private set; }
        /// <summary>
        /// The maximum position on the NavigationGrid in normal coordinate space (top right in 2D).
        /// NavigationGridCells are undefined beyond these maximums.
        /// </summary>
        public Vector3 MaxGridPosition { get; private set; }
        /// <summary>
        /// Calculated resolution of the Navigation Grid (percentage of a cell 1 normal unit takes up, not to be confused with 1/CellSize).
        /// Multiple used to convert cell-based coordinates back into normal coordinates (CellCountX/Z / TranslationMaxGridPosition).
        /// </summary>
        public Vector3 TranslationMaxGridPosition { get; private set; }
        /// <summary>
        /// Ideal number of normal units a cell takes up (not fully accurate, but mostly, refer to TranslationMaxGridPosition for true size).
        /// </summary>
        public float CellSize { get; private set; }
        /// <summary>
        /// Width of the Navigation Grid in cells.
        /// </summary>
        public uint CellCountX { get; private set; }
        /// <summary>
        /// Height of the Navigation Grid in cells.
        /// </summary>
        public uint CellCountY { get; private set; }
        /// <summary>
        /// Array of all cells contained in this Navigation Grid.
        /// </summary>
        public NavigationGridCell[] Cells { get; private set; }
        /// <summary>
        /// Array of region tags where each index represents a cell's index.
        /// </summary>
        public uint[] RegionTags { get; private set; }
        /// <summary>
        /// Table of regions possible in the current Navigation Grid.
        /// Regions are the areas representing key points on a map. In the case of OldSR, this could be lanes top, middle, or bot, and the last region being jungle.
        /// *NOTE*: Regions only exist in Navigation Grids with a version of 5 or higher. OldSR is version 3.
        /// </summary>
        public NavigationRegionTagTable RegionTagTable { get; private set; }
        /// <summary>
        /// Number of sampled heights in the X coordinate plane.
        /// </summary>
        public uint SampledHeightsCountX { get; private set; }
        /// <summary>
        /// Number of sampled heights in the Y coordinate plane (Z coordinate in 3D space).
        /// </summary>
        public uint SampledHeightsCountY { get; private set; }
        /// <summary>
        /// Multiple used to convert from normal coordinates to an index format used to get sampled heights from the Navigation Grid.
        /// </summary>
        /// TODO: Seems to be volatile. If there ever comes a time when Navigation Grid editing becomes easy, that'd be the perfect time to rework the methods for getting sampled heights.
        public Vector2 SampledHeightsDistance { get; private set; }
        /// <summary>
        /// Array of sampled heights where each index represents a cell's index (depends on SampledHeightsCountX/Y).
        /// </summary>
        public float[] SampledHeights { get; private set; }
        /// <summary>
        /// Grid of hints.
        /// Function likely related to pathfinding.
        /// Currently Unused.
        /// </summary>
        public NavigationHintGrid HintGrid { get; private set; }
        /// <summary>
        /// Width of the map in normal coordinate space, where the origin is at (0, 0).
        /// *NOTE*: Not to be confused with MaxGridPosition.X, whos origin is at MinGridPosition.
        /// </summary>
        public float MapWidth { get; private set; }
        /// <summary>
        /// Height of the map in normal coordinate space, where the origin is at (0, 0).
        /// *NOTE*: Not to be confused with MaxGridPosition.Z, whos origin is at MinGridPosition.
        /// </summary>
        public float MapHeight { get; private set; }
        /// <summary>
        /// Center of the map in normal coordinate space.
        /// </summary>
        public Vector2 MiddleOfMap { get; private set; }

        public NavigationGrid(string fileLocation) : this(File.OpenRead(fileLocation)) { }
        public NavigationGrid(byte[] buffer) : this(new MemoryStream(buffer)) { }
        public NavigationGrid(Stream stream)
        {
            using (BinaryReader br = new BinaryReader(stream))
            {
                byte major = br.ReadByte();
                ushort minor = major != 2 ? br.ReadUInt16() : (ushort)0;
                if (major != 2 && major != 3 && major != 5 && major != 7)
                {
                    throw new Exception(string.Format("Unsupported Navigation Grid Version: {0}.{1}", major, minor));
                }

                this.MinGridPosition = br.ReadVector3();
                this.MaxGridPosition = br.ReadVector3();

                this.CellSize = br.ReadSingle();
                this.CellCountX = br.ReadUInt32();
                this.CellCountY = br.ReadUInt32();

                this.Cells = new NavigationGridCell[this.CellCountX * this.CellCountY];
                this.RegionTags = new uint[this.CellCountX * this.CellCountY];

                if (major == 2 || major == 3 || major == 5)
                {
                    for (int i = 0; i < this.Cells.Length; i++)
                    {
                        this.Cells[i] = NavigationGridCell.ReadVersion5(br, i);
                    }

                    if (major == 5)
                    {
                        for (int i = 0; i < this.RegionTags.Length; i++)
                        {
                            this.RegionTags[i] = br.ReadUInt16();
                        }
                    }
                }
                else if (major == 7)
                {
                    for (int i = 0; i < this.Cells.Length; i++)
                    {
                        this.Cells[i] = NavigationGridCell.ReadVersion7(br, i);
                    }
                    for (int i = 0; i < this.Cells.Length; i++)
                    {
                        this.Cells[i].SetFlags((NavigationGridCellFlags)br.ReadUInt16());
                    }

                    for (int i = 0; i < this.RegionTags.Length; i++)
                    {
                        this.RegionTags[i] = br.ReadUInt32();
                    }
                }

                if(major >= 5)
                {
                    uint groupCount = major == 5 ? 4u : 8u;
                    this.RegionTagTable = new NavigationRegionTagTable(br, groupCount);
                }

                this.SampledHeightsCountX = br.ReadUInt32();
                this.SampledHeightsCountY = br.ReadUInt32();
                this.SampledHeightsDistance = br.ReadVector2();
                this.SampledHeights = new float[this.SampledHeightsCountX * this.SampledHeightsCountY];
                for (int i = 0; i < this.SampledHeights.Length; i++)
                {
                    this.SampledHeights[i] = br.ReadSingle();
                }

                this.HintGrid = new NavigationHintGrid(br);

                this.MapWidth = this.MaxGridPosition.X + this.MinGridPosition.X;
                this.MapHeight = this.MaxGridPosition.Z + this.MinGridPosition.Z;
                this.MiddleOfMap = new Vector2(this.MapWidth / 2, this.MapHeight / 2);
                this.TranslationMaxGridPosition = new Vector3
                {
                    X = this.CellCountX / (this.MaxGridPosition.X - this.MinGridPosition.X),
                    Z = this.CellCountY / (this.MaxGridPosition.Z - this.MinGridPosition.Z)
                };
            }
        }

        /// <summary>
        /// Finds a path of waypoints, which are aligned by the cells of the navgrid (A* method), that lead to a set destination.
        /// </summary>
        /// <param name="from">Point that the path starts at.</param>
        /// <param name="to">Point that the path ends at.</param>
        /// <param name="distanceThreshold">Amount of distance away from terrain that the path should be.</param>
        /// <returns>List of points forming a path in order: from -> to</returns>
        public List<Vector2> GetPath(Vector2 from, Vector2 to, float distanceThreshold = 0)
        {
            List<Vector2> returnList = new List<Vector2>() { from };

            Vector2 vectorFrom = TranslateToNavGrid(from);
            NavigationGridCell cellFrom = GetCell((short)vectorFrom.X, (short)vectorFrom.Y);

            Vector2 vectorTo = TranslateToNavGrid(to);
            NavigationGridCell goal = GetCell((short)vectorTo.X, (short)vectorTo.Y);

            if (cellFrom != null && goal != null)
            {
                SimplePriorityQueue<Stack<NavigationGridCell>> priorityQueue = new SimplePriorityQueue<Stack<NavigationGridCell>>();

                Stack<NavigationGridCell> start = new Stack<NavigationGridCell>();
                start.Push(cellFrom);
                priorityQueue.Enqueue(start, NavigationGridCell.Distance(cellFrom, goal));

                Dictionary<int, NavigationGridCell> closedList = new Dictionary<int, NavigationGridCell>();
                closedList.Add(cellFrom.ID, cellFrom);

                Stack<NavigationGridCell> path = null;

                // while there are still paths to explore
                while (true)
                {
                    if (!priorityQueue.TryFirst(out path))
                    {
                        // no solution
                        path = null;
                        return null;
                    }

                    float currentCost = priorityQueue.GetPriority(priorityQueue.First);
                    priorityQueue.TryDequeue(out path);

                    NavigationGridCell cell = path.Peek();
                    currentCost -= (NavigationGridCell.Distance(cell, goal) + cell.Heuristic); // decrease the heuristic to get the cost

                    // found the min solution and return it (path)
                    if (cell.ID == goal.ID)
                    {
                        break;
                    }

                    NavigationGridCell tempCell = null;
                    foreach (NavigationGridCell neighborCell in GetCellNeighbors(cell))
                    {
                        // if the neighbor is in the closed list - skip
                        if (closedList.TryGetValue(neighborCell.ID, out tempCell))
                        {
                            continue;
                        }

                        // not walkable - skip
                        if (distanceThreshold != 0 && !IsWalkable(neighborCell.Locator.X, neighborCell.Locator.Y, distanceThreshold, false))
                        {
                            closedList.Add(neighborCell.ID, neighborCell);
                            continue;
                        }
                        else if (neighborCell.HasFlag(NavigationGridCellFlags.NOT_PASSABLE) || neighborCell.HasFlag(NavigationGridCellFlags.SEE_THROUGH))
                        {
                            closedList.Add(neighborCell.ID, neighborCell);
                            continue;
                        }

                        // calculate the new path and cost +heuristic and add to the priority queue
                        Stack<NavigationGridCell> npath = new Stack<NavigationGridCell>(new Stack<NavigationGridCell>(path));
                        npath.Push(neighborCell);

                        // add 1 for every cell used
                        priorityQueue.Enqueue(npath, currentCost + 1 + neighborCell.Heuristic + neighborCell.ArrivalCost + neighborCell.AdditionalCost
                            + NavigationGridCell.Distance(neighborCell, goal));

                        closedList.Add(neighborCell.ID, neighborCell);
                    }
                }

                // shouldn't happen usually
                if (path == null)
                {
                    return null;
                }

                var pathList = new List<NavigationGridCell>(path);
                pathList.Reverse();
                pathList = SmoothPath(pathList);

                // removes the first point
                pathList.RemoveAt(0);

                foreach (NavigationGridCell navGridCell in pathList.ToArray())
                {
                    returnList.Add(TranslateFrmNavigationGrid(navGridCell.Locator));
                }
                return returnList;
            }

            return null;
        }

        /// <summary>
        /// Remove waypoints (cells) that have LOS from one to the other.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>Smoothed list of waypoints.</returns>
        private List<NavigationGridCell> SmoothPath(List<NavigationGridCell> path)
        {
            int curWaypointToSmooth = 0;
            int i = path.Count - 1;
            while (curWaypointToSmooth != path.Count - 1)
            {
                // no waypoint in LOS - continue to smooth the next one
                if (i <= curWaypointToSmooth + 1)
                {
                    curWaypointToSmooth++;
                    i = path.Count - 1;
                }
                // if the next point in the LOS, remove the current one
                // TODO: equal of floats should be with epsilon
                else if (!IsAnythingBetween(path[curWaypointToSmooth], path[i]))
                {
                    path.RemoveRange(curWaypointToSmooth + 1, i - (curWaypointToSmooth + 1));
                    curWaypointToSmooth++; // the remove function removed all the indexes between them
                    i = path.Count - 1;
                }
                else
                {
                    --i;
                }
            }
            return path;
        }

        /// <summary>
        /// Translates the given Vector2 into cell format where each unit is a cell.
        /// This is to simplify the calculations required to get cells.
        /// </summary>
        /// <param name="vector">Vector2 to translate.</param>
        /// <returns>Cell formatted Vector2.</returns>
        public Vector2 TranslateToNavGrid(Vector2 vector)
        {
            vector.X = (vector.X - this.MinGridPosition.X) * this.TranslationMaxGridPosition.X;
            vector.Y = (vector.Y - this.MinGridPosition.Z) * this.TranslationMaxGridPosition.Z;
            return vector;
        }

        /// <summary>
        /// Translates the given cell locator position back into normal coordinate space as a Vector2.
        /// </summary>
        /// <param name="locator">Cell locator.</param>
        /// <returns>Normal coordinate space Vector2.</returns>
        public Vector2 TranslateFrmNavigationGrid(NavigationGridLocator locator)
        {
            return TranslateFrmNavigationGrid(new Vector2(locator.X, locator.Y));
        }

        /// <summary>
        /// Translates the given cell formatted Vector2 back into normal coordinate space.
        /// </summary>
        /// <param name="vector">Vector2 to translate.</param>
        /// <returns>Normal coordinate space Vector2.</returns>
        public Vector2 TranslateFrmNavigationGrid(Vector2 vector)
        {
            Vector2 ret = new Vector2
            {
                X = vector.X / this.TranslationMaxGridPosition.X + this.MinGridPosition.X,
                Y = vector.Y / this.TranslationMaxGridPosition.Z + this.MinGridPosition.Z
            };

            return ret;
        }

        /// <summary>
        /// Gets the index of the cell at the given position.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="z">2D Y coordinate.</param>
        /// <param name="translate">Whether or not the given position should be translated into cell format. False = given position is already in cell format.</param>
        /// <returns>Cell index.</returns>
        public int GetCellIndex(float x, float z, bool translate = true)
        {
            Vector2 vector = new Vector2(x, z);

            if (translate)
            {
                vector = TranslateToNavGrid(new Vector2 { X = x, Y = z });
            }

            // TODO: Cleanup all the casting but keep the same method.
            long index = (short)vector.Y * this.CellCountX + (short)vector.X;
            if ((short)vector.X < 0 || (short)vector.X > this.CellCountX || (short)vector.Y < 0 || (short)vector.Y > this.CellCountY || index >= this.Cells.Length)
            {
                return -1;
            }

            return (int)index;
        }

        /// <summary>
        /// Gets the cell at the given cell based coordinates.
        /// </summary>
        /// <param name="x">cell based X coordinate</param>
        /// <param name="y">cell based Y coordinate.</param>
        /// <returns>Cell instance.</returns>
        public NavigationGridCell GetCell(short x, short y)
        {
            long index = y * this.CellCountX + x;
            if (x < 0 || x > this.CellCountX || y < 0 || y > this.CellCountY || index >= this.Cells.Length)
            {
                return null;
            }

            return this.Cells[index];
        }

        /// <summary>
        /// Gets a list of all cells within 8 cardinal directions of the given cell.
        /// </summary>
        /// <param name="cell">Cell to start the check at.</param>
        /// <returns>List of neighboring cells.</returns>
        private List<NavigationGridCell> GetCellNeighbors(NavigationGridCell cell)
        {
            List<NavigationGridCell> neighbors = new List<NavigationGridCell>();
            for (short dirY = -1; dirY <= 1; dirY++)
            {
                for (short dirX = -1; dirX <= 1; dirX++)
                {
                    short nx = (short)(cell.Locator.X + dirX);
                    short ny = (short)(cell.Locator.Y + dirY);
                    NavigationGridCell neighborCell = GetCell(nx, ny);
                    if (neighborCell != null)
                    {
                        neighbors.Add(neighborCell);
                    }
                }
            }
            return neighbors;
        }

        /// <summary>
        /// Gets the index of a cell that is closest to the given 2D point.
        /// Usually used when the given point is outside the boundaries of the Navigation Grid.
        /// </summary>
        /// <param name="x">X coordinate to check.</param>
        /// <param name="y">Y coordinate to check.</param>
        /// <param name="translate">Whether or not the given coordinates are in LS form.</param>
        /// <returns>Index of a valid cell.</returns>
        public int GetClosestValidCellIndex(float x, float y, bool translate = true)
        {
            Vector3 minGridPos = MinGridPosition;
            // Because indices are from 0, we subtract a single cell's size from the maximums to prevent getting -1 cell index.
            Vector3 maxGridPos = new Vector3(MaxGridPosition.X - (1 / TranslationMaxGridPosition.X), MaxGridPosition.Y, MaxGridPosition.Z - (1 / TranslationMaxGridPosition.Z));

            if (!translate)
            {
                minGridPos = Vector3.Zero;
                Vector2 max2D = TranslateToNavGrid(new Vector2(maxGridPos.X, maxGridPos.Z));
                maxGridPos = new Vector3(max2D.X, 0, max2D.Y);
            }

            if (Extensions.IsVectorValid(new Vector2(x, y), new Vector2(maxGridPos.X, maxGridPos.Z), new Vector2(minGridPos.X, minGridPos.Z)))
            {
                return GetCellIndex(x, y, translate);
            }

            // Left
            if (x < minGridPos.X)
            {
                // Bottom
                if (y < minGridPos.Z)
                {
                    return Cells.Length - (int)CellCountX;
                }
                // Top
                else if (y > maxGridPos.Z)
                {
                    return 0;
                }

                // Middle
                return GetCellIndex(minGridPos.X, y, translate);
            }

            // Right
            if (x > maxGridPos.X)
            {
                // Bottom
                if (y < minGridPos.Z)
                {
                    return Cells.Length - 1;
                }
                // Top
                else if (y > maxGridPos.Z)
                {
                    return (int)CellCountX;
                }

                // Middle
                return GetCellIndex(maxGridPos.X, y, translate);
            }

            // Bottom
            if (y < minGridPos.Z)
            {
                return GetCellIndex(x, minGridPos.Z, translate);
            }

            // Top
            return GetCellIndex(x, maxGridPos.Z, translate);
        }

        /// <summary>
        /// Gets a list of cells within the specified range of a specified point.
        /// </summary>
        /// <param name="origin">Vector2 with normal coordinates to start the check. *NOTE*: Must be untranslated (normal coordinates).</param>
        /// <param name="radius">Range to check around the origin.</param>
        /// <returns>List of all cells in range. Null if range extends outside of NavigationGrid boundaries.</returns>
        private List<NavigationGridCell> GetAllCellsInRange(Vector2 origin, float radius, bool translate = true)
        {
            List<NavigationGridCell> cells = new List<NavigationGridCell>();
            
            float stepX = radius;
            float stepY = radius;
            Vector2 trueOrigin = origin;
            if(!translate)
            {
                stepX *= this.TranslationMaxGridPosition.X;
                stepY *= this.TranslationMaxGridPosition.Z;
                trueOrigin = TranslateFrmNavigationGrid(origin);
            }

            // Ordered: bottom left, bottom right, top right.
            int[] cellIndices = new int[3]
            {
                GetCellIndex(origin.X - stepX, origin.Y - stepY, translate),
                GetCellIndex(origin.X + stepX, origin.Y - stepY, translate),
                GetCellIndex(origin.X + stepX, origin.Y + stepY, translate)
            };

            int rowIndex = cellIndices[0];
            int columnIndex = cellIndices[1];
            for (int i = cellIndices[0]; i <= cellIndices[2]; i++)
            {
                if (i > columnIndex)
                {
                    i = rowIndex + (int)CellCountX;
                    rowIndex = i;
                    columnIndex += (int)CellCountX;
                }

                if (i >= Cells.Length || i < 0)
                {
                    break;
                }

                NavigationGridCell cell = Cells[i];

                if (Extensions.DistanceSquaredToRectangle(TranslateFrmNavigationGrid(cell.Locator), 1.0f / this.TranslationMaxGridPosition.X, 1.0f / this.TranslationMaxGridPosition.Z, trueOrigin) <= radius * radius)
                {
                    cells.Add(cell);
                }
            }

            return cells;
        }

        /// <summary>
        /// Whether or not the cell at the given position can be pathed on.
        /// </summary>
        /// <param name="x">X coordinate to check.</param>
        /// <param name="y">Y coordinate to check,</param>
        /// <param name="checkRadius">Radius around the given point to check for walkability.</param>
        /// <param name="translate">Whether or not to translate the given position to cell-based format.</param>
        /// <returns>True/False.</returns>
        public bool IsWalkable(float x, float y, float checkRadius = 0, bool translate = true)
        {
            return IsWalkable(new Vector2(x, y), checkRadius, translate);
        }

        /// <summary>
        /// Whether or not the cell at the given position can be pathed on.
        /// </summary>
        /// <param name="coords">Vector2 position to check.</param>
        /// <param name="checkRadius">Radius around the given point to check for walkability.</param>
        /// <param name="translate">Whether or not to translate the given position to cell-based format.</param>
        /// <returns>True/False.</returns>
        public bool IsWalkable(Vector2 coords, float checkRadius = 0, bool translate = true)
        {
            if (checkRadius == 0)
            {
                Vector2 vector = new Vector2 { X = coords.X, Y = coords.Y };

                if (translate)
                {
                    vector = TranslateToNavGrid(new Vector2 { X = coords.X, Y = coords.Y });
                }

                NavigationGridCell cell = GetCell((short)vector.X, (short)vector.Y);

                return cell != null && !cell.HasFlag(NavigationGridCellFlags.NOT_PASSABLE) && !cell.HasFlag(NavigationGridCellFlags.SEE_THROUGH);
            }

            List<NavigationGridCell> cells = GetAllCellsInRange(coords, checkRadius, translate);

            if (cells.Count == 0)
            {
                return false;
            }

            foreach (NavigationGridCell c in cells)
            {
                if (c == null || c.HasFlag(NavigationGridCellFlags.NOT_PASSABLE) || c.HasFlag(NavigationGridCellFlags.SEE_THROUGH))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Whether or not the given position is see-through. In other words, if it does not block vision.
        /// </summary>
        /// <param name="x">X coordinate to check.</param>
        /// <param name="y">Y coordinate to check.</param>
        /// <param name="translate">Whether or not to translate the given position to cell-based format.</param>
        /// <returns>True/False.</returns>
        public bool IsVisible(float x, float y, bool translate = true)
        {
            return IsVisible(new Vector2(x, y), translate);
        }

        /// <summary>
        /// Whether or not the given position is see-through. In other words, if it does not block vision.
        /// </summary>
        /// <param name="coords">Vector2 position to check.</param>
        /// <param name="translate">Whether or not to translate the given position to cell-based format.</param>
        /// <returns>True/False.</returns>
        public bool IsVisible(Vector2 coords, bool translate = true)
        {
            Vector2 vector = new Vector2 { X = coords.X, Y = coords.Y };

            if (translate)
            {
                vector = TranslateToNavGrid(new Vector2 { X = coords.X, Y = coords.Y });
            }

            NavigationGridCell cell = GetCell((short)vector.X, (short)vector.Y);

            //TODO: implement bush logic here
            return IsVisible(cell);
        }

        bool IsVisible(NavigationGridCell cell)
        {
            return cell != null 
                && (!cell.HasFlag(NavigationGridCellFlags.NOT_PASSABLE) 
                || cell.HasFlag(NavigationGridCellFlags.SEE_THROUGH)
                || cell.HasFlag(NavigationGridCellFlags.HAS_GLOBAL_VISION));
        }

        /// <summary>
        /// Whether or not the given position has the specified flags.
        /// </summary>
        /// <param name="coords">Vector2 position to check.</param>
        /// <param name="translate">Whether or not to translate the given position to cell-based format.</param>
        /// <returns>True/False.</returns>
        public bool HasFlag(Vector2 coords, NavigationGridCellFlags flag, bool translate = true)
        {
            Vector2 vector = new Vector2 { X = coords.X, Y = coords.Y };

            if (translate)
            {
                vector = TranslateToNavGrid(new Vector2 { X = coords.X, Y = coords.Y });
            }

            NavigationGridCell cell = GetCell((short)vector.X, (short)vector.Y);

            return cell != null && cell.HasFlag(flag);
        }

        /// <summary>
        /// Gets the height of the ground at the given position. Used purely for packets.
        /// </summary>
        /// <param name="location">Vector2 position to check.</param>
        /// <returns>Height (3D Y coordinate) at the given position.</returns>
        public float GetHeightAtLocation(Vector2 location)
        {
            // Uses SampledHeights to get the height of a given location on the Navigation Grid
            // This is the method the game uses to get height data

            if (location.X >= this.MinGridPosition.X && location.Y >= this.MinGridPosition.Z &&
                location.X <= this.MaxGridPosition.X && location.Y <= this.MaxGridPosition.Z)
            {
                float reguestedHeightX = (location.X - this.MinGridPosition.X) / this.SampledHeightsDistance.X;
                float requestedHeightY = (location.Y - this.MinGridPosition.Z) / this.SampledHeightsDistance.Y;
                
                int sampledHeight1IndexX = (int)reguestedHeightX;
                int sampledHeight1IndexY = (int)requestedHeightY;
                int sampledHeight2IndexX;
                int sampledHeight2IndexY;

                float v13;
                float v15;

                if (reguestedHeightX >= this.SampledHeightsCountX - 1)
                {
                    v13 = 1.0f;
                    sampledHeight2IndexX = sampledHeight1IndexX--;
                }
                else
                {
                    v13 = 0.0f;
                    sampledHeight2IndexX = sampledHeight1IndexX + 1;
                }
                if (requestedHeightY >= this.SampledHeightsCountY - 1)
                {
                    v15 = 1.0f;
                    sampledHeight2IndexY = sampledHeight1IndexY--;
                }
                else
                {
                    v15 = 0.0f;
                    sampledHeight2IndexY = sampledHeight1IndexY + 1;
                }

                uint sampledHeightsCount = this.SampledHeightsCountX * this.SampledHeightsCountY;
                int v1 = (int)this.SampledHeightsCountX * sampledHeight1IndexY;
                int x0y0 = v1 + sampledHeight1IndexX;

                if (v1 + sampledHeight1IndexX < sampledHeightsCount)
                {
                    int v19 = sampledHeight2IndexX + v1;
                    if (v19 < sampledHeightsCount)
                    {
                        int v20 = sampledHeight2IndexY * (int)this.SampledHeightsCountX;
                        int v21 = v20 + sampledHeight1IndexX;

                        if (v21 < sampledHeightsCount)
                        {
                            int v22 = sampledHeight2IndexX + v20;
                            if (v22 < sampledHeightsCount)
                            {
                                float height = ((1.0f - v13) * this.SampledHeights[x0y0])
                                          + (v13 * this.SampledHeights[v19])
                                          + (((this.SampledHeights[v21] * (1.0f - v13))
                                          + (this.SampledHeights[v22] * v13)) * v15);

                                return (1.0f - v15) * height;
                            }
                        }
                    }
                }

            }

            return 0.0f;
        }

        /// <summary>
        /// Gets the height of the ground at the given position. Used purely for packets.
        /// </summary>
        /// <param name="x">X coordinate to check.</param>
        /// <param name="y">Y coordinate to check.</param>
        /// <returns>Height (3D Y coordinate) at the given position.</returns>
        public float GetHeightAtLocation(float x, float y)
        {
            return GetHeightAtLocation(new Vector2(x, y));
        }

        /// <summary>
        /// Casts a ray and returns false when failed, with a stopping position, or true on success with the given destination.
        /// </summary>
        /// <param name="origin">Vector position to start the ray cast from.</param>
        /// <param name="destination">Vector2 position to end the ray cast at.</param>
        /// <param name="checkWalkable">Whether or not the ray stops when hitting a position which blocks pathing.</param>
        /// <param name="checkVisible">Whether or not the ray stops when hitting a position which blocks vision.</param>
        /// <returns>True = Reached destination with destination. False = Failed, with stopping position.</returns>
        public KeyValuePair<bool, Vector2> CastRay(Vector2 origin, Vector2 destination, bool checkWalkable = false, bool checkVisible = false)
        {
            // Out of bounds
            if (origin.X < MinGridPosition.X || origin.X >= MaxGridPosition.X || origin.Y < MinGridPosition.Z || origin.Y >= MaxGridPosition.Z)
            {
                return new KeyValuePair<bool, Vector2>(false, new Vector2(float.NaN, float.NaN));
            }

            origin = TranslateToNavGrid(origin);
            destination = TranslateToNavGrid(destination);

            Vector2 dist = destination - origin;
            float greatestdist = Math.Max(
                Math.Abs(dist.X),
                Math.Abs(dist.Y)
            );

            int i;
            int il = (int)greatestdist;
            Vector2 d = dist / greatestdist;

            bool prevPosHadBush = HasFlag(origin, NavigationGridCellFlags.HAS_GRASS, false);
            bool destinationHasGrass = HasFlag(destination, NavigationGridCellFlags.HAS_GRASS, false);

            for (i = 0; i < il; i++)
            {
                
                //TODO: Implement methods for maps whose NavGrids don't use SEE_THROUGH flags for buildings
                if (checkWalkable)
                {
                    if(!IsWalkable(origin, translate: false))
                    {
                        break;
                    }
                }
                
                if (checkVisible)
                {
                    var cell = GetCell((short)origin.X, (short)origin.Y);

                    if (!IsVisible(cell))
                    {
                        break;
                    }

                    bool isGrass = cell.HasFlag(NavigationGridCellFlags.HAS_GRASS);

                    // If you are outside of a bush
                    if (!prevPosHadBush)
                    {
                        if (isGrass)
                        {
                            break;
                        }
                    }

                    // If you are in a different bush
                    if (prevPosHadBush && destinationHasGrass)
                    {
                        if (!isGrass)
                        {
                            break;
                        }
                    }
                }

                // if checkWalkable == true, stop incrementing when (x1, x2) is a see-able position
                // if checkWalkable == false, stop incrementing when (x1, x2) is a non-see-able position
                origin += d;
            }

            return new KeyValuePair<bool, Vector2>(
                i == il, TranslateFrmNavigationGrid(origin)
            );
        }

        /// <summary>
        /// Casts a ray in the given direction and returns false when failed, with a stopping position, or true on success with the given destination.
        /// *NOTE*: Is not actually infinite, just travels (direction * 1024) units ahead of the given origin.
        /// </summary>
        /// <param name="origin">Vector position to start the ray cast from.</param>
        /// <param name="direction">Ray cast direction.</param>
        /// <param name="checkWalkable">Whether or not the ray stops when hitting a position which blocks pathing.</param>
        /// <param name="checkVisible">Whether or not the ray stops when hitting a position which blocks vision. *NOTE*: Does not apply if checkWalkable is also true.</param>
        /// <returns>True = Reached destination with destination. False = Failed, with stopping position.</returns>
        public KeyValuePair<bool, Vector2> CastInfiniteRay(Vector2 origin, Vector2 direction, bool checkWalkable = true, bool checkVisible = false)
        {
            return CastRay(origin, origin + direction * 1024, checkWalkable, checkVisible);
        }

        /// <summary>
        /// Whether or not there is anything blocking pathing or vision from the starting position to the ending position. (depending on checkVision).
        /// </summary>
        /// <param name="startPos">Position to start the check from.</param>
        /// <param name="endPos">Position to end the check at.</param>
        /// <param name="checkVision">True = Check if vision is blocked. False = Check if pathing is blocked.</param>
        /// <returns>True/False.</returns>
        public KeyValuePair<bool, Vector2> IsAnythingBetween(Vector2 startPos, Vector2 endPos, bool checkVision = false)
        {
            KeyValuePair<bool, Vector2> result = CastRay(startPos, endPos, !checkVision, checkVision);
            return new KeyValuePair<bool, Vector2>(!result.Key, result.Value);
        }

        /// <summary>
        /// Whether or not there is anything blocking the two given GameObjects from either seeing eachother or pathing straight towards eachother (depending on checkVision).
        /// </summary>
        /// <param name="a">GameObject to start the check from.</param>
        /// <param name="b">GameObject to end the check at.</param>
        /// <param name="checkVision">True = Check for positions that block vision. False = Check for positions that block pathing.</param>
        /// <returns>True/False.</returns>
        public bool IsAnythingBetween(IGameObject a, IGameObject b, bool checkVision = false)
        {
            if (a is IObjBuilding)
            {
                double rayDist = Math.Sqrt((CastRay(b.Position, a.Position, !checkVision, checkVision).Value - b.Position).SqrLength());
                rayDist += a.PathfindingRadius;
                return (rayDist * rayDist) < (b.Position - a.Position).SqrLength();
            }
            if (b is IObjBuilding)
            {
                double rayDist = Math.Sqrt((CastRay(a.Position, b.Position, !checkVision, checkVision).Value - a.Position).SqrLength());
                rayDist += b.PathfindingRadius;
                return (rayDist * rayDist) < (b.Position - a.Position).SqrLength();
            }
            return !CastRay(a.Position, b.Position, !checkVision, checkVision).Key;
        }

        /// <summary>
        /// Whether or not there is anything blocking pathing from the first given cell to the next.
        /// </summary>
        /// <param name="origin">Cell to start the check from.</param>
        /// <param name="destination">Cell to end the check at.</param>
        /// <param name="ignoreTerrain">Whether or not to ignore terrain when checking for pathability between cells.</param>
        /// <returns>True/False.</returns>
        public bool IsAnythingBetween(NavigationGridCell origin, NavigationGridCell destination, bool ignoreTerrain = false)
        {
            float x1 = origin.Locator.X;
            float y1 = origin.Locator.Y;
            float x2 = destination.Locator.X;
            float y2 = destination.Locator.Y;

            if (x1 < 0 || y1 < 0 || x1 >= CellCountX || y1 >= CellCountY)
            {
                return false;
            }

            float distx = x2 - x1;
            float disty = y2 - y1;
            float greatestdist = Math.Abs(distx);
            if (Math.Abs(disty) > greatestdist)
            {
                greatestdist = Math.Abs(disty);
            }

            int il = (int)greatestdist;
            float dx = distx / greatestdist;
            float dy = disty / greatestdist;
            int i;
            for (i = 0; i <= il; i++)
            {
                if (!ignoreTerrain)
                {
                    // 4 corners
                    List<Vector2> vertices = new List<Vector2>()
                    {
                        new Vector2((short)Math.Ceiling(x1), (short)Math.Ceiling(y1)),
                        new Vector2((short)Math.Floor(x1), (short)Math.Ceiling(y1)),
                        new Vector2((short)Math.Ceiling(x1), (short)Math.Floor(y1)),
                        new Vector2((short)Math.Floor(x1), (short)Math.Floor(y1)),
                    };

                    // if none are walkable, then the path is blocked.
                    if (!vertices.Exists(v => IsWalkable(v, 0, false)))
                    {
                        return true;
                    }
                }

                // report on terrain
                x1 += dx;
                y1 += dy;
            }

            return false;
        }

        /// <summary>
        /// Gets the closest pathable position to the given position. *NOTE*: Computationally heavy, use sparingly.
        /// </summary>
        /// <param name="location">Vector2 position to start the check at.</param>
        /// <param name="distanceThreshold">Amount of distance away from terrain the exit should be.</param>
        /// <returns>Vector2 position which can be pathed on.</returns>
        public Vector2 GetClosestTerrainExit(Vector2 location, float distanceThreshold = 0)
        {
            if (IsWalkable(location, distanceThreshold))
            {
                return location;
            }

            double trueX = location.X;
            double trueY = location.Y;
            double angle = Math.PI / 4;
            // What is the point of rr?
            double rr = (location.X - trueX) * (location.X - trueX) + (location.Y - trueY) * (location.Y - trueY);
            double r = Math.Sqrt(rr);

            // x = r * cos(angle)
            // y = r * sin(angle)
            // r = distance from center
            // Draws spirals until it finds a walkable spot
            while (!IsWalkable((float)trueX, (float)trueY, distanceThreshold))
            {
                trueX = location.X + r * Math.Cos(angle);
                trueY = location.Y + r * Math.Sin(angle);
                angle += Math.PI / 4;
                r += 1;
            }

            return new Vector2((float)trueX, (float)trueY);
        }
    }
}
