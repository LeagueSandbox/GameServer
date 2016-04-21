using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using InibinSharp;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer;

namespace LeagueSandbox.GameServer.Logic.RAF
{
    public class AIMesh : InibinSharp.AIMesh
    {
        public int TriangleCount {get { return fileStream.triangle_count; } }


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

        internal Vector2 GetClosestTerrainExit(Vector2 to)
        {
            //TODO: Optimize
            //So far called only when pathfinding to inaccesible location
             
            Vector2 bestPoint = new Vector2();
            float bestDist = float.MaxValue;
            for(int i=0;i<TriangleCount;i++)
            {
                Vector2 candidate = ClosestPointInTriangle(to, getTriangle(i));
                float distance = Vector2.Distance(to, candidate);

                if (distance < bestDist)
                {
                    bestPoint = candidate;
                    bestDist = distance;
                }
                    
            }
            return bestPoint;
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
            return (castRaySqr(a.getPosition(), b.getPosition()) <= (b.getPosition() - a.getPosition()).SqrLength());
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

        public Triangle getTriangle(int index)
        {
            if (index < 0 || index >= TriangleCount)
                return null;
            else
                return fileStream.triangles[index];  
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

        
        /// <summary>
        /// Returns indexes of triangle's neighbours
        /// </summary>
        public List<int> GetTriNeighbours(int index)
        {
            if (index < 0 || index >= fileStream.triangle_count)
                return new List<int>();

            List<int> ret = new List<int>();
            if (fileStream.triangles[index].neighbour1 >= 0)
                ret.Add(fileStream.triangles[index].neighbour1);

            if (fileStream.triangles[index].neighbour2 >= 0)
                ret.Add(fileStream.triangles[index].neighbour2);

            if (fileStream.triangles[index].neighbour3 >= 0)
                ret.Add(fileStream.triangles[index].neighbour3);

            return ret;
        }

        /// <summary>
        /// Finds index of navmesh triangle of position
        /// Returns -1 if position is outside navmesh
        /// </summary>
        public int GetTriAtPos(Vector2 pos)
        {
            //TODO: Optimize (takes about 1ms), possibly use quadtree?
            for(int i=0;i<fileStream.triangle_count;i++)
            {
                Triangle tri = fileStream.triangles[i];
                if (IsInside(tri, pos))
                    return i;
            }
            return -1;
        }

        public bool IsInside(Triangle tri, Vector2 pos)
        {
            //add some tolerance to avoid problems between triangle edges
            float tolerance = mapWidth > mapHeight ? mapWidth : mapHeight;
            tolerance *= float.Epsilon;

            Vector2 p0 = new Vector2(tri.Face.v1.X, tri.Face.v1.Z);
            Vector2 p1 = new Vector2(tri.Face.v2.X, tri.Face.v2.Z);
            Vector2 p2 = new Vector2(tri.Face.v3.X, tri.Face.v3.Z);

            var s = p0.Y * p2.X - p0.X * p2.Y + (p2.Y - p0.Y) * pos.X + (p0.X - p2.X) * pos.Y;
            var t = p0.X * p1.Y - p0.Y * p1.X + (p0.Y - p1.Y) * pos.X + (p1.X - p0.X) * pos.Y;

            if ((s < 0) != (t < 0))
                return false;

            var A = -p1.Y * p2.X + p0.Y * (p2.X - p1.X) + p0.X * (p1.Y - p2.Y) + p1.X * p2.Y;
            if (A < 0.0)
            {
                s = -s;
                t = -t;
                A = -A;
            }
            return s > 0-tolerance && t > 0-tolerance && (s + t) <= A + tolerance;
        }

        public float DistanceToTriangle(Vector2 point, int triangleIndex)
        {
            if (triangleIndex < 0 || triangleIndex >= TriangleCount)
            {
                Logger.LogCoreError("DistanceToTriangle invalid index " + triangleIndex);
                return 0.0f;
            }

            return Vector2.Distance(point, ClosestPointInTriangle(point, getTriangle(triangleIndex)));
        }

        public Vector2 ClosestPointInTriangle(Vector2 point,Triangle tri)
        {
            return ClosestPointInTriangle(point, new Vector2(tri.Face.v1.X, tri.Face.v1.Z),
                new Vector2(tri.Face.v2.X, tri.Face.v2.Z),
                new Vector2(tri.Face.v3.X, tri.Face.v3.Z));
        }

        public Vector2 ClosestPointInTriangle(Vector2 point, Vector2 a, Vector2 b, Vector2 c)
        {
            // Check if P in vertex region outside A
            Vector2 ab = b - a;
            Vector2 ac = c - a;
            Vector2 ap = point - a;
            float d1 = Vector2.Dot(ab, ap);
            float d2 = Vector2.Dot(ac, ap);
            if (d1 <= 0.0f && d2 <= 0.0f) return a; // barycentric coordinates (1,0,0)
                                                    // Check if P in vertex region outside B
            Vector2 bp = point - b;
            float d3 = Vector2.Dot(ab, bp);
            float d4 = Vector2.Dot(ac, bp);
            if (d3 >= 0.0f && d4 <= d3) return b; // barycentric coordinates (0,1,0)
                                                  // Check if P in edge region of AB, if so return projection of P onto AB
            float vc = d1 * d4 - d3 * d2;
            if (vc <= 0.0f && d1 >= 0.0f && d3 <= 0.0f)
            {
                float v2 = d1 / (d1 - d3);
                return a + v2 * ab; // barycentric coordinates (1-v,v,0)
            }
            // Check if P in vertex region outside C
            Vector2 cp = point - c;
            float d5 = Vector2.Dot(ab, cp);
            float d6 = Vector2.Dot(ac, cp);
            if (d6 >= 0.0f && d5 <= d6) return c; // barycentric coordinates (0,0,1)
                                                  // Check if P in edge region of AC, if so return projection of P onto AC
            float vb = d5 * d2 - d1 * d6;
            if (vb <= 0.0f && d2 >= 0.0f && d6 <= 0.0f)
            {
                float w2 = d2 / (d2 - d6);
                return a + w2 * ac; // barycentric coordinates (1-w,0,w)
            }
            // Check if P in edge region of BC, if so return projection of P onto BC
            float va = d3 * d6 - d5 * d4;
            if (va <= 0.0f && (d4 - d3) >= 0.0f && (d5 - d6) >= 0.0f)
            {
                float w2 = (d4 - d3) / ((d4 - d3) + (d5 - d6));
                return b + w2 * (c - b); // barycentric coordinates (0,1-w,w)
            }
            // P inside face region. Compute Q through its barycentric coordinates (u,v,w)
            float denom = 1.0f / (va + vb + vc);
            float v = vb * denom;
            float w = vc * denom;
            return a + ab * v + ac * w; // = u*a + v*b + w*c, u = va * denom = 1.0f-v-w
        }


        /// <summary>
        /// Finds portal vertices between two NavMesh triangles
        /// </summary>
        /// <returns>Portal vertices in order (left,right) </returns>
        public Tuple<Vector2, Vector2> GetPortalPoints(int firstIndex, int secondIndex)
        {
            //left and right are shared vertieces between 1st and 2nd nav triangle
            List<Vector3> sharedVerts = new List<Vector3>();

            Triangle first = getTriangle(firstIndex);
            Triangle second = getTriangle(secondIndex);
            foreach (Vector3 vec1 in new List<Vector3> { first.Face.v1, first.Face.v2, first.Face.v3 })
            {
                foreach (Vector3 vec2 in new List<Vector3> { second.Face.v1, second.Face.v2, second.Face.v3 })
                    if (vec1 == vec2) //tolerance should not be necessary, Vectors should be bit-exact copies
                        sharedVerts.Add(vec1);
            }

            if (sharedVerts.Count != 2)
            {
                Logger.LogCoreError("Could not find shared vertices in navmesh");
            }

            

            Vector2 a = new Vector2(sharedVerts[0].X, sharedVerts[0].Z);
            Vector2 b = new Vector2(sharedVerts[1].X, sharedVerts[1].Z);
            Vector2 c = GetTriCenter(firstIndex);
            //verify that we return in correct order;
            if (MathExt.Vec2Cross(a - c, b - c) < 0)
                return Tuple.Create(a, b);
            else
                return Tuple.Create(b, a);
        }

        


        /// <summary>
        /// Distance between two triangle centers (ignores height)
        /// </summary>
        public float GetTriDistance(int a, int b)
        {
            return Vector2.Distance(GetTriCenter(a), GetTriCenter(b));
        }

        /// <summary>
        /// Returns center of NavMesh triangle (ignores height)
        /// </summary>
        public Vector2 GetTriCenter(int triangleIndex)
        {
            if (triangleIndex < 0 || triangleIndex >= fileStream.triangle_count)
            {
                Logger.LogCoreError("AIMesh:Trying to find center of nonexisting triangle!");
                return new Vector2(float.NaN, float.NaN);
            }
                
            else
            {
                Triangle t = fileStream.triangles[triangleIndex];

                Vector2 center = new Vector2(0, 0);
                center.X = (t.Face.v1.X + t.Face.v2.X + t.Face.v3.X) / 3;
                center.Y = (t.Face.v1.Z + t.Face.v2.Z + t.Face.v3.Z) / 3;
                return center;

            }
               

        }
    }
}