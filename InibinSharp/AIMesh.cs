#region LICENSE

// Copyright 2014 - 2014 InibinSharp
// Inibin.cs is part of InibinSharp.
// InibinSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// InibinSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// You should have received a copy of the GNU General Public License
// along with InibinSharp. If not, see <http://www.gnu.org/licenses/>.

#endregion

#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using InibinSharp.RAF;
using System.Numerics;

#endregion

namespace InibinSharp
{
    public class AIMesh
    {
        protected const int AIMESH_TEXTURE_SIZE = 1024;
        protected BinaryReader buffer;
        protected __AIMESHFILE fileStream;

        protected double lowX, lowY, highX, highY, lowestZ;
        protected ScanLine[] scanlineLowest = new ScanLine[AIMESH_TEXTURE_SIZE];
        protected ScanLine[] scanlineHighest = new ScanLine[AIMESH_TEXTURE_SIZE];
        protected float[] heightMap;
        protected float[] xMap;
        protected float[] yMap;
        protected float mapWidth, mapHeight;
        protected bool loaded;

        public AIMesh(byte[] data)
            : this(new MemoryStream(data))
        {
        }

        public AIMesh(string filePath)
            : this(File.ReadAllBytes(filePath))
        {
        }

        public AIMesh(RAFFileListEntry file)
            : this(file.GetContent())
        {
        }

        public AIMesh(Stream stream)
        {
            buffer = new BinaryReader(stream);
            for (int i = 0; i < AIMESH_TEXTURE_SIZE; i++) // For every scanline for the triangle rendering
            {
                scanlineLowest[i] = new ScanLine { u = 1e10f, v = 1e10f, x = 1e10f, y = 1e10f, z = 1e10f };
                scanlineHighest[i] = new ScanLine { u = -1e10f, v = -1e10f, x = -1e10f, y = -1e10f, z = -1e10f };
            }
            heightMap = new float[AIMESH_TEXTURE_SIZE * AIMESH_TEXTURE_SIZE]; // Shows occupied or not
            xMap = new float[AIMESH_TEXTURE_SIZE * AIMESH_TEXTURE_SIZE]; // Shows x
            yMap = new float[AIMESH_TEXTURE_SIZE * AIMESH_TEXTURE_SIZE]; // Shows y

            fileStream = new __AIMESHFILE();
            fileStream.magic = buffer.ReadChars(8);
            fileStream.version = buffer.ReadInt32();
            fileStream.triangle_count = buffer.ReadInt32();
            fileStream.zero = new int[] { buffer.ReadInt32(), buffer.ReadInt32() };

            for (var i = 0; i < fileStream.triangle_count; i++)
            {
                var triangle = ReadTriangle(buffer);

                fileStream.triangles[i] = triangle;
            }
            outputMesh(AIMESH_TEXTURE_SIZE, AIMESH_TEXTURE_SIZE);

            ////writeFile(AIMESH_TEXTURE_SIZE, AIMESH_TEXTURE_SIZE);

            loaded = true;
        }

        private Triangle ReadTriangle(BinaryReader reader)
        {
            Triangle tri = new Triangle();

            tri.Face.v1.X = reader.ReadSingle();
            tri.Face.v1.Y = reader.ReadSingle();
            tri.Face.v1.Z = reader.ReadSingle();

            tri.Face.v2.X = reader.ReadSingle();
            tri.Face.v2.Y = reader.ReadSingle();
            tri.Face.v2.Z = reader.ReadSingle();

            tri.Face.v3.X = reader.ReadSingle();
            tri.Face.v3.Y = reader.ReadSingle();
            tri.Face.v3.Z = reader.ReadSingle();

            tri.neighbour1 = buffer.ReadInt16();
            tri.neighbour2 = buffer.ReadInt16();
            tri.neighbour3 = buffer.ReadInt16();

            return tri;
        }

        bool outputMesh(int width, int height)
        {
            mapHeight = mapWidth = 0;
            for (var i = 0; i < width * height; i++)
                heightMap[i] = -99999.99f; // Clear the map

            lowX = 9e9;
            lowY = 9e9;
            highX = 0;
            highY = 0;
            lowestZ = 9e9; // Init triangle

            for (var i = 0; i < fileStream.triangle_count; i++)
            // Need to find the absolute values.. So we can map it to AIMESH_TEXTURE_SIZExAIMESH_TEXTURE_SIZE instead of 13000x15000
            {
                // Triangle low X check
                if (fileStream.triangles[i].Face.v1.X < lowX)
                    lowX = fileStream.triangles[i].Face.v1.X;
                if (fileStream.triangles[i].Face.v2.X < lowX)
                    lowX = fileStream.triangles[i].Face.v2.X;
                if (fileStream.triangles[i].Face.v3.X < lowX)
                    lowX = fileStream.triangles[i].Face.v3.X;

                // Triangle low Y check
                if (fileStream.triangles[i].Face.v1.Z < lowY)
                    lowY = fileStream.triangles[i].Face.v1.Z;
                if (fileStream.triangles[i].Face.v2.Z < lowY)
                    lowY = fileStream.triangles[i].Face.v2.Z;
                if (fileStream.triangles[i].Face.v3.Z < lowY)
                    lowY = fileStream.triangles[i].Face.v3.Z;

                // Triangle high X check
                if (fileStream.triangles[i].Face.v1.X > highX)
                    highX = fileStream.triangles[i].Face.v1.X;
                if (fileStream.triangles[i].Face.v2.X > highX)
                    highX = fileStream.triangles[i].Face.v2.X;
                if (fileStream.triangles[i].Face.v3.X > highX)
                    highX = fileStream.triangles[i].Face.v3.X;

                // Triangle high Y check
                if (fileStream.triangles[i].Face.v1.Z > highY)
                    highY = fileStream.triangles[i].Face.v1.Z;
                if (fileStream.triangles[i].Face.v2.Z > highY)
                    highY = fileStream.triangles[i].Face.v2.Z;
                if (fileStream.triangles[i].Face.v3.Z > highY)
                    highY = fileStream.triangles[i].Face.v3.Z;
            }

            mapWidth = (float)(highX + lowX);
            mapHeight = (float)(highY + lowY);

            // If the width or width larger?
            if ((highY - lowY) < (highX - lowX))
            {
                highX = 1.0f / (highX - lowX) * width; // We're wider than we're high, map on width
                highY = highX; // Keep aspect ratio Basically, 1 y should be 1 x.
                lowY = 0; // Though we need to project this in the middle, no offset
            }
            else
            {
                highY = 1.0f / (highY - lowY) * height; // We're higher than we're wide, map on width
                highX = highY; // Keep aspect ratio Basically, 1 x should be 1 y.
                               // lowX = 0; // X is already in the middle? ??????
            }

            for (var i = 0; i < fileStream.triangle_count; i++) // For every triangle
            {
                Triangle t_Triangle = new Triangle();// Create a triangle that is warped to heightmap coordinates
                t_Triangle.Face.v1.X = (float)((fileStream.triangles[i].Face.v1.X - lowX) * highX);
                t_Triangle.Face.v1.Y = fileStream.triangles[i].Face.v1.Y;
                t_Triangle.Face.v1.Z = (float)((fileStream.triangles[i].Face.v1.Z - lowY) * highY);

                t_Triangle.Face.v2.X = (float)((fileStream.triangles[i].Face.v2.X - lowX) * highX);
                t_Triangle.Face.v2.Y = fileStream.triangles[i].Face.v2.Y;
                t_Triangle.Face.v2.Z = (float)((fileStream.triangles[i].Face.v2.Z - lowY) * highY);

                t_Triangle.Face.v3.X = (float)((fileStream.triangles[i].Face.v3.X - lowX) * highX);
                t_Triangle.Face.v3.Y = fileStream.triangles[i].Face.v3.Y;
                t_Triangle.Face.v3.Z = (float)((fileStream.triangles[i].Face.v3.Z - lowY) * highY);

                /*
                // Draw just the wireframe.
                drawLine(t_Triangle.Face.v1.x, t_Triangle.Face.v1.z, t_Triangle.Face.v2.x, t_Triangle.Face.v2.z, t_Info, width, height);
                drawLine(t_Triangle.Face.v2.x, t_Triangle.Face.v2.z, t_Triangle.Face.v3.x, t_Triangle.Face.v3.z, t_Info, width, height);
                drawLine(t_Triangle.Face.v3.x, t_Triangle.Face.v3.z, t_Triangle.Face.v1.x, t_Triangle.Face.v1.z, t_Info, width, height);
                */

                // Draw this triangle onto the heightmap using an awesome triangle drawing function
                drawTriangle(t_Triangle, width, height);
            }

            //writeFile(t_Info, width, height);
            return true;
        }
        void drawTriangle(Triangle triangle, int width, int height)
        {
            // The heart of the rasterizer

            Vector3[] tempVertex = new Vector3[3];

            tempVertex[0] = new Vector3();
            tempVertex[0].X = triangle.Face.v1.X;
            tempVertex[0].Y = triangle.Face.v1.Z;
            tempVertex[0].Z = triangle.Face.v1.Y;

            tempVertex[1] = new Vector3();
            tempVertex[1].X = triangle.Face.v2.X;
            tempVertex[1].Y = triangle.Face.v2.Z;
            tempVertex[1].Z = triangle.Face.v2.Y;

            tempVertex[2] = new Vector3();
            tempVertex[2].X = triangle.Face.v3.X;
            tempVertex[2].Y = triangle.Face.v3.Z;
            tempVertex[2].Z = triangle.Face.v3.Y;

            fillScanLine(tempVertex[0], tempVertex[1]);
            fillScanLine(tempVertex[1], tempVertex[2]);
            fillScanLine(tempVertex[2], tempVertex[0]);

            float tempWidth = width;
            float tempHeight = height;
            // target width and width

            int startY = (int)Math.Min(tempVertex[0].Y, Math.Min(tempVertex[1].Y, tempVertex[2].Y));
            int endY = (int)Math.Max(tempVertex[0].Y, Math.Max(tempVertex[1].Y, tempVertex[2].Y));
            // Get the scanline where we start drawing and where we stop.

            endY = Math.Min(endY, height - 1);
            startY = Math.Max(0, startY);

            for (int y = startY; y <= endY; y++) // for each scanline
            {
                if (scanlineLowest[y].x < scanlineHighest[y].x) // If we actually have something filled in this scanline
                {
                    int yw = y * height;

                    float z = scanlineLowest[y].z;
                    float u = scanlineLowest[y].u;
                    float v = scanlineLowest[y].v;
                    // The start of the Z, U, and V coordinate.

                    float deltaX = 1.0f / (scanlineHighest[y].x - scanlineLowest[y].x);
                    // Interpolation over X (change in X between the two, then reverse it so it's usable as multiplication
                    // in divisions

                    float deltaZ = (scanlineHighest[y].z - scanlineLowest[y].z) * deltaX;
                    float deltaU = (scanlineHighest[y].u - scanlineLowest[y].u) * deltaX;
                    float deltaV = (scanlineHighest[y].v - scanlineLowest[y].v) * deltaX;
                    // The interpolation in Z, U and V in respect to the interpolation of X	

                    // Sub-texel correction
                    int x = (int)scanlineLowest[y].x;
                    int tx = x + 1;
                    int distInt = (int)(scanlineHighest[y].x) - (int)(scanlineLowest[y].x);
                    if (distInt > 0.0f)
                    {
                        u += (((float)(tx)) - tx) * deltaU;
                        v += (((float)(tx)) - tx) * deltaV;
                        z += (((float)(tx)) - tx) * deltaZ;
                    }

                    if (!(scanlineHighest[y].x < 0 || x >= height))
                        for (int i = 0; x < (int)scanlineHighest[y].x; i++, x++) // for each piece of the scanline
                        {
                            if (x >= height) break; // If we're out of screen, break out this loop

                            if (x < 0)
                            {
                                int inverseX = Math.Abs(x);
                                z += deltaZ * inverseX;
                                u += deltaU * inverseX;
                                v += deltaV * inverseX;
                                x = 0;
                            }


                            {
                                // Get the point on the texture that we need to draw. It basically picks a pixel based on the uv.

                                //a_Target->GetRenderTarget()->Plot(x, tempHeight - y, 255);
                                heightMap[x + (y) * height] = z;
                                xMap[x + (y) * height] = scanlineLowest[y].x + deltaX * i;
                                yMap[x + (y) * height] = scanlineLowest[y].y;
                                if (z < lowestZ) lowestZ = z;
                            }

                            z += deltaZ;
                            u += deltaU;
                            v += deltaV;
                            // interpolate
                        }
                }

                scanlineLowest[y].x = 1e10f;
                scanlineHighest[y].x = -1e10f;
            }
        }

        void fillScanLine(Vector3 vertex1, Vector3 vertex2)
        {
            // Fills a scanline structure with information about the triangle on this y scanline.

            if (vertex1.Y > vertex2.Y)
            {
                fillScanLine(vertex2, vertex1);
                return;
            }
            // We need to go from low to high so switch if the other one is higher

            if (vertex2.Y < 0 || vertex1.Y >= AIMESH_TEXTURE_SIZE)
                return;
            // There's nothing on this line

            var deltaPos = new Vector3();

            deltaPos.X = vertex2.X - vertex1.X;
            deltaPos.Y = vertex2.Y - vertex1.Y;
            deltaPos.Z = vertex2.Z - vertex1.Z;


            float t_DYResp = deltaPos.Y == 0 ? 0 : 1.0f / deltaPos.Y;
            int startY = (int)vertex1.Y, endY = (int)vertex2.Y;

            float x = vertex1.X;
            float z = vertex1.Z;

            float deltaX = deltaPos.X * t_DYResp;
            float deltaZ = deltaPos.Z * t_DYResp;

            float t_Inc = 1.0f - FRACPOS(vertex1.Y);

            // subpixel correction
            startY++;
            x += deltaX * t_Inc;
            z += deltaZ * t_Inc;

            if (startY < 0)
            {
                x -= deltaX * startY;
                z -= deltaZ * startY;
                startY = 0;
            }

            // Small fix
            if (endY >= AIMESH_TEXTURE_SIZE) endY = AIMESH_TEXTURE_SIZE - 1;

            // For each scanline that this triangle uses
            for (int y = startY; y <= endY; y++)
            {
                if (x < scanlineLowest[y].x) // If the x is lower than our lowest x
                {
                    scanlineLowest[y].x = (x); // Fill the scanline struct with our info
                    scanlineLowest[y].y = (float)y;
                    scanlineLowest[y].z = z;
                }
                if (x > scanlineHighest[y].x) // If the x is higher than our highest x
                {
                    scanlineHighest[y].x = (x); // Fill the scanline struct with our info
                    scanlineHighest[y].y = (float)y;
                    scanlineHighest[y].z = z;
                }

                // Interpolate
                // Or going to the part of the triangle on the next scanline
                x += deltaX;
                z += deltaZ;
            }
        }

        //??
        private float FRACPOS(float x)
        {
            return x - (int)(x);
        }

        public class Triangle
        {
            public short neighbour1 { get; internal set; }
            public short neighbour2 { get; internal set; }
            public short neighbour3 { get; internal set; }
            public Face Face = new Face();
        };

        public struct Face
        {
            public Vector3 v1;
            public Vector3 v2;
            public Vector3 v3;
        }

        protected class ScanLine
        {
            public float x, y, z, u, v;
        };

        protected class __AIMESHFILE
        {
            private const int MAX_TRIANGLE_COUNT = (2 << 15);
            public char[] magic = new char[8];
            public int version;
            public int triangle_count;
            public int[] zero = new int[2];

            public Triangle[] triangles = new Triangle[MAX_TRIANGLE_COUNT];
            // TODO: I have to find a better way to do this. :/
            // Basically, it's a set of triangles that go on for triangle_count. 
            // Dynamic allocation is stupid, and this way is potentially well, unsafe.
            // pointers will make the app think that the first 4 bytes are a pointer.. Sigh.
        };

    }



}