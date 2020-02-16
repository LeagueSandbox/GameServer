using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using GameServerCore;
using GameServerCore.Content;
using GameServerCore.Domain.GameObjects;
using Priority_Queue;
using LeagueSandbox.GameServer.GameObjects;
using RoyT.AStar;
using Vector2 = System.Numerics.Vector2;
using System.Numerics;
using GameServerLib.Extensions;

namespace LeagueSandbox.GameServer.Content.Navigation
{
    public class NavigationGrid : INavigationGrid
    {
        public const float SCALE = 2f;

        public Vector3 MinGridPosition { get; private set; }
        public Vector3 MaxGridPosition { get; private set; }
        public Vector3 TranslationMaxGridPosition { get; private set; }

        public float CellSize { get; private set; }
        public uint CellCountX { get; private set; }
        public uint CellCountY { get; private set; }
        public NavigationGridCell[] Cells { get; private set; }

        public uint[] RegionTags { get; private set; }
        public NavigationRegionTagTable RegionTagTable { get; private set; }

        public uint SampledHeightsCountX { get; private set; }
        public uint SampledHeightsCountY { get; private set; }
        public Vector2 SampledHeightsDistance { get; private set; }
        public float[] SampledHeights { get; private set; }

        public NavigationHintGrid HintGrid { get; private set; }

        public float OffsetX { get; private set; }
        public float OffsetZ { get; private set; }
        public float MapWidth { get; private set; }
        public float MapHeight { get; private set; }
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

        public List<Vector2> GetPath(Vector2 from, Vector2 to)
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
                Dictionary<int, NavigationGridCell> closedList = new Dictionary<int, NavigationGridCell>();
                Stack<NavigationGridCell> path = null;

                start.Push(cellFrom);
                priorityQueue.Enqueue(start, NavigationGridCell.Distance(cellFrom, goal));
                closedList.Add(cellFrom.ID, cellFrom);

                // while there are still paths to explore
                while (true)
                {
                    if (!priorityQueue.TryFirst(out _))
                    {
                        // no solution
                        return null;
                    }

                    float currentCost = priorityQueue.GetPriority(priorityQueue.First);
                    priorityQueue.TryDequeue(out path);

                    NavigationGridCell cell = path.Peek();

                    currentCost -= (NavigationGridCell.Distance(cell, goal) + cell.Heuristic); // decrease the heuristic to get the cost

                    // found the min solution return it (path)
                    if (cell.ID == goal.ID)
                    {
                        break;
                    }

                    foreach (NavigationGridCell neighborCell in GetCellNeighbors(cell))
                    {
                        // if the neighbor in the closed list - skip
                        if (closedList.TryGetValue(neighborCell.ID, out _))
                        {
                            continue;
                        }

                        // not walkable - skip
                        if (neighborCell.HasFlag(NavigationGridCellFlags.NOT_PASSABLE) || neighborCell.HasFlag(NavigationGridCellFlags.SEE_THROUGH))
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

                NavigationGridCell[] pathArray = path.ToArray();
                Array.Reverse(pathArray);

                List<NavigationGridCell> pathList = SmoothPath(new List<NavigationGridCell>(pathArray));

                pathList.RemoveAt(0); // removes the first point

                foreach (NavigationGridCell navGridCell in pathList.ToArray())
                {
                    returnList.Add(TranslateFrmNavigationGrid(navGridCell.Locator));
                }

                return returnList;
            }

            return null;
        }

        /// <summary>
        /// Remove waypoints that have LOS from the waypoint before the waypoint after
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
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

        public Vector2 TranslateToNavGrid(Vector2 vector)
        {
            vector.X = (vector.X - this.MinGridPosition.X) * this.TranslationMaxGridPosition.X;
            vector.Y = (vector.Y - this.MinGridPosition.Z) * this.TranslationMaxGridPosition.Z;
            return vector;
        }
        public Vector2 TranslateFrmNavigationGrid(NavigationGridLocator locator)
        {
            return TranslateFrmNavigationGrid(new Vector2(locator.X, locator.Y));
        }
        public Vector2 TranslateFrmNavigationGrid(Vector2 vector)
        {
            Vector2 ret = new Vector2
            {
                X = vector.X / this.TranslationMaxGridPosition.X + this.MinGridPosition.X,
                Y = vector.Y / this.TranslationMaxGridPosition.Z + this.MinGridPosition.Z
            };

            return ret;
        }

        public Vector2 GetCellVector(short x, short y)
        {
            return new Vector2(UncompressX(x), UncompressZ(y));
        }

        public int GetCellIndex(short x, short y)
        {
            Vector2 vector = GetCellVector(x, y);

            if (vector.X > 0)
            {
                return (int)(vector.X + vector.Y * this.CellCountX);
            }

            return -1;
        }
        public int GetCellIndex(float x, float z)
        {
            for (int i = 0; i < this.Cells.Length; i++)
            {
                if (UncompressX(this.Cells[i].Locator.X) > x - this.CellSize && UncompressX(this.Cells[i].Locator.X) < x + this.CellSize &&
                    UncompressZ(this.Cells[i].Locator.Y) > z - this.CellSize && UncompressZ(this.Cells[i].Locator.Y) < z + this.CellSize)
                {
                    return i;
                }
            }

            return -1;
        }

        public NavigationGridCell GetCell(short x, short y)
        {
            long index = y * this.CellCountX + x;
            if (x < 0 || x > this.CellCountX || y < 0 || y > this.CellCountY || index >= this.Cells.Length)
            {
                return null;
            }

            return this.Cells[index];
        }

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

        public ushort CompressX(float positionX)
        {
            if (Math.Abs((positionX - this.OffsetX) * (1 / SCALE)) >= ushort.MaxValue)
            {
                throw new Exception("Compressed position reached maximum value.");
            }

            return (ushort)Convert.ToInt32((positionX - this.OffsetX) * (1 / SCALE));
        }
        public ushort CompressZ(float positionZ)
        {
            if (Math.Abs((positionZ - this.OffsetZ) * (1 / SCALE)) >= ushort.MaxValue)
            {
                throw new Exception("Compressed position reached maximum value.");
            }

            return (ushort)Convert.ToInt32((positionZ - this.OffsetZ) * (1 / SCALE));
        }

        public float UncompressX(short shortX)
        {
            return shortX / (1 / SCALE) + this.OffsetX;
        }
        public float UncompressZ(short shortZ)
        {
            return shortZ / (1 / SCALE) + this.OffsetZ;
        }

        public Vector2 Uncompress(Vector2 vector)
        {
            return new Vector2
            {
                X = vector.X / this.MaxGridPosition.X + this.MinGridPosition.X,
                Y = vector.Y / this.MaxGridPosition.Z + this.MinGridPosition.Z
            };
        }

        public bool IsWalkable(float x, float y)
        {
            return IsWalkable(new Vector2(x, y));
        }
        public bool IsWalkable(Vector2 coords)
        {
            Vector2 vector = TranslateToNavGrid(new Vector2 { X = coords.X, Y = coords.Y });
            NavigationGridCell cell = GetCell((short)vector.X, (short)vector.Y);

            return cell != null && !cell.HasFlag(NavigationGridCellFlags.NOT_PASSABLE);
        }

        public bool IsSeeThrough(float x, float y)
        {
            return IsSeeThrough(new Vector2(x, y));
        }
        public bool IsSeeThrough(Vector2 coords)
        {
            Vector2 vector = TranslateToNavGrid(new Vector2 { X = coords.X, Y = coords.Y });
            NavigationGridCell cell = GetCell((short)vector.X, (short)vector.Y);

            return cell != null && cell.HasFlag(NavigationGridCellFlags.SEE_THROUGH);
        }

        public bool IsBrush(float x, float y)
        {
            return IsBrush(new Vector2(x, y));
        }
        public bool IsBrush(Vector2 coords)
        {
            Vector2 vector = TranslateToNavGrid(new Vector2 { X = coords.X, Y = coords.Y });
            NavigationGridCell cell = GetCell((short)vector.X, (short)vector.Y);

            return cell != null && cell.HasFlag(NavigationGridCellFlags.HAS_GRASS);
        }

        public bool HasGlobalVision(Vector2 coords)
        {
            Vector2 vector = TranslateToNavGrid(coords);
            NavigationGridCell cell = GetCell((short)vector.X, (short)vector.Y);

            return cell != null && cell.HasFlag(NavigationGridCellFlags.HAS_GLOBAL_VISION);
        }

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
        public float GetHeightAtLocation(float x, float y)
        {
            return GetHeightAtLocation(new Vector2(x, y));
        }

        public float CastRay(Vector2 origin, Vector2 destination, bool inverseRay = false)
        {
            return (float)Math.Sqrt(CastRaySqr(origin, destination, inverseRay));
        }
        public float CastRaySqr(Vector2 origin, Vector2 destination, bool checkWalkable = false)
        {
            float x1 = origin.X;
            float y1 = origin.Y;
            float x2 = destination.X;
            float y2 = destination.Y;

            if (x1 < 0 || y1 < 0 || x1 >= this.MapWidth || y1 >= this.MapHeight)
            {
                return 0.0f;
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
            for (i = 0; i < il; i++)
            {
                //TODO: Implement bush logic (preferably near here)
                //TODO: Implement methods for NavGrids without SEE_THROUGH flags
                if (IsWalkable(x1, y1) == checkWalkable && IsSeeThrough(x1, y1) == checkWalkable)
                {
                    break;
                }

                // if checkWalkable == true, stop incrementing when (x1, x2) is a see-able position
                // if checkWalkable == false, stop incrementing when (x1, x2) is a non-see-able position
                x1 += dx;
                y1 += dy;
            }

            if (i == il || (x1 == origin.X && y1 == origin.Y))
            {
                return (new Vector2(x2, y2) - origin).SqrLength();
            }

            return (new Vector2(x1, y1) - origin).SqrLength();
        }
        public float CastInfiniteRaySqr(Vector2 origin, Vector2 direction, bool inverseRay = false)
        {
            return CastRaySqr(origin, origin + direction * 1024, inverseRay);
        }
        public float CastInfiniteRay(Vector2 origin, Vector2 direction, bool inverseRay = false)
        {
            return (float)Math.Sqrt(CastInfiniteRaySqr(origin, direction, inverseRay));
        }

        public bool IsAnythingBetween(Vector2 a, Vector2 b)
        {
            return CastRaySqr(a, b) < (b - a).SqrLength();
        }
        public bool IsAnythingBetween(IGameObject a, IGameObject b)
        {
            if (a is IObjBuilding)
            {
                double rayDist = Math.Sqrt(CastRaySqr(b.GetPosition(), a.GetPosition()));
                rayDist += a.CollisionRadius;
                return (rayDist * rayDist) < (b.GetPosition() - a.GetPosition()).SqrLength();
            }
            if (b is IObjBuilding)
            {
                double rayDist = Math.Sqrt(CastRaySqr(a.GetPosition(), b.GetPosition()));
                rayDist += b.CollisionRadius;
                return (rayDist * rayDist) < (b.GetPosition() - a.GetPosition()).SqrLength();
            }
            return CastRaySqr(a.GetPosition(), b.GetPosition()) < (b.GetPosition() - a.GetPosition()).SqrLength();
        }
        public bool IsAnythingBetween(NavigationGridLocator a, NavigationGridLocator b)
        {
            return IsAnythingBetween(new Vector2(a.X, a.Y), new Vector2(b.X, b.Y));
        }
        public bool IsAnythingBetween(NavigationGridCell a, NavigationGridCell b)
        {
            return IsAnythingBetween(a.Locator, b.Locator);
        }

        public Vector2 GetClosestTerrainExit(Vector2 location)
        {
            if (IsWalkable(location))
            {
                return location;
            }

            double trueX = location.X;
            double trueY = location.Y;
            double angle = Math.PI / 4;
            double rr = (location.X - trueX) * (location.X - trueX) + (location.Y - trueY) * (location.Y - trueY);
            double r = Math.Sqrt(rr);

            // x = r * cos(angle)
            // y = r * sin(angle)
            // r = distance from center
            // Draws spirals until it finds a walkable spot
            while (!IsWalkable((float)trueX, (float)trueY))
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