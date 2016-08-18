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
        public const int AIMESH_TEXTURE_SIZE = 1024;
        public BinaryReader buffer;
        public __AIMESHFILE fileStream;

        public double lowX, lowY, highX, highY, lowestZ;
        public ScanLine[] scanlineLowest = new ScanLine[AIMESH_TEXTURE_SIZE];
        public ScanLine[] scanlineHighest = new ScanLine[AIMESH_TEXTURE_SIZE];
        public float[] heightMap;
        public float[] xMap;
        public float[] yMap;
        public float mapWidth, mapHeight;
        public bool loaded;

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
                var triangle = new Triangle();
                for (var j = 0; j < 3; j++)
                    triangle.Face.v1[j] = buffer.ReadSingle();
                for (var j = 0; j < 3; j++)
                    triangle.Face.v2[j] = buffer.ReadSingle();
                for (var j = 0; j < 3; j++)
                    triangle.Face.v3[j] = buffer.ReadSingle();
                triangle.unk1 = buffer.ReadInt16();
                triangle.unk2 = buffer.ReadInt16();
                triangle.triangle_reference = buffer.ReadInt16();

                fileStream.triangles[i] = triangle;
            }
            outputMesh(AIMESH_TEXTURE_SIZE, AIMESH_TEXTURE_SIZE);

            ////writeFile(AIMESH_TEXTURE_SIZE, AIMESH_TEXTURE_SIZE);

            loaded = true;
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
                if (fileStream.triangles[i].Face.v1[0] < lowX)
                    lowX = fileStream.triangles[i].Face.v1[0];
                if (fileStream.triangles[i].Face.v2[0] < lowX)
                    lowX = fileStream.triangles[i].Face.v2[0];
                if (fileStream.triangles[i].Face.v3[0] < lowX)
                    lowX = fileStream.triangles[i].Face.v3[0];

                // Triangle low Y check
                if (fileStream.triangles[i].Face.v1[2] < lowY)
                    lowY = fileStream.triangles[i].Face.v1[2];
                if (fileStream.triangles[i].Face.v2[2] < lowY)
                    lowY = fileStream.triangles[i].Face.v2[2];
                if (fileStream.triangles[i].Face.v3[2] < lowY)
                    lowY = fileStream.triangles[i].Face.v3[2];

                // Triangle high X check
                if (fileStream.triangles[i].Face.v1[0] > highX)
                    highX = fileStream.triangles[i].Face.v1[0];
                if (fileStream.triangles[i].Face.v2[0] > highX)
                    highX = fileStream.triangles[i].Face.v2[0];
                if (fileStream.triangles[i].Face.v3[0] > highX)
                    highX = fileStream.triangles[i].Face.v3[0];

                // Triangle high Y check
                if (fileStream.triangles[i].Face.v1[2] > highY)
                    highY = fileStream.triangles[i].Face.v1[2];
                if (fileStream.triangles[i].Face.v2[2] > highY)
                    highY = fileStream.triangles[i].Face.v2[2];
                if (fileStream.triangles[i].Face.v3[2] > highY)
                    highY = fileStream.triangles[i].Face.v3[2];
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
                t_Triangle.Face.v1[0] = (float)((fileStream.triangles[i].Face.v1[0] - lowX) * highX);
                t_Triangle.Face.v1[1] = fileStream.triangles[i].Face.v1[1];
                t_Triangle.Face.v1[2] = (float)((fileStream.triangles[i].Face.v1[2] - lowY) * highY);

                t_Triangle.Face.v2[0] = (float)((fileStream.triangles[i].Face.v2[0] - lowX) * highX);
                t_Triangle.Face.v2[1] = fileStream.triangles[i].Face.v2[1];
                t_Triangle.Face.v2[2] = (float)((fileStream.triangles[i].Face.v2[2] - lowY) * highY);

                t_Triangle.Face.v3[0] = (float)((fileStream.triangles[i].Face.v3[0] - lowX) * highX);
                t_Triangle.Face.v3[1] = fileStream.triangles[i].Face.v3[1];
                t_Triangle.Face.v3[2] = (float)((fileStream.triangles[i].Face.v3[2] - lowY) * highY);

                /*
                // Draw just the wireframe.
                drawLine(t_Triangle.Face.v1[0], t_Triangle.Face.v1[2], t_Triangle.Face.v2[0], t_Triangle.Face.v2[2], t_Info, width, height);
                drawLine(t_Triangle.Face.v2[0], t_Triangle.Face.v2[2], t_Triangle.Face.v3[0], t_Triangle.Face.v3[2], t_Info, width, height);
                drawLine(t_Triangle.Face.v3[0], t_Triangle.Face.v3[2], t_Triangle.Face.v1[0], t_Triangle.Face.v1[2], t_Info, width, height);
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

            Vertex[] tempVertex = new Vertex[3];

            tempVertex[0] = new Vertex();
            tempVertex[0].x = triangle.Face.v1[0];
            tempVertex[0].y = triangle.Face.v1[2];
            tempVertex[0].z = triangle.Face.v1[1];

            tempVertex[1] = new Vertex();
            tempVertex[1].x = triangle.Face.v2[0];
            tempVertex[1].y = triangle.Face.v2[2];
            tempVertex[1].z = triangle.Face.v2[1];

            tempVertex[2] = new Vertex();
            tempVertex[2].x = triangle.Face.v3[0];
            tempVertex[2].y = triangle.Face.v3[2];
            tempVertex[2].z = triangle.Face.v3[1];

            fillScanLine(tempVertex[0], tempVertex[1]);
            fillScanLine(tempVertex[1], tempVertex[2]);
            fillScanLine(tempVertex[2], tempVertex[0]);

            float tempWidth = width;
            float tempHeight = height;
            // target width and width

            int startY = (int)Math.Min(tempVertex[0].y, Math.Min(tempVertex[1].y, tempVertex[2].y));
            int endY = (int)Math.Max(tempVertex[0].y, Math.Max(tempVertex[1].y, tempVertex[2].y));
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

        void fillScanLine(Vertex vertex1, Vertex vertex2)
        {
            // Fills a scanline structure with information about the triangle on this y scanline.

            if (vertex1.y > vertex2.y)
            {
                fillScanLine(vertex2, vertex1);
                return;
            }
            // We need to go from low to high so switch if the other one is higher

            if (vertex2.y < 0 || vertex1.y >= AIMESH_TEXTURE_SIZE)
                return;
            // There's nothing on this line

            var deltaPos = new Vertex();

            deltaPos.x = vertex2.x - vertex1.x;
            deltaPos.y = vertex2.y - vertex1.y;
            deltaPos.z = vertex2.z - vertex1.z;

            /*These are unused
            float tempWidth = AIMESH_TEXTURE_SIZE;
            float tempHeight = AIMESH_TEXTURE_SIZE;
            */

            float t_DYResp = deltaPos.y == 0 ? 0 : 1.0f / deltaPos.y;
            int startY = (int)vertex1.y, endY = (int)vertex2.y;

            float x = vertex1.x;
            float z = vertex1.z;

            float deltaX = deltaPos.x * t_DYResp;
            float deltaZ = deltaPos.z * t_DYResp;

            float t_Inc = 1.0f - FRACPOS(vertex1.y);

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
    }


    public class ScanLine
    {
        public float x, y, z, u, v;
    };

    public class Vertex
    {
        public float x, y, z;
    };
    public class Face
    {
        public float[] v1 = new float[3]; // Seems to be a vertex [x,y,z]
        public float[] v2 = new float[3]; // Seems to be a vertex [x,y,z]
        public float[] v3 = new float[3]; // Seems to be a vertex [x,y,z]
    }

    public class Triangle
    {
        public short unk1;
        public short unk2;
        public short triangle_reference;
        public Face Face = new Face();
    };

    public class __AIMESHFILE
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