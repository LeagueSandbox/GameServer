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

namespace LeagueSandbox.GameServer.Content
{
    //TODO: use constructor and remove public setters
    public class NavGrid : INavGrid
    {
        public byte MajorVersion { get; set; }
        public short MinorVersion { get; set; }
        public Vector<float> MinGridPos { get; set; }
        public Vector<float> MaxGridPos { get; set; }
        public Vector<float> TranslationMaxGridPos { get; set; }
        public float CellSize { get; set; }
        public uint XCellCount { get; set; }
        public uint YCellCount { get; set; }
        public NavGridCell[] Cells { get; set; } // XCellCount * YCellCount cells
        public ushort[] CellFlags { get; set; } // Version 7 change
        public int XSampledHeightCount { get; set; }
        public int YSampledHeightCount { get; set; }
        public float DirectionX { get; set; }
        public float DirectionY { get; set; }
        public float OffsetX { get; set; }
        public float OffsetZ { get; set; }
        public float MapWidth { get; set; }
        public float MapHeight { get; set; }
        public Vector2 MiddleOfMap { get; set; }
        public const float SCALE = 2f;

        public List<Vector2> GetPath(Vector2 from, Vector2 to)
        {
            List<Vector2> returnList = new List<Vector2>() { from };
            var vectorFrom = TranslateToNavGrid(new Vector<float> { X = from.X, Y = from.Y });
            var cellFrom = GetCell((short)vectorFrom.X, (short)vectorFrom.Y);

            var vectorTo = TranslateToNavGrid(new Vector<float> { X = to.X, Y = to.Y });
            var goal = GetCell((short)vectorTo.X, (short)vectorTo.Y);

            if (cellFrom != null && goal != null)
            {
                SimplePriorityQueue<Stack<NavGridCell>> priorityQueue = new SimplePriorityQueue<Stack<NavGridCell>>();
                var start = new Stack<NavGridCell>();
                start.Push(cellFrom);
                priorityQueue.Enqueue(start, CellDistance(cellFrom, goal));

                Dictionary<int, NavGridCell> closedList = new Dictionary<int, NavGridCell>();
                closedList.Add(cellFrom.Id, cellFrom);

                Stack<NavGridCell> path = null;

                // while there are still paths to explore
                while (true)
                {
                    if (!priorityQueue.TryFirst(out path))
                    {
                        // no solution
                        path = null;
                        break;
                    }
                    var curCost = priorityQueue.GetPriority(priorityQueue.First);
                    priorityQueue.TryDequeue(out path);
                    var cell = path.Peek();
                    curCost -= (CellDistance(cell, goal) + cell.Heuristic); // decrease the heuristic to get the cost
                    // found the min solution return it (path)
                    if (cell.Id == goal.Id) break;
                    NavGridCell tempCell = null;
                    foreach (NavGridCell ncell in GetCellNeighbors(cell))
                    {
                        // if the neighbor in the closed list - skip
                        if (closedList.TryGetValue(ncell.Id, out tempCell)) continue;
                        // not walkable - skip
                        if (ncell.HasFlag(this, NavigationGridCellFlags.NOT_PASSABLE) || ncell.HasFlag(this, NavigationGridCellFlags.SEE_THROUGH))
                        {
                            closedList.Add(ncell.Id, ncell);
                            continue;
                        }
                        // calculate the new path and cost +heuristic and add to the priority queue
                        var npath = new Stack<NavGridCell>(new Stack<NavGridCell>(path));
                        npath.Push(ncell);
                        // add 1 for every cell used
                        priorityQueue.Enqueue(npath, curCost + 1 + ncell.Heuristic + ncell.ArrivalCost + ncell.AdditionalCost + CellDistance(ncell, goal));
                        closedList.Add(ncell.Id, ncell);
                    }
                }
                // shouldn't happen usually
                if (path == null) return null;
                var arr = path.ToArray();
                Array.Reverse(arr);
                var pathList = SmoothPath(new List<NavGridCell>(arr));
                pathList.RemoveAt(0); // removes the first point
                foreach (var navGridCell in pathList.ToArray())
                {
                    var cellPosition = TranslateFromNavGrid(new Vector<float>() { X = navGridCell.X, Y = navGridCell.Y });
                    returnList.Add(new Vector2(cellPosition.X, cellPosition.Y));
                }
                return returnList;
            }
            return null;
        }
        private int CellDistance(NavGridCell cell1, NavGridCell cell2)
        {
            return (Math.Abs(cell1.X - cell2.X) + Math.Abs(cell1.Y - cell2.Y));
        }

        /// <summary>
        /// Remove waypoints that have LOS from the waypoint before the waypoint after
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private List<NavGridCell> SmoothPath(List<NavGridCell> path)
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

        public void CreateTranslation()
        {
            if (TranslationMaxGridPos == null)
            {
                TranslationMaxGridPos = new Vector<float>
                {
                    X = XCellCount / (MaxGridPos.X - MinGridPos.X),
                    Z = YCellCount / (MaxGridPos.Z - MinGridPos.Z)
                };
            }
        }

        public Vector<float> TranslateToNavGrid(Vector<float> vector)
        {
            CreateTranslation();
            vector.ForceSize(2);

            vector.X = (vector.X - MinGridPos.X) * TranslationMaxGridPos.X;
            vector.Y = (vector.Y - MinGridPos.Z) * TranslationMaxGridPos.Z;
            return vector;
        }

        public Vector<float> TranslateFromNavGrid(Vector<float> vector)
        {
            CreateTranslation();
            vector.ForceSize(2);

            var ret = new Vector<float>
            {
                X = vector.X / TranslationMaxGridPos.X + MinGridPos.X,
                Y = vector.Y / TranslationMaxGridPos.Z + MinGridPos.Z
            };

            return ret;
        }

        public Vector<float> Uncompress(Vector<float> vector)
        {
            vector.ForceSize(2);

            var ret = new Vector<float>
            {
                X = vector.X / MaxGridPos.X + MinGridPos.X,
                Y = vector.Y / MaxGridPos.Z + MinGridPos.Z
            };

            return ret;
        }

        public ushort GetFlag(ushort x, ushort y)
        {
            return CellFlags[x + y * XCellCount];
        }

        public void ToImage(string fileName)
        {
            var width = (int)XCellCount;
            var height = (int)YCellCount;
            var pixels = new byte[Cells.Length * 4];

            var offset = 0;
            for (var i = 0; i < Cells.Length; i++)
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


            var header = new byte[]
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

            using (var writer = new BinaryWriter(File.Open(fileName, FileMode.Create)))
            {
                writer.Write(header);
                writer.Write(pixels);
            }
        }

        public Vector<int> GetCellVector(short x, short y)
        {
            // Changed to binary search
            var width = (int)XCellCount;
            var height = (int)YCellCount;
            var cellSize = (int)CellSize;
            var index = new Vector<int>
            {
                X = width / 2,
                Y = height / 2
            };
            var step = new Vector<int>
            {
                X = width / 4,
                Y = height / 4
            };

            while (true)
            {
                var i = index.X + index.Y * width;

                if (Cells[i].X < x)
                {
                    index.X += step.X;
                    step.X = step.X / 2;
                }
                else if (Cells[i].X > x)
                {
                    index.X -= step.X;
                    step.X = step.X / 2;
                }

                if (Cells[i].Y < y)
                {
                    index.Y += step.Y;
                    step.Y = step.Y / 2;
                }
                else if (Cells[i].Y > y)
                {
                    index.Y -= step.Y;
                    step.Y = step.Y / 2;
                }

                if (Math.Abs(Cells[i].X - x) < cellSize && Math.Abs(Cells[i].Y - y) < cellSize)
                {
                    return index;
                }

                if (step.X == 0 && step.Y == 0)
                {
                    return new Vector<int>
                    {
                        X = -1,
                        Y = -1
                    };
                }
            }
        }

        public Vector<ushort> GetCellVectorShort(short x, short y)
        {
            var index = GetCellVector(x, y);
            return new Vector<ushort>
            {
                X = (ushort)index.X,
                Y = (ushort)index.Y
            };
        }

        public int GetCellIndex(short x, short y)
        {
            var vector = GetCellVector(x, y);

            if (vector.X > 0)
            {
                return (int)(vector.X + vector.Y * XCellCount);
            }

            return -1;
        }

        public int GetCellIndex(float x, float z)
        {
            for (var i = 0; i < Cells.Length; i++)
            {
                if (UncompressX(Cells[i].X) > x - CellSize && UncompressX(Cells[i].X) < x + CellSize &&
                    UncompressZ(Cells[i].Y) > z - CellSize && UncompressZ(Cells[i].Y) < z + CellSize)
                {
                    return i;
                }
            }

            return -1;
        }
        private List<NavGridCell> GetCellNeighbors(NavGridCell cell)
        {
            var x = cell.X;
            var y = cell.Y;
            List<NavGridCell> neighbors = new List<NavGridCell>();
            for (int dirY = -1; dirY <= 1; dirY++)
            {
                for (int dirX = -1; dirX <= 1; dirX++)
                {
                    var nx = x + dirX;
                    var ny = y + dirY;
                    var ncell = GetCell(nx, ny);
                    if (ncell != null) neighbors.Add(ncell);
                }
            }
            return neighbors;
        }

        public NavGridCell GetCell(short x, short y)
        {
            var index = y * XCellCount + x;
            if (x < 0 || x > XCellCount || y < 0 || y > YCellCount || index >= Cells.Length)
            {
                return null;
            }

            return Cells[index];
        }

        public NavGridCell GetCell(int x, int y)
        {
            var index = y * XCellCount + x;
            if (x < 0 || x > XCellCount || y < 0 || y > YCellCount || index >= Cells.Length)
            {
                return null;
            }

            return Cells[index];
        }

        public bool HasFlag(int index, NavigationGridCellFlags flag)
        {
            return (CellFlags[index] & (int)flag) == (int)flag;
        }

        public ushort CompressX(float positionX)
        {
            if (Math.Abs((positionX - OffsetX) * (1 / SCALE)) >= ushort.MaxValue)
            {
                throw new Exception("Compressed position reached maximum value.");
            }

            var ret = Convert.ToInt32((positionX - OffsetX) * (1 / SCALE));

            return (ushort)ret;
        }

        public ushort CompressZ(float positionZ)
        {
            if (Math.Abs((positionZ - OffsetZ) * (1 / SCALE)) >= ushort.MaxValue)
            {
                throw new Exception("Compressed position reached maximum value.");
            }

            var ret = Convert.ToInt32((positionZ - OffsetZ) * (1 / SCALE));

            return (ushort)ret;
        }

        public float UncompressX(short shortX)
        {
            return Convert.ToSingle(shortX / (1 / SCALE) + OffsetX);
        }

        public float UncompressZ(short shortZ)
        {
            return Convert.ToSingle(shortZ / (1 / SCALE) + OffsetZ);
        }

        public Vector2 GetSize()
        {
            return new Vector2(MapWidth / 2, MapHeight / 2);
        }

        public bool IsWalkable(Vector2 coords)
        {
            var vector = TranslateToNavGrid(new Vector<float> { X = coords.X, Y = coords.Y });
            var cell = GetCell((short)vector.X, (short)vector.Y);

            return cell != null && !cell.HasFlag(this, NavigationGridCellFlags.NOT_PASSABLE);
        }

        public bool IsSeeThrough(Vector2 coords)
        {
            var vector = TranslateToNavGrid(new Vector<float> { X = coords.X, Y = coords.Y });
            var cell = GetCell((short)vector.X, (short)vector.Y);

            return cell != null && cell.HasFlag(this, NavigationGridCellFlags.SEE_THROUGH);
        }

        public bool IsBrush(Vector2 coords)
        {
            var vector = TranslateToNavGrid(new Vector<float> { X = coords.X, Y = coords.Y });
            var cell = GetCell((short)vector.X, (short)vector.Y);

            return cell != null && cell.HasFlag(this, NavigationGridCellFlags.HAS_GRASS);
        }

        public bool IsWalkable(float x, float y)
        {
            return IsWalkable(new Vector2(x, y));
        }

        public bool IsSeeThrough(float x, float y)
        {
            return IsSeeThrough(new Vector2(x, y));
        }

        public bool IsBrush(float x, float y)
        {
            return IsBrush(new Vector2(x, y));
        }

        public bool HasGlobalVision(Vector2 coords)
        {
            var vector = TranslateToNavGrid(new Vector<float> { X = coords.X, Y = coords.Y });
            var cell = GetCell((short)vector.X, (short)vector.Y);

            return cell != null && cell.HasFlag(this, NavigationGridCellFlags.HAS_GLOBAL_VISION);
        }

        public float GetHeightAtLocation(Vector2 coords)
        {
            var vector = TranslateToNavGrid(new Vector<float> { X = coords.X, Y = coords.Y });
            var cell = GetCell((short)vector.X, (short)vector.Y);
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

        public bool IsAnythingBetween(NavGridCell origin, NavGridCell destination)
        {
            return IsAnythingBetween(new Vector2(origin.X, origin.Y), new Vector2(destination.X, destination.Y));
        }

        public float CastRaySqr(Vector2 origin, Vector2 destination, bool checkWalkable = false)
        {
            var x1 = origin.X;
            var y1 = origin.Y;
            var x2 = destination.X;
            var y2 = destination.Y;

            if (x1 < 0 || y1 < 0 || x1 >= MapWidth || y1 >= MapHeight)
            {
                return 0.0f;
            }

            var distx = x2 - x1;
            var disty = y2 - y1;
            var greatestdist = Math.Abs(distx);
            if (Math.Abs(disty) > greatestdist)
            {
                greatestdist = Math.Abs(disty);
            }

            var il = (int)greatestdist;
            var dx = distx / greatestdist;
            var dy = disty / greatestdist;
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
            return CastRaySqr(a.GetPosition(), b.GetPosition()) < (b.GetPosition() - a.GetPosition()).SqrLength();
        }

        public Vector2 GetClosestTerrainExit(Vector2 location)
        {
            if (IsWalkable(location))
            {
                return location;
            }

            var trueX = (double)location.X;
            var trueY = (double)location.Y;
            var angle = Math.PI / 4;
            var rr = (location.X - trueX) * (location.X - trueX) + (location.Y - trueY) * (location.Y - trueY);
            var r = Math.Sqrt(rr);

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

    public class NavGridCell
    {
        public int Id;
        public float CenterHeight;
        public uint SessionId;
        public float ArrivalCost;
        public bool IsOpen;
        public float Heuristic;
        public uint ActorList;
        public short X;
        public short Y;
        public float AdditionalCost;
        public float HintAsGoodCell;
        public uint AdditionalCostRefCount;
        public uint GoodCellSessionId;
        public float RefHintWeight;
        public short ArrivalDirection;
        public short[] RefHintNode;

        public bool HasFlag(NavGrid grid, NavigationGridCellFlags flag)
        {
            return grid.HasFlag(Id, flag);
        }
    }

    public enum NavigationGridCellFlags : short
    {
        HAS_GRASS = 0x1,
        NOT_PASSABLE = 0x2,
        BUSY = 0x4,
        TARGETTED = 0x8,
        MARKED = 0x10,
        PATHED_ON = 0x20,
        SEE_THROUGH = 0x40,
        OTHER_DIRECTION_END_TO_START = 0x80,
        HAS_GLOBAL_VISION = 0x100,
        // HAS_TRANSPARENT_TERRAIN = 0x42 // (SeeThrough | NotPassable)
    }

    internal class NavBinaryReader
    {
        private BinaryReader _reader;

        public NavBinaryReader(Stream stream)
        {
            _reader = new BinaryReader(stream);
        }

        public BinaryReader GetBinaryReader()
        {
            return _reader;
        }

        public long GetReaderPosition()
        {
            return _reader.BaseStream.Position;
        }

        public Vector<float> ReadVector2()
        {
            var vector = new Vector<float>
            {
                X = _reader.ReadSingle(),
                Y = _reader.ReadSingle()
            };

            return vector;
        }

        public Vector<float> ReadVector3()
        {
            var vector = new Vector<float>
            {
                X = _reader.ReadSingle(),
                Y = _reader.ReadSingle(),
                Z = _reader.ReadSingle()
            };

            return vector;
        }

        public NavGridCell ReadGridCell_Version5(out ushort flag)
        {
            var result = new NavGridCell
            {
                CenterHeight = _reader.ReadSingle(),
                SessionId = _reader.ReadUInt32(),
                ArrivalCost = _reader.ReadSingle(),
                IsOpen = _reader.ReadUInt32() == 1,
                Heuristic = _reader.ReadSingle(),
                ActorList = _reader.ReadUInt32(),
                X = _reader.ReadInt16(),
                Y = _reader.ReadInt16(),
                AdditionalCost = _reader.ReadSingle(),
                HintAsGoodCell = _reader.ReadSingle(),
                AdditionalCostRefCount = _reader.ReadUInt32(),
                GoodCellSessionId = _reader.ReadUInt32(),
                RefHintWeight = _reader.ReadSingle(),
                ArrivalDirection = _reader.ReadInt16()
            };

            flag = _reader.ReadUInt16();
            result.RefHintNode = new short[2];
            result.RefHintNode[0] = _reader.ReadInt16();
            result.RefHintNode[1] = _reader.ReadInt16();

            return result;
        }

        public NavGridCell ReadGridCell_Version7()
        {
            var result = new NavGridCell
            {
                CenterHeight = _reader.ReadSingle(),
                SessionId = _reader.ReadUInt32(),
                ArrivalCost = _reader.ReadSingle(),
                IsOpen = _reader.ReadUInt32() == 1,
                Heuristic = _reader.ReadSingle(),
                X = _reader.ReadInt16(),
                Y = _reader.ReadInt16(),
                ActorList = _reader.ReadUInt32()
            };

            _reader.ReadUInt32(); // <- "Unk1"
            result.GoodCellSessionId = _reader.ReadUInt32();
            result.RefHintWeight = _reader.ReadSingle();
            _reader.ReadUInt16();   // <- "Unk2"
            result.ArrivalDirection = _reader.ReadInt16();
            result.RefHintNode = new short[2];
            result.RefHintNode[0] = _reader.ReadInt16();
            result.RefHintNode[1] = _reader.ReadInt16();

            return result;
        }
    }

    public class NavGridReader
    {
        public static NavGrid ReadBinary(string filePath)
        {
            NavBinaryReader b = null;

            try
            {
                b = new NavBinaryReader(File.Open(filePath, FileMode.Open, FileAccess.Read));
            }
            catch
            {
                // Ignored
            }

            return ReadData(b);
        }

        public static NavGrid ReadBinary(byte[] fileBytes)
        {
            NavBinaryReader b = null;

            try
            {
                b = new NavBinaryReader(new MemoryStream(fileBytes));
            }
            catch
            {
                // Ignored
            }

            return ReadData(b);
        }

        private static NavGrid ReadData(NavBinaryReader b)
        {
            var grid = new NavGrid
            {
                MajorVersion = b.GetBinaryReader().ReadByte()
            };

            if (grid.MajorVersion != 2)
            {
                grid.MinorVersion = b.GetBinaryReader().ReadInt16();
            }

            grid = ReadStandardNavGridHeader(b, grid);
            grid.Cells = new NavGridCell[grid.XCellCount * grid.YCellCount];
            grid.CellFlags = new ushort[grid.XCellCount * grid.YCellCount];

            if (grid.MajorVersion == 0x02 || grid.MajorVersion == 0x03 || grid.MajorVersion == 0x05)
            {
                // Read cells, total size: 0x38 * XCellCount * YCellCount bytes

                for (var i = 0; i < grid.Cells.Length; i++)
                {
                    grid.Cells[i] = b.ReadGridCell_Version5(out grid.CellFlags[i]);
                    grid.Cells[i].Id = i;
                }

                grid.XSampledHeightCount = b.GetBinaryReader().ReadInt32();
                grid.YSampledHeightCount = b.GetBinaryReader().ReadInt32();

                //should be mXSampledHeightDist
                grid.DirectionX = b.GetBinaryReader().ReadSingle();
                //should be mYSampledHeightDist
                grid.DirectionY = b.GetBinaryReader().ReadSingle();
            }
            else if (grid.MajorVersion == 0x07)
            {
                // Read cells, total size: 0x30 * XCellCount * YCellCount bytes

                for (var i = 0; i < grid.Cells.Length; i++)
                {
                    grid.Cells[i] = b.ReadGridCell_Version7();
                    grid.Cells[i].Id = i;
                }

                for (var i = 0; i < grid.Cells.Length; i++)
                {
                    grid.CellFlags[i] = b.GetBinaryReader().ReadUInt16();
                }
            }
            else
            {
                throw new Exception($"Magic number at the start is unsupported! Value: {grid.MajorVersion:X}");
            }

            grid.MapWidth = grid.MaxGridPos.X + grid.MinGridPos.X;
            grid.MapHeight = grid.MaxGridPos.Z + grid.MinGridPos.Z;
            grid.MiddleOfMap = new Vector2(grid.MapWidth / 2, grid.MapHeight / 2);
            return grid;
        }

        private static NavGrid ReadStandardNavGridHeader(NavBinaryReader b, NavGrid grid)
        {
            grid.MinGridPos = b.ReadVector3();
            grid.MaxGridPos = b.ReadVector3();

            grid.CellSize = b.GetBinaryReader().ReadSingle();
            grid.XCellCount = b.GetBinaryReader().ReadUInt32();
            grid.YCellCount = b.GetBinaryReader().ReadUInt32();

            return grid;
        }
    }
}