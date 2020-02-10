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
        public Vector3 MinGridPosition { get; set; }
        public Vector3 MaxGridPosition { get; set; }
        public Vector3 TranslationMaxGridPosition { get; set; }
        public float CellSize { get; private set; }
        public uint CellCountX { get; set; }
        public uint CellCountY { get; set; }
        public NavigationGridCell[] Cells { get; set; }
        public ushort[] CellFlags { get; set; } // Version 7 change
        public Vector2 SampledHeightDistance { get; private set; }
        public float OffsetX { get; private set; }
        public float OffsetZ { get; private set; }
        public float MapWidth { get; private set; }
        public float MapHeight { get; private set; }
        public Vector2 MiddleOfMap { get; private set; }
        public const float SCALE = 2f;

        public NavigationGrid(string fileLocation) : this(File.OpenRead(fileLocation)) { }
        public NavigationGrid(byte[] buffer) : this(new MemoryStream(buffer)) { }
        public NavigationGrid(Stream stream)
        {
            using (BinaryReader br = new BinaryReader(stream))
            {
                byte major = br.ReadByte();
                ushort minor = major != 2 ? br.ReadUInt16() : (ushort)0;

                this.MinGridPosition = br.ReadVector3();
                this.MaxGridPosition = br.ReadVector3();

                this.CellSize = br.ReadSingle();
                this.CellCountX = br.ReadUInt32();
                this.CellCountY = br.ReadUInt32();

                this.Cells = new NavigationGridCell[this.CellCountX * this.CellCountY];
                this.CellFlags = new ushort[this.CellCountX * this.CellCountY];

                if (major == 2 || major == 3 || major == 5)
                {
                    for (int i = 0; i < this.Cells.Length; i++)
                    {
                        this.Cells[i] = NavigationGridCell.ReadVersion5(br, i, out this.CellFlags[i]);
                    }

                    int sampledHeightCountX = br.ReadInt32();
                    int sampledHeightCountY = br.ReadInt32();

                    this.SampledHeightDistance = br.ReadVector2();
                }
                else if (major == 0x07)
                {
                    for (int i = 0; i < this.Cells.Length; i++)
                    {
                        this.Cells[i] = NavigationGridCell.ReadVersion7(br, i);
                    }

                    for (int i = 0; i < this.Cells.Length; i++)
                    {
                        this.CellFlags[i] = br.ReadUInt16();
                    }
                }
                else
                {
                    throw new Exception(string.Format("Unsupported Navigation Grid Version: {0}.{1}", major, minor));
                }

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
                    if (!priorityQueue.TryFirst(out path))
                    {
                        // no solution
                        return null;
                    }

                    float currentCost = priorityQueue.GetPriority(priorityQueue.First);
                    priorityQueue.TryDequeue(out path);

                    NavigationGridCell cell = path.Peek();

                    currentCost -= (NavigationGridCell.Distance(cell, goal) + cell.Heuristic); // decrease the heuristic to get the cost

                    // found the min solution return it (path)
                    if (cell.ID == goal.ID) break;

                    NavigationGridCell tempCell = null;
                    foreach (NavigationGridCell ncell in GetCellNeighbors(cell))
                    {
                        // if the neighbor in the closed list - skip
                        if (closedList.TryGetValue(ncell.ID, out tempCell)) continue;

                        // not walkable - skip
                        if (ncell.HasFlag(this, NavigationGridCellFlags.NOT_PASSABLE) || ncell.HasFlag(this, NavigationGridCellFlags.SEE_THROUGH))
                        {
                            closedList.Add(ncell.ID, ncell);
                            continue;
                        }

                        // calculate the new path and cost +heuristic and add to the priority queue
                        Stack<NavigationGridCell> npath = new Stack<NavigationGridCell>(new Stack<NavigationGridCell>(path));
                        
                        npath.Push(ncell);
                        // add 1 for every cell used
                        priorityQueue.Enqueue(npath, currentCost + 1 + ncell.Heuristic + ncell.ArrivalCost + ncell.AdditionalCost 
                            + NavigationGridCell.Distance(ncell, goal));
                        closedList.Add(ncell.ID, ncell);
                    }
                }

                NavigationGridCell[] pathArray = path.ToArray();
                Array.Reverse(pathArray);

                List<NavigationGridCell> pathList = SmoothPath(new List<NavigationGridCell>(pathArray));

                pathList.RemoveAt(0); // removes the first point

                foreach (NavigationGridCell navGridCell in pathList.ToArray())
                {
                    returnList.Add(TranslateFromNavGrid(new Vector2 { X = navGridCell.X, Y = navGridCell.Y }));
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
        public Vector2 TranslateFromNavGrid(Vector2 vector)
        {
            Vector2 ret = new Vector2
            {
                X = vector.X / this.TranslationMaxGridPosition.X + this.MinGridPosition.X,
                Y = vector.Y / this.TranslationMaxGridPosition.Z + this.MinGridPosition.Z
            };

            return ret;
        }

        public Vector2 Uncompress(Vector2 vector)
        {
            Vector2 ret = new Vector2
            {
                X = vector.X / this.MaxGridPosition.X + this.MinGridPosition.X,
                Y = vector.Y / this.MaxGridPosition.Z + this.MinGridPosition.Z
            };

            return ret;
        }

        public ushort GetFlag(ushort x, ushort y)
        {
            return this.CellFlags[x + y * this.CellCountX];
        }

        public void ToImage(string fileName)
        {
            int width = (int)this.CellCountX;
            int height = (int)this.CellCountY;
            byte[] pixels = new byte[this.Cells.Length * 4];

            int offset = 0;
            for (int i = 0; i < this.Cells.Length; i++)
            {
                if (HasFlag(i, NavigationGridCellFlags.NOT_PASSABLE))
                {
                    byte red = 0xFF;
                    byte green = 0x00;
                    byte blue = 0x00;

                    pixels[offset] = blue; // r
                    pixels[offset + 1] = green; // g
                    pixels[offset + 2] = red; // b
                    pixels[offset + 3] = 0xFF; // a
                }

                offset += 4;
            }


            byte[] header = new byte[]
            {
                0x00, // ID length
                0x00, // no color map
                0x02, // uncompressed, true color
                0x00, 0x00, 0x00, 0x00,
                0x00,
                0x00, 0x00, 0x00, 0x00, // x and y origin
                (byte)(width & 0x00FF),
                (byte)((width & 0xFF00) >> 8),
                (byte)(height & 0x00FF),
                (byte)((height & 0xFF00) >> 8),
                0x20, // 32 bit bitmap
                0x00
            };

            using (BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create)))
            {
                writer.Write(header);
                writer.Write(pixels);
            }
        }

        // TODO: seems like using binary search for simple index finding on a grid!!!
        // CHANGE THIS
        public Vector2 GetCellVector(short x, short y)
        {
            // Changed to binary search
            int width = (int)this.CellCountX;
            int height = (int)this.CellCountY;
            int cellSize = (int)this.CellSize;
            Vector2 index = new Vector2
            {
                X = width / 2,
                Y = height / 2
            };
            Vector2 step = new Vector2
            {
                X = width / 4,
                Y = height / 4
            };

            while (true)
            {
                int i = (int)(index.X + index.Y * width);

                if (this.Cells[i].X < x)
                {
                    index.X += step.X;
                    step.X = step.X / 2;
                }
                else if (this.Cells[i].X > x)
                {
                    index.X -= step.X;
                    step.X = step.X / 2;
                }

                if (this.Cells[i].Y < y)
                {
                    index.Y += step.Y;
                    step.Y = step.Y / 2;
                }
                else if (this.Cells[i].Y > y)
                {
                    index.Y -= step.Y;
                    step.Y = step.Y / 2;
                }

                if (Math.Abs(this.Cells[i].X - x) < cellSize && Math.Abs(this.Cells[i].Y - y) < cellSize)
                {
                    return index;
                }

                if (step.X == 0 && step.Y == 0)
                {
                    return new Vector2
                    {
                        X = -1,
                        Y = -1
                    };
                }
            }
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
                if (UncompressX(this.Cells[i].X) > x - this.CellSize && UncompressX(this.Cells[i].X) < x + this.CellSize &&
                    UncompressZ(this.Cells[i].Y) > z - this.CellSize && UncompressZ(this.Cells[i].Y) < z + this.CellSize)
                {
                    return i;
                }
            }

            return -1;
        }
        private List<NavigationGridCell> GetCellNeighbors(NavigationGridCell cell)
        {
            short x = cell.X;
            short y = cell.Y;
            List<NavigationGridCell> neighbors = new List<NavigationGridCell>();
            for (int dirY = -1; dirY <= 1; dirY++)
            {
                for (int dirX = -1; dirX <= 1; dirX++)
                {
                    int nx = x + dirX;
                    int ny = y + dirY;
                    NavigationGridCell ncell = GetCell(nx, ny);
                    if (ncell != null) neighbors.Add(ncell);
                }
            }
            return neighbors;
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
        public NavigationGridCell GetCell(int x, int y)
        {
            long index = y * this.CellCountX + x;
            if (x < 0 || x > this.CellCountX || y < 0 || y > this.CellCountY || index >= this.Cells.Length)
            {
                return null;
            }

            return this.Cells[index];
        }

        public ushort CompressX(float positionX)
        {
            if (Math.Abs((positionX - this.OffsetX) * (1 / SCALE)) >= ushort.MaxValue)
            {
                throw new Exception("Compressed position reached maximum value.");
            }

            int ret = Convert.ToInt32((positionX - this.OffsetX) * (1 / SCALE));

            return (ushort)ret;
        }
        public ushort CompressZ(float positionZ)
        {
            if (Math.Abs((positionZ - this.OffsetZ) * (1 / SCALE)) >= ushort.MaxValue)
            {
                throw new Exception("Compressed position reached maximum value.");
            }

            int ret = Convert.ToInt32((positionZ - this.OffsetZ) * (1 / SCALE));

            return (ushort)ret;
        }

        public float UncompressX(short shortX)
        {
            return Convert.ToSingle(shortX / (1 / SCALE) + this.OffsetX);
        }
        public float UncompressZ(short shortZ)
        {
            return Convert.ToSingle(shortZ / (1 / SCALE) + this.OffsetZ);
        }

        public Vector2 GetSize()
        {
            return new Vector2(this.MapWidth / 2, this.MapHeight / 2);
        }

        public bool HasFlag(int index, NavigationGridCellFlags flag)
        {
            return (this.CellFlags[index] & (int)flag) == (int)flag;
        }

        public bool IsWalkable(float x, float y)
        {
            return IsWalkable(new Vector2(x, y));
        }
        public bool IsWalkable(Vector2 coords)
        {
            Vector2 vector = TranslateToNavGrid(new Vector2 { X = coords.X, Y = coords.Y });
            NavigationGridCell cell = GetCell((short)vector.X, (short)vector.Y);

            return cell != null && !cell.HasFlag(this, NavigationGridCellFlags.NOT_PASSABLE);
        }

        public bool IsSeeThrough(float x, float y)
        {
            return IsSeeThrough(new Vector2(x, y));
        }
        public bool IsSeeThrough(Vector2 coords)
        {
            Vector2 vector = TranslateToNavGrid(new Vector2 { X = coords.X, Y = coords.Y });
            NavigationGridCell cell = GetCell((short)vector.X, (short)vector.Y);

            return cell != null && cell.HasFlag(this, NavigationGridCellFlags.SEE_THROUGH);
        }

        public bool IsBrush(float x, float y)
        {
            return IsBrush(new Vector2(x, y));
        }
        public bool IsBrush(Vector2 coords)
        {
            Vector2 vector = TranslateToNavGrid(new Vector2 { X = coords.X, Y = coords.Y });
            NavigationGridCell cell = GetCell((short)vector.X, (short)vector.Y);

            return cell != null && cell.HasFlag(this, NavigationGridCellFlags.HAS_GRASS);
        }

        public bool HasGlobalVision(Vector2 coords)
        {
            Vector2 vector = TranslateToNavGrid(coords);
            NavigationGridCell cell = GetCell((short)vector.X, (short)vector.Y);

            return cell != null && cell.HasFlag(this, NavigationGridCellFlags.HAS_GLOBAL_VISION);
        }

        public float GetHeightAtLocation(Vector2 coords)
        {
            Vector2 vector = TranslateToNavGrid(coords);
            NavigationGridCell cell = GetCell((short)vector.X, (short)vector.Y);
            if (cell != null)
            {
                return cell.CenterHeight;
            }

            return float.MinValue;
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
        public bool IsAnythingBetween(NavigationGridCell origin, NavigationGridCell destination)
        {
            return IsAnythingBetween(new Vector2(origin.X, origin.Y), new Vector2(destination.X, destination.Y));
        }

        public Vector2 GetClosestTerrainExit(Vector2 location)
        {
            if (IsWalkable(location))
            {
                return location;
            }

            double trueX = (double)location.X;
            double trueY = (double)location.Y;
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