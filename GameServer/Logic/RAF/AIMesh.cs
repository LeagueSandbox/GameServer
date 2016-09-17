using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Logic.GameObjects;
using static ENet.Native;
using LeagueSandbox.GameServer.Logic.Enet;

namespace LeagueSandbox.GameServer.Logic.RAF
{
    public class AIMesh : InibinSharp.AIMesh
    {
        public AIMesh(byte[] data) : base(data)
        {
        }

        public AIMesh(string filePath) : base(filePath)
        {
        }

        public AIMesh(InibinSharp.RAF.RAFFileListEntry file) : base(file)
        {
        }

        public AIMesh(Stream stream) : base(stream)
        {

        }
        public float getY(float argX, float argY)
        {
            if (loaded)
            {
                argX = (int)((argX - lowX) * highX); argY = (int)((argY - lowY) * highY);

                if ((argX >= 0.0f && argX < AIMESH_TEXTURE_SIZE) && (argY >= 0.0f && argY < AIMESH_TEXTURE_SIZE))
                {
                    int pos = (int)((argX) + (argY) * AIMESH_TEXTURE_SIZE);
                    if (pos < AIMESH_TEXTURE_SIZE * AIMESH_TEXTURE_SIZE)
                        return heightMap[pos];
                }
            }
            return -99999.99f;
        }
        public bool isWalkable(float argX, float argY)
        {
            return getY(argX, argY) > -254.0f;
        }

        public float castRay(Vector2 origin, Vector2 destination, bool inverseRay = false)
        {
            return (float)Math.Sqrt(castRaySqr(origin, destination, inverseRay));

        }
        public float castRaySqr(Vector2 origin, Vector2 destination, bool inverseRay = false)
        {
            float x1 = origin.X;
            float y1 = origin.Y;
            float x2 = destination.X;
            float y2 = destination.Y;

            if ((x1 < 0) || (y1 < 0) || (x1 >= mapWidth) || (y1 >= mapHeight))
            {
                return 0.0f;
            }

            float b = x2 - x1;
            float h = y2 - y1;
            float l = Math.Abs(b);
            if (Math.Abs(h) > l) l = Math.Abs(h);
            int il = (int)l;
            float dx = b / (float)l;
            float dy = h / (float)l;
            int i;
            for (i = 0; i <= il; i++)
            {
                if (isWalkable(x1, y1) == inverseRay) break;
                // Inverse = report on walkable
                // Normal = report on terrain
                // so break when isWalkable == true and inverse == true
                // Break when isWalkable == false and inverse == false
                x1 += dx;
                y1 += dy;
            }

            if (i == il)
                return (/*TranslateToRealCoordinate*/(new Vector2(x2, y2)) - /*TranslateToRealCoordinate*/(origin)).SqrLength();
            return (/*TranslateToRealCoordinate*/(new Vector2(x1, y1)) - /*TranslateToRealCoordinate*/(origin)).SqrLength();
        }

        public float castInfiniteRaySqr(Vector2 origin, Vector2 direction, bool inverseRay = false)
        {
            return castRaySqr(origin, origin + (direction * (float)AIMESH_TEXTURE_SIZE), inverseRay);
        }
        public float castInfiniteRay(Vector2 origin, Vector2 direction, bool inverseRay = false)
        {
            return (float)Math.Sqrt(castInfiniteRaySqr(origin, direction, inverseRay));
        }
        public bool isAnythingBetween(Vector2 a, Vector2 b)
        {
            return (castRaySqr(a, b) <= (b - a).SqrLength());
        }
        public bool isAnythingBetween(GameObject a, GameObject b)
        {
            return (castRaySqr(a.GetPosition(), b.GetPosition()) <= (b.GetPosition() - a.GetPosition()).SqrLength());
        }
        public Vector2 getClosestTerrainExit(Vector2 location)
        {
            if (isWalkable(location.X, location.Y))
            {
                return location;
            }

            var trueX = (double)location.X;
            var trueY = (double)location.Y;
            var angle = Math.PI / 180;
            var rr = (location.X - trueX) * (location.X - trueX) + (location.Y - trueY) * (location.Y - trueY);
            var r = Math.Sqrt(rr);
            // x = r * cos(angle)
            // y = r * sin(angle)
            // r = distance from center
            // Draws spirals until it finds a way out
            while (!isWalkable((float)trueX, (float)trueY))
            {
                trueX = location.X + r * Math.Cos(angle);
                trueY = location.Y + r * Math.Sin(angle);
                angle += Math.PI / 180;
                r += 0.1f;
            }
            return new Vector2((float)trueX, (float)trueY);
        }

        public float getWidth()
        {
            return mapWidth;
        }
        public float getHeight()
        {
            return mapHeight;
        }
        public float getSize()
        {
            return (mapHeight > mapWidth) ? mapHeight : mapWidth;
        }

        public Vector2 TranslateToTextureCoordinate(Vector2 vector)
        {
            if (loaded)
            {
                vector.X = (int)((vector.X - lowX) * highX);
                vector.Y = (int)((vector.Y - lowY) * highY);
            }
            return vector;
        }
        public Vector2 TranslateToRealCoordinate(Vector2 vector)
        {
            if (loaded)
            {
                vector.X = (float)((vector.X / highX) + lowX);
                vector.Y = (float)((vector.Y / highY) + lowY);
            }
            return vector;
        }

        public bool isLoaded()
        {
            return loaded;
        }
    }
}