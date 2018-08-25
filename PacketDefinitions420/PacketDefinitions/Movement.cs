using System.Collections.Generic;
using System.IO;
using System.Numerics;
using GameServerCore;
using GameServerCore.Content;

namespace PacketDefinitions420.PacketDefinitions
{
    internal static class Movement
    {
        public static Pair<bool, bool> IsAbsolute(Vector2 vec)
        {
            var ret = new Pair<bool, bool>();
            ret.Item1 = vec.X < sbyte.MinValue || vec.X > sbyte.MaxValue;
            ret.Item2 = vec.Y < sbyte.MinValue || vec.Y > sbyte.MaxValue;

            return ret;
        }

        public static void SetBitmaskValue(ref byte[] mask, int pos, bool val)
        {
            if (val)
            {
                mask[pos / 8] |= (byte)(1 << pos % 8);
            }
            else
            {
                mask[pos / 8] &= (byte)~(1 << pos % 8);
            }
        }

        public static byte[] EncodeWaypoints(INavGrid navGrid, List<Vector2> waypoints)
        {
            var mapSize = navGrid.GetSize();
            var numCoords = waypoints.Count * 2;

            var maskBytes = new byte[(numCoords - 3) / 8 + 1];
            var tempStream = new MemoryStream();
            var resultStream = new MemoryStream();
            var tempBuffer = new BinaryWriter(tempStream);
            var resultBuffer = new BinaryWriter(resultStream);

            var lastCoord = new Vector2();
            var coordinate = 0;
            foreach (var waypoint in waypoints)
            {
                var curVector = new Vector2((waypoint.X - mapSize.X) / 2, (waypoint.Y - mapSize.Y) / 2);
                if (coordinate == 0)
                {
                    tempBuffer.Write((short)curVector.X);
                    tempBuffer.Write((short)curVector.Y);
                }
                else
                {
                    var relative = new Vector2(curVector.X - lastCoord.X, curVector.Y - lastCoord.Y);
                    var isAbsolute = IsAbsolute(relative);

                    if (isAbsolute.Item1)
                    {
                        tempBuffer.Write((short)curVector.X);
                    }
                    else
                    {
                        tempBuffer.Write((byte)relative.X);
                    }

                    if (isAbsolute.Item2)
                    {
                        tempBuffer.Write((short)curVector.Y);
                    }
                    else
                    {
                        tempBuffer.Write((byte)relative.Y);
                    }

                    SetBitmaskValue(ref maskBytes, coordinate - 2, !isAbsolute.Item1);
                    SetBitmaskValue(ref maskBytes, coordinate - 1, !isAbsolute.Item2);
                }
                lastCoord = curVector;
                coordinate += 2;
            }

            if (numCoords > 2)
            {
                resultBuffer.Write(maskBytes);
            }

            resultBuffer.Write(tempStream.ToArray());

            return resultStream.ToArray();
        }
    }
}