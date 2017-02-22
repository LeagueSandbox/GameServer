using System;
using System.IO;
using System.Linq;
using System.Numerics;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.GameObjects;

namespace LeagueSandbox.GameServer.Logic.RAF
{
    public class NavGrid
    {
        public byte MajorVersion;
        public short MinorVersion;
        public Vector<float> MinGridPos;
        public Vector<float> MaxGridPos;
        public Vector<float> TranslationMaxGridPos;
        public float CellSize;
        public uint XCellCount;
        public uint YCellCount;
        public NavGridCell[] Cells; // XCellCount * YCellCount cells
        public ushort[] CellFlags; // Version 7 change
        public int XSampledHeightCount;
        public int YSampledHeightCount;
        public float DirectionX;
        public float DirectionY;
        public float OffsetX;
        public float OffsetZ;
        public float MapWidth;
        public float MapHeight;
        public const float Scale = 2f;

        public void CreateTranslation()
        {
            if (TranslationMaxGridPos == null)
            {
                TranslationMaxGridPos = new Vector<float>
                {
                    X = ((float)XCellCount / MaxGridPos.X),
                    Z = ((float)YCellCount / MaxGridPos.Z)
                };
            }
        }

        public Vector<float> TranslateToNavGrid(Vector<float> a_Vector)
        {
            CreateTranslation();
            a_Vector.ForceSize(2);

            a_Vector.X = (a_Vector.X - MinGridPos.X) * TranslationMaxGridPos.X;
            a_Vector.Y = (a_Vector.Y - MinGridPos.Z) * TranslationMaxGridPos.Z;
            return a_Vector;
        }

        public Vector<float> TranslateFromNavGrid(Vector<float> a_Vector)
        {
            CreateTranslation();
            a_Vector.ForceSize(2);

            var t_Vector = new Vector<float>
            {
                X = ((float) a_Vector.X / TranslationMaxGridPos.X) + MinGridPos.X,
                Y = ((float) a_Vector.Y / TranslationMaxGridPos.Z) + MinGridPos.Z
            };

            return t_Vector;
        }

        public Vector<float> Uncompress(Vector<float> a_Vector)
        {
            a_Vector.ForceSize(2);

            var t_Vector = new Vector<float>
            {
                X = ((float) a_Vector.X / MaxGridPos.X) + MinGridPos.X,
                Y = ((float) a_Vector.Y / MaxGridPos.Z) + MinGridPos.Z
            };


            return t_Vector;
        }

        public ushort GetFlag(ushort a_X, ushort a_Y)
        {
            return CellFlags[a_X + a_Y * XCellCount];
        }

        public void ToImage(string a_Filename)
        {
            var t_Width = (int)XCellCount;
            var t_Height = (int)YCellCount;
            var t_Pixels = new byte[Cells.Length * 4];

            var t_Offset = 0;
            for (var i = 0; i < Cells.Length; i++)
            {
                if (HasFlag(i, NavigationGridCellFlags.CELL_NOT_PASSABLE))
                {
                    byte t_Red = 0xFF;
                    byte t_Green = 0x0;
                    byte t_Blue = 0x0;

                    t_Pixels[t_Offset] = t_Blue; // r
                    t_Pixels[t_Offset + 1] = t_Green; // g
                    t_Pixels[t_Offset + 2] = t_Red; // b
                    t_Pixels[t_Offset + 3] = 255; // a
                }

                t_Offset += 4;
            }


            var t_Header = new byte[]
            {
                0, // ID length
                0, // no color map
                2, // uncompressed, true color
                0, 0, 0, 0,
                0,
                0, 0, 0, 0, // x and y origin
                (byte)(t_Width & 0x00FF),
                (byte)((t_Width & 0xFF00) >> 8),
                (byte)(t_Height & 0x00FF),
                (byte)((t_Height & 0xFF00) >> 8),
                32, // 32 bit bitmap
                0
            };

            using (var t_Writer = new BinaryWriter(File.Open(a_Filename, FileMode.Create)))
            {
                t_Writer.Write(t_Header);
                t_Writer.Write(t_Pixels);
            }
        }

        public Vector<int> GetCellVector(short a_X, short a_Y)
        {
            // Changed to binary search
            var t_Width = (int)XCellCount;
            var t_Height = (int)YCellCount;
            var t_CellSize = (int)CellSize;
            var t_Index = new Vector<int>
            {
                X = t_Width / 2,
                Y = t_Height / 2
            };
            var t_Step = new Vector<int>
            {
                X = t_Width / 4,
                Y = t_Height / 4
            };

            while (true)
            {
                var t_I = t_Index.X + t_Index.Y * t_Width;

                if (Cells[t_I].mX < a_X)
                {
                    t_Index.X += t_Step.X;
                    t_Step.X = t_Step.X / 2;
                }
                else if (Cells[t_I].mX > a_X)
                {
                    t_Index.X -= t_Step.X;
                    t_Step.X = t_Step.X / 2;
                }

                if (Cells[t_I].mY < a_Y)
                {
                    t_Index.Y += t_Step.Y;
                    t_Step.Y = t_Step.Y / 2;
                }
                else if (Cells[t_I].mY > a_Y)
                {
                    t_Index.Y -= t_Step.Y;
                    t_Step.Y = t_Step.Y / 2;
                }

                if (Math.Abs(Cells[t_I].mX - a_X) < t_CellSize && Math.Abs(Cells[t_I].mY - a_Y) < t_CellSize)
                    return t_Index;
                else if (t_Step.X == 0 && t_Step.Y == 0)
                    return new Vector<int> //(-1, -1);
                    {
                        X = -1, Y = -1
                    };
            }
        }

        public Vector<ushort> GetCellVectorShort(short a_X, short a_Y)
        {
            var t_Index = GetCellVector(a_X, a_Y);
            return new Vector<ushort>
            {
                X = (ushort)t_Index.X,
                Y = (ushort)t_Index.Y
            };
        }

        public int GetCellIndex(short a_X, short a_Y)
        {
            var t_Vector = GetCellVector(a_X, a_Y);

            if (t_Vector.X > 0)
                return (int)(t_Vector.X + t_Vector.Y * XCellCount);
            else
                return -1;
        }

        public int GetCellIndex(float a_X, float a_Z)
        {
            for (var i = 0; i < Cells.Length; i++)
                if (UncompressX(Cells[i].mX) > a_X - CellSize && UncompressX(Cells[i].mX) < a_X + CellSize &&
                    UncompressZ(Cells[i].mY) > a_Z - CellSize && UncompressZ(Cells[i].mY) < a_Z + CellSize)
                    return i;

            return -1;
        }

        public NavGridCell GetCell(short a_X, short a_Y)
        {
            if (a_X < 0 || a_Y < 0 || a_X > XCellCount || a_Y > YCellCount)
            {
                return null;
            }

            var cell = Cells.FirstOrDefault(x => x.mX == a_X && x.mY == a_Y);
            return cell;
        }

        public bool HasFlag(int a_Index, NavigationGridCellFlags flag)
        {
            return (((int)CellFlags[a_Index] & (int)flag) == (int)flag);
        }

        public ushort CompressX(float PositionX)
        {
            if (Math.Abs((PositionX - OffsetX) * (1 / Scale)) >= ushort.MaxValue)
                throw new Exception("Compressed position reached 0x7fff");

            var ret = Convert.ToInt32((PositionX - OffsetX) * (1 / Scale));

            return (ushort)ret;
        }

        public ushort CompressZ(float PositionZ)
        {
            if (Math.Abs((PositionZ - OffsetZ) * (1 / Scale)) >= ushort.MaxValue)
                throw new Exception("Compressed position reached 0x7fff");

            var ret = Convert.ToInt32((PositionZ - OffsetZ) * (1 / Scale));

            return (ushort)ret;
        }

        public float UncompressX(short ShortX)
        {
            return Convert.ToSingle((ShortX / (1 / Scale)) + OffsetX);
        }

        public float UncompressZ(short ShortZ)
        {
            return Convert.ToSingle((ShortZ / (1 / Scale)) + OffsetZ);
        }

        public bool IsWalkable(Vector2 coords)
        {
            var vectorBullshit = TranslateToNavGrid(new Vector<float> {X = coords.X, Y = coords.Y});
            var cell = GetCell((short)vectorBullshit.X, (short)vectorBullshit.Y);
            return cell != null && !cell.HasFlag(this, NavigationGridCellFlags.CELL_NOT_PASSABLE);
        }

        public bool IsWalkable(float x, float y)
        {
            return IsWalkable(new Vector2(x, y));
        }

        public bool IsBrush(Vector2 coords)
        {
            var vectorBullshit = TranslateToNavGrid(new Vector<float> { X = coords.X, Y = coords.Y });
            var cell = GetCell((short)vectorBullshit.X, (short)vectorBullshit.Y);
            return cell != null && cell.HasFlag(this, NavigationGridCellFlags.CELL_HAS_GRASS);
        }

        public bool HasGlobalVision(Vector2 coords)
        {
            var vectorBullshit = TranslateToNavGrid(new Vector<float> { X = coords.X, Y = coords.Y });
            var cell = GetCell((short)vectorBullshit.X, (short)vectorBullshit.Y);
            return cell != null && cell.HasFlag(this, NavigationGridCellFlags.CELL_HAS_ANTI_BRUSH);
        }

        public float GetHeightAtLocation(Vector2 coords)
        {
            var vectorBullshit = TranslateToNavGrid(new Vector<float> { X = coords.X, Y = coords.Y });
            var cell = GetCell((short)vectorBullshit.X, (short)vectorBullshit.Y);
            if (cell != null)
            {
                return cell.CenterHeight;
            }
            return float.MinValue;
        }

        public float CastRay(Vector2 origin, Vector2 destination, bool inverseRay = false)
        {
            return (float)Math.Sqrt(CastRaySqr(origin, destination, inverseRay));
        }

        public float CastRaySqr(Vector2 origin, Vector2 destination, bool inverseRay = false)
        {
            var x1 = origin.X;
            var y1 = origin.Y;
            var x2 = destination.X;
            var y2 = destination.Y;

            if (x1 < 0 || y1 < 0 /*|| x1 >= MapWidth || y1 >= MapHeight*/)
            {
                return 0.0f;
            }

            var b = x2 - x1;
            var h = y2 - y1;
            var l = Math.Abs(b);
            if (Math.Abs(h) > l)
            {
                l = Math.Abs(h);
            }
            var il = (int)l;
            var dx = b / l;
            var dy = h / l;
            int i;
            for (i = 0; i <= il; i++)
            {
                if (IsWalkable(x1, y1) == inverseRay)
                {
                    break;
                }
                // Inverse = report on walkable
                // Normal = report on terrain
                // so break when isWalkable == true and inverse == true
                // Break when isWalkable == false and inverse == false
                x1 += dx;
                y1 += dy;
            }

            if (i == il)
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
            return CastRaySqr(a, b) <= (b - a).SqrLength();
        }
        public bool IsAnythingBetween(GameObject a, GameObject b)
        {
            return CastRaySqr(a.GetPosition(), b.GetPosition()) <= (b.GetPosition() - a.GetPosition()).SqrLength();
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
            // Draws spirals until it finds a way out
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
        public int ID;
        public float CenterHeight;
        public uint SessionID;
        public float ArrivalCost;
        public bool IsOpen;
        public float Heuristic;
        public uint ActorList;
        public short mX;
        public short mY;
        public float AdditionalCost;
        public float HintAsGoodCell;
        public uint AdditionalCostRefCount;
        public uint GoodCellSessionID;
        public float RefHintWeight;
        public short ArrivalDirection;
        public short[] RefHintNode;

        public bool HasFlag(NavGrid a_Grid, NavigationGridCellFlags a_Flag)
        {
            return a_Grid.HasFlag(ID, a_Flag);
        }
    }

    public enum NavigationGridCellFlags : short
    {
        CELL_HAS_GRASS = 0x1,
        CELL_NOT_PASSABLE = 0x2,
        CELL_BUSY = 0x4,
        CELL_TARGETTED = 0x8,
        CELL_MARKED = 0x10,
        CELL_PATHED_ON = 0x20,
        CELL_SEE_THROUGH = 0x40,
        CELL_OTHERDIRECTION_END_TO_START = 0x80,
        CELL_HAS_ANTI_BRUSH = 0x100,
        CELL_HAS_TRANSPARENTTERRAIN = 0x42,
    }

    class NavBinaryReader
    {
        private BinaryReader reader;

        public NavBinaryReader(Stream stream)
        {
            reader = new BinaryReader(stream);
        }

        public BinaryReader GetBinaryReader()
        {
            return reader;
        }

        public long GetReaderPosition()
        {
            return reader.BaseStream.Position;
        }

        public Vector<float> ReadVector2()
        {
            var vector = new Vector<float>
            {
                X = reader.ReadSingle(),
                Y = reader.ReadSingle()
            };


            return vector;
        }

        public Vector<float> ReadVector3()
        {
            var vector = new Vector<float>
            {
                X = reader.ReadSingle(),
                Y = reader.ReadSingle(),
                Z = reader.ReadSingle()
            };


            return vector;
        }

        public NavGridCell ReadGridCell_Version5(out ushort a_Flags)
        {
            var result = new NavGridCell
            {
                CenterHeight = reader.ReadSingle(),
                SessionID = reader.ReadUInt32(),
                ArrivalCost = reader.ReadSingle(),
                IsOpen = reader.ReadUInt32() == 1,
                Heuristic = reader.ReadSingle(),
                ActorList = reader.ReadUInt32(),
                mX = reader.ReadInt16(),
                mY = reader.ReadInt16(),
                AdditionalCost = reader.ReadSingle(),
                HintAsGoodCell = reader.ReadSingle(),
                AdditionalCostRefCount = reader.ReadUInt32(),
                GoodCellSessionID = reader.ReadUInt32(),
                RefHintWeight = reader.ReadSingle(),
                ArrivalDirection = reader.ReadInt16()
            };

            a_Flags = reader.ReadUInt16();
            result.RefHintNode = new short[2];
            result.RefHintNode[0] = reader.ReadInt16();
            result.RefHintNode[1] = reader.ReadInt16();

            return result;
        }

        public NavGridCell ReadGridCell_Version7()
        {
            var result = new NavGridCell();

            result.CenterHeight = reader.ReadSingle();
            result.SessionID = reader.ReadUInt32();
            result.ArrivalCost = reader.ReadSingle();
            result.IsOpen = reader.ReadUInt32() == 1;
            result.Heuristic = reader.ReadSingle();
            result.mX = reader.ReadInt16();
            result.mY = reader.ReadInt16();
            result.ActorList = reader.ReadUInt32();
            reader.ReadUInt32(); // <- "Unk1"
            result.GoodCellSessionID = reader.ReadUInt32();
            result.RefHintWeight = reader.ReadSingle();
            reader.ReadUInt16();   // <- "Unk2"
            result.ArrivalDirection = reader.ReadInt16();
            result.RefHintNode = new short[2];
            result.RefHintNode[0] = reader.ReadInt16();
            result.RefHintNode[1] = reader.ReadInt16();

            return result;
        }
    }

    public class NavGridReader
    {
        public static NavGrid ReadBinary(string filePath)
        {
            NavBinaryReader b;

            try
            {
                b = new NavBinaryReader(File.Open(filePath, FileMode.Open, FileAccess.Read));
            }
            catch (Exception e)
            {
                throw new Exception("There was an error reading the file: " + e.ToString());
            }

            return ReadData(b);
        }

        public static NavGrid ReadBinary(byte[] fileBytes)
        {
            NavBinaryReader b;

            try
            {
                b = new NavBinaryReader(new MemoryStream(fileBytes));
            }
            catch (Exception e)
            {
                throw new Exception("There was an error reading the file: " + e.ToString());
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
                grid.MinorVersion =  b.GetBinaryReader().ReadInt16();

            grid = ReadStandardNavGridHeader(b, grid);
            grid.Cells = new NavGridCell[grid.XCellCount * grid.YCellCount];
            grid.CellFlags = new ushort[grid.XCellCount * grid.YCellCount];

            switch (grid.MajorVersion)
            {
                case 0x02:
                case 0x03:
                case 0x05:
                    // Read cells, total size: 0x38 * XCellCount * YCellCount bytes

                    for (var i = 0; i < grid.Cells.Length; i++)
                    {
                        grid.Cells[i] = b.ReadGridCell_Version5(out grid.CellFlags[i]);
                        grid.Cells[i].ID = i;
                    }

                    grid.XSampledHeightCount = b.GetBinaryReader().ReadInt32();
                    grid.YSampledHeightCount = b.GetBinaryReader().ReadInt32();

                    //should be mXSampledHeightDist
                    grid.DirectionX = b.GetBinaryReader().ReadSingle();
                    //should be mYSampledHeightDist
                    grid.DirectionY = b.GetBinaryReader().ReadSingle();
                    break;
                case 0x07:
                    // Read cells, total size: 0x30 * XCellCount * YCellCount bytes

                    for (var i = 0; i < grid.Cells.Length; i++)
                    {
                        grid.Cells[i] = b.ReadGridCell_Version7();
                        grid.Cells[i].ID = i;
                    }

                    for (var i = 0; i < grid.Cells.Length; i++)
                        grid.CellFlags[i] = b.GetBinaryReader().ReadUInt16();

                    break;
                default:
                    throw new Exception("Magic number at the start is unsupported! Value: " + grid.MajorVersion.ToString("X"));
            }

            var highestX = 0;
            var highestY = 0;
            foreach (var cell in grid.Cells)
            {
                if (cell.mX > highestX)
                {
                    highestX = cell.mX;
                }
                if (cell.mY > highestY)
                {
                    highestY = cell.mY;
                }
            }
            var asdf = grid.TranslateFromNavGrid(new Vector<float> { X = highestX, Y = highestY });
            grid.MapWidth = asdf.X;
            grid.MapHeight = asdf.Y;

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

    public class Vector<T> where T : struct
    {
        public T X;
        public T Y;
        public T Z;

        public Vector() { }

        public void ForceSize(int size)
        {
            if (size <= 2)
            {
                Z = default(T);
            }
            if (size <= 1)
            {
                Y = default(T);
            }
            if (size == 0)
            {
                X = default(T);
            }
        }
    }
}