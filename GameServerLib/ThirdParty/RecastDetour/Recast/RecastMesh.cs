using System;
using System.Diagnostics;

public static partial class Recast {

    public class rcEdge {
        public ushort[] vert = new ushort[2];
        public ushort[] polyEdge = new ushort[2];
        public ushort[] poly = new ushort[2];
    };

    public static bool buildMeshAdjacency(ushort[] polys, int npolys,
                                   int nverts, int vertsPerPoly) {
        // Based on code by Eric Lengyel from:
        // http://www.terathon.com/code/edges.php

        int maxEdgeCount = npolys * vertsPerPoly;
        ushort[] firstEdge = new ushort[nverts + maxEdgeCount];//(ushort*)rcAlloc(sizeof(ushort)*(nverts + maxEdgeCount), RC_ALLOC_TEMP);
        if (firstEdge == null)
            return false;
        //ushort* nextEdge = firstEdge + nverts;
        int nextEdgeIndex = nverts;
        int edgeCount = 0;

        //rcEdge* edges = (rcEdge*)rcAlloc(sizeof(rcEdge)*maxEdgeCount, RC_ALLOC_TEMP);
        rcEdge[] edges = new rcEdge[maxEdgeCount];
        rccsArrayItemsCreate(edges);
        if (edges == null) {
            //rcFree(firstEdge);
            firstEdge = null;
            return false;
        }

        for (int i = 0; i < nverts; i++) {
            firstEdge[i] = RC_MESH_NULL_IDX;
        }

        for (int i = 0; i < npolys; ++i) {
            int tIndex = i * vertsPerPoly * 2;
            //ushort* t = &polys[i*vertsPerPoly*2];
            for (int j = 0; j < vertsPerPoly; ++j) {
                if (polys[tIndex + j] == RC_MESH_NULL_IDX) break;
                ushort v0 = polys[tIndex + j];
                ushort v1 = (j + 1 >= vertsPerPoly || polys[tIndex + j + 1] == RC_MESH_NULL_IDX) ? polys[tIndex + 0] : polys[tIndex + j + 1];
                if (v0 < v1) {
                    rcEdge edge = edges[edgeCount];
                    edge.vert[0] = v0;
                    edge.vert[1] = v1;
                    edge.poly[0] = (ushort)i;
                    edge.polyEdge[0] = (ushort)j;
                    edge.poly[1] = (ushort)i;
                    edge.polyEdge[1] = 0;
                    // Insert edge
                    firstEdge[nextEdgeIndex + edgeCount] = firstEdge[v0];
                    firstEdge[v0] = (ushort)edgeCount;
                    edgeCount++;
                }
            }
        }

        for (int i = 0; i < npolys; ++i) {
            //ushort* t = &polys[i*vertsPerPoly*2];
            int tIndex = i * vertsPerPoly * 2;
            for (int j = 0; j < vertsPerPoly; ++j) {
                if (polys[tIndex + j] == RC_MESH_NULL_IDX) break;
                ushort v0 = polys[tIndex + j];
                ushort v1 = (j + 1 >= vertsPerPoly || polys[tIndex + j + 1] == RC_MESH_NULL_IDX) ? polys[tIndex + 0] : polys[tIndex + j + 1];
                if (v0 > v1) {
                    for (ushort e = firstEdge[v1]; e != RC_MESH_NULL_IDX; e = firstEdge[nextEdgeIndex + e]) {
                        rcEdge edge = edges[e];
                        if (edge.vert[1] == v0 && edge.poly[0] == edge.poly[1]) {
                            edge.poly[1] = (ushort)i;
                            edge.polyEdge[1] = (ushort)j;
                            break;
                        }
                    }
                }
            }
        }

        // Store adjacency
        for (int i = 0; i < edgeCount; ++i) {
            rcEdge e = edges[i];
            if (e.poly[0] != e.poly[1]) {
                //ushort* p0 = &polys[e.poly[0]*vertsPerPoly*2];
                //ushort* p1 = &polys[e.poly[1]*vertsPerPoly*2];
                //p0[vertsPerPoly + e.polyEdge[0]] = e.poly[1];
                //p1[vertsPerPoly + e.polyEdge[1]] = e.poly[0];
                polys[e.poly[0] * vertsPerPoly * 2 + vertsPerPoly + e.polyEdge[0]] = e.poly[1];
                polys[e.poly[1] * vertsPerPoly * 2 + vertsPerPoly + e.polyEdge[1]] = e.poly[0];
            }
        }

        //rcFree(firstEdge);
        //rcFree(edges);

        return true;
    }


    const int VERTEX_BUCKET_COUNT = (1 << 12);

    public static int computeVertexHash(int x, int y, int z) {
        uint h1 = 0x8da6b343; // Large multiplicative constants;
        uint h2 = 0xd8163841; // here arbitrarily chosen primes
        uint h3 = 0xcb1ab31f;
        uint n = (uint)(h1 * x + h2 * y + h3 * z);
        return (int)(n & (VERTEX_BUCKET_COUNT - 1));
    }

    public static ushort addVertex(ushort x, ushort y, ushort z,
                                    ushort[] verts, int[] firstVert, int[] nextVert, ref int nv) {
        int bucket = computeVertexHash(x, 0, z);
        int i = firstVert[bucket];

        while (i != -1) {
            //const ushort* v = &verts[i*3];
            int vIndex = i * 3;
            if (verts[vIndex] == x && (Math.Abs(verts[vIndex + 1] - y) <= 2) && verts[vIndex + 2] == z) {
                return (ushort)i;
            }
            i = nextVert[i]; // next
        }

        // Could not find, create new.
        i = nv; nv++;
        //ushort[] v = &verts[i*3];
        int vInd = i * 3;
        verts[vInd] = x;
        verts[vInd + 1] = y;
        verts[vInd + 2] = z;
        nextVert[i] = firstVert[bucket];
        firstVert[bucket] = i;

        return (ushort)i;
    }

    public static int prev(int i, int n) {
        return i - 1 >= 0 ? i - 1 : n - 1;
    }
    public static int next(int i, int n) {
        return i + 1 < n ? i + 1 : 0;
    }

    public static int area2(int[] a, int[] b, int[] c) {
        return (b[0] - a[0]) * (c[2] - a[2]) - (c[0] - a[0]) * (b[2] - a[2]);
    }
    public static int area2(int[] a, int aStart, int[] b, int bStart, int[] c, int cStart) {
        return (b[bStart + 0] - a[aStart + 0]) * (c[cStart + 2] - a[aStart + 2]) - (c[cStart + 0] - a[aStart + 0]) * (b[bStart + 2] - a[aStart + 2]);
    }

    //	Exclusive or: true iff exactly one argument is true.
    //	The arguments are negated to ensure that they are 0/1
    //	values.  Then the bitwise Xor operator may apply.
    //	(This idea is due to Michael Baldwin.)
    public static bool xorb(bool x, bool y) {
        return !x ^ !y;
    }

    // Returns true iff c is strictly to the left of the directed
    // line through a to b.
    public static bool left(int[] a, int[] b, int[] c) {
        return area2(a, b, c) < 0;
    }
    public static bool left(int[] a, int aStart, int[] b, int bStart, int[] c, int cStart) {
        return area2(a, aStart, b, bStart, c, cStart) < 0;
    }

    public static bool leftOn(int[] a, int[] b, int[] c) {
        return area2(a, b, c) <= 0;
    }
    public static bool leftOn(int[] a, int aStart, int[] b, int bStart, int[] c, int cStart) {
        return area2(a, aStart, b, bStart, c, cStart) <= 0;
    }

    public static bool collinear(int[] a, int[] b, int[] c) {
        return area2(a, b, c) == 0;
    }
    public static bool collinear(int[] a, int aStart, int[] b, int bStart, int[] c, int cStart) {
        return area2(a, aStart, b, bStart, c, cStart) == 0;
    }

    //	Returns true iff ab properly intersects cd: they share
    //	a point interior to both segments.  The properness of the
    //	intersection is ensured by using strict leftness.
    public static bool intersectProp(int[] a, int[] b, int[] c, int[] d) {
        // Eliminate improper cases.
        if (collinear(a, b, c) || collinear(a, b, d) ||
            collinear(c, d, a) || collinear(c, d, b))
            return false;

        return xorb(left(a, b, c), left(a, b, d)) && xorb(left(c, d, a), left(c, d, b));
    }
    public static bool intersectProp(int[] a, int aStart, int[] b, int bStart, int[] c, int cStart, int[] d, int dStart) {
        // Eliminate improper cases.
        if (collinear(a, aStart, b, bStart, c, cStart) || collinear(a, aStart, b, bStart, d, dStart) ||
            collinear(c, cStart, d, dStart, a, aStart) || collinear(c, cStart, d, dStart, b, bStart))
            return false;

        return xorb(left(a, aStart, b, bStart, c, cStart), left(a, aStart, b, bStart, d, dStart)) && xorb(left(c, cStart, d, dStart, a, aStart), left(c, cStart, d, dStart, b, bStart));
    }

    // Returns T iff (a,b,c) are collinear and point c lies 
    // on the closed segement ab.
    public static bool between(int[] a, int[] b, int[] c) {
        if (!collinear(a, b, c))
            return false;
        // If ab not vertical, check betweenness on x; else on y.
        if (a[0] != b[0])
            return ((a[0] <= c[0]) && (c[0] <= b[0])) || ((a[0] >= c[0]) && (c[0] >= b[0]));
        else
            return ((a[2] <= c[2]) && (c[2] <= b[2])) || ((a[2] >= c[2]) && (c[2] >= b[2]));
    }
    public static bool between(int[] a, int aStart, int[] b, int bStart, int[] c, int cStart) {
        if (!collinear(a, aStart, b, bStart, c, cStart))
            return false;
        // If ab not vertical, check betweenness on x; else on y.
        if (a[aStart+0] != b[bStart+0])
            return ((a[aStart+0] <= c[cStart+0]) && (c[cStart+0] <= b[bStart+0])) || ((a[aStart+0] >= c[cStart+0]) && (c[cStart+0] >= b[bStart+0]));
        else
            return ((a[aStart+2] <= c[cStart+2]) && (c[cStart+2] <= b[bStart+2])) || ((a[aStart+2] >= c[cStart+2]) && (c[cStart+2] >= b[bStart+2]));
    }


    // Returns true iff segments ab and cd intersect, properly or improperly.
    public static bool intersect(int[] a, int[] b, int[] c, int[] d) {
        if (intersectProp(a, b, c, d))
            return true;
        else if (between(a, b, c) || between(a, b, d) ||
                 between(c, d, a) || between(c, d, b))
            return true;
        else
            return false;
    }
    public static bool intersect(int[] a, int aStart, int[] b, int bStart, int[] c, int cStart, int[] d, int dStart) {
        if (intersectProp(a, aStart, b, bStart, c, cStart, d, dStart))
            return true;
        else if (between(a, aStart, b, bStart, c, cStart) || between(a, aStart, b, bStart, d, dStart) ||
                 between(c, cStart, d, dStart, a, aStart) || between(c, cStart, d, dStart, b, bStart))
            return true;
        else
            return false;
    }

    public static bool vequal(int[] a, int[] b) {
        return a[0] == b[0] && a[2] == b[2];
    }
    public static bool vequal(int[] a, int aStart, int[] b, int bStart) {
        return a[aStart + 0] == b[bStart + 0] && a[aStart + 2] == b[bStart + 2];
    }


    // Returns T iff (v_i, v_j) is a proper internal *or* external
    // diagonal of P, *ignoring edges incident to v_i and v_j*.
    public static bool diagonalie(int i, int j, int n, int[] verts, int[] indices) {
        //int* d0 = &verts[(indices[i] & 0x0fffffff) * 4];
        //int* d1 = &verts[(indices[j] & 0x0fffffff) * 4];
        int d0Start = (indices[i] & 0x0fffffff) * 4;
        int d1Start = (indices[j] & 0x0fffffff) * 4;

        // For each edge (k,k+1) of P
        for (int k = 0; k < n; k++) {
            int k1 = next(k, n);
            // Skip edges incident to i or j
            if (!((k == i) || (k1 == i) || (k == j) || (k1 == j))) {
                int p0Start = (indices[k] & 0x0fffffff) * 4;
                int p1Start = (indices[k1] & 0x0fffffff) * 4;

                if (vequal(verts, d0Start, verts, p0Start) || vequal(verts,d1Start, verts, p0Start) || vequal(verts, d0Start, verts, p1Start) || vequal(verts, d1Start, verts, p1Start))
                    continue;

                if (intersect(verts, d0Start,verts, d1Start,verts, p0Start, verts, p1Start))
                    return false;
            }
        }
        return true;
    }

    // Returns true iff the diagonal (i,j) is strictly internal to the 
    // polygon P in the neighborhood of the i endpoint.
    public static bool inCone(int i, int j, int n, int[] verts, int[] indices) {
        int piStart = (indices[i] & 0x0fffffff) * 4;
        int pjStart = (indices[j] & 0x0fffffff) * 4;
        int pi1Start = (indices[next(i, n)] & 0x0fffffff) * 4;
        int pin1Start = (indices[prev(i, n)] & 0x0fffffff) * 4;

        // If P[i] is a convex vertex [ i+1 left or on (i-1,i) ].
        if (leftOn(verts, pin1Start,verts, piStart,verts, pi1Start))
            return left(verts,piStart,verts, pjStart,verts, pin1Start) && left(verts,pjStart,verts, piStart,verts, pi1Start);
        // Assume (i-1,i,i+1) not collinear.
        // else P[i] is reflex.
        return !(leftOn(verts,piStart,verts, pjStart,verts, pi1Start) && leftOn(verts, pjStart,verts, piStart, verts, pin1Start));
    }

    // Returns T iff (v_i, v_j) is a proper internal
    // diagonal of P.
    public static bool diagonal(int i, int j, int n, int[] verts, int[] indices) {
        return inCone(i, j, n, verts, indices) && diagonalie(i, j, n, verts, indices);
    }

    public static int triangulate(int n, int[] verts, int[] indices, int[] tris) {
        int ntris = 0;
        //int* dst = tris;
        //int[] dst = tris;
        int dstIndex = 0;

        int removeVertexFlag = 0;
        unchecked {
            removeVertexFlag = (int)0x80000000;
        }

        // The last bit of the index is used to indicate if the vertex can be removed.
        for (int i = 0; i < n; i++) {
            int _i1 = next(i, n);
            int _i2 = next(_i1, n);
            if (diagonal(i, _i2, n, verts, indices)) {
                unchecked {
                    indices[_i1] |= removeVertexFlag;
                }
            }
        }

        while (n > 3) {
            int minLen = -1;
            int mini = -1;
            for (int i = 0; i < n; i++) {
                int _i1 = next(i, n);
                if ((indices[_i1] & removeVertexFlag) != 0) {
                    int p0Start = (indices[i] & 0x0fffffff) * 4;
                    int p2Start = (indices[next(_i1, n)] & 0x0fffffff) * 4;
                    int dx = verts[p2Start+0] - verts[p0Start+0];
                    int dy = verts[p2Start+2] - verts[p0Start+2];
                    int len = dx * dx + dy * dy;

                    if (minLen < 0 || len < minLen) {
                        minLen = len;
                        mini = i;
                    }
                }
            }

            if (mini == -1) {
                // Should not happen.
                /*			printf("mini == -1 ntris=%d n=%d\n", ntris, n);
                            for (int i = 0; i < n; i++)
                            {
                                printf("%d ", indices[i] & 0x0fffffff);
                            }
                            printf("\n");*/
                return -ntris;
            }

            int i0 = mini;
            int i1 = next(i0, n);
            int i2 = next(i1, n);

            tris[dstIndex] = indices[i0] & 0x0fffffff;
            ++dstIndex;
            tris[dstIndex] = indices[i1] & 0x0fffffff;
            ++dstIndex;
            tris[dstIndex] = indices[i2] & 0x0fffffff;
            ++dstIndex;

            ntris++;

            // Removes P[i1] by copying P[i+1]...P[n-1] left one index.
            n--;
            for (int k = i1; k < n; k++)
                indices[k] = indices[k + 1];

            if (i1 >= n) i1 = 0;
            i0 = prev(i1, n);
            // Update diagonal flags.
            if (diagonal(prev(i0, n), i1, n, verts, indices))
                indices[i0] |= removeVertexFlag;
            else
                indices[i0] &= 0x0fffffff;

            if (diagonal(i0, next(i1, n), n, verts, indices))
                indices[i1] |= removeVertexFlag;
            else
                indices[i1] &= 0x0fffffff;
        }

        // Append the remaining triangle.
        tris[dstIndex] = indices[0] & 0x0fffffff;
        ++dstIndex;
        tris[dstIndex] = indices[1] & 0x0fffffff;
        ++dstIndex;
        tris[dstIndex] = indices[2] & 0x0fffffff;
        ++dstIndex;
        ntris++;

        return ntris;
    }

    public static int countPolyVerts(ushort[] p, int pStart, int nvp) {
        for (int i = 0; i < nvp; ++i) {
            if (p[pStart + i] == RC_MESH_NULL_IDX) {
                return i;
            }
        }
        return nvp;
    }

    public static bool uleft(ushort[] a, ushort[] b, ushort[] c) {
        return ((int)b[0] - (int)a[0]) * ((int)c[2] - (int)a[2]) -
               ((int)c[0] - (int)a[0]) * ((int)b[2] - (int)a[2]) < 0;
    }

    public static bool uleft(ushort[] a, int aStart, ushort[] b, int bStart, ushort[] c, int cStart) {
        return ((int)b[bStart + 0] - (int)a[aStart + 0]) * ((int)c[cStart + 2] - (int)a[aStart + 2]) -
               ((int)c[cStart + 0] - (int)a[aStart + 0]) * ((int)b[bStart + 2] - (int)a[aStart + 2]) < 0;
    }

    public static int getPolyMergeValue(ushort[] pa, int paStart, ushort[] pb, int pbStart,
                                 ushort[] verts, ref int ea, ref int eb,
                                 int nvp) {
        int na = countPolyVerts(pa, paStart, nvp);
        int nb = countPolyVerts(pb, pbStart, nvp);

        // If the merged polygon would be too big, do not merge.
        if (na + nb - 2 > nvp)
            return -1;

        // Check if the polygons share an edge.
        ea = -1;
        eb = -1;

        for (int i = 0; i < na; ++i) {
            ushort va0 = pa[paStart + i];
            ushort va1 = pa[paStart + ((i + 1) % na)];
            if (va0 > va1) {
                rcSwap(ref va0, ref va1);
            }
            for (int j = 0; j < nb; ++j) {
                ushort vb0 = pb[pbStart + j];
                ushort vb1 = pb[pbStart + ((j + 1) % nb)];
                if (vb0 > vb1)
                    rcSwap(ref vb0, ref vb1);
                if (va0 == vb0 && va1 == vb1) {
                    ea = i;
                    eb = j;
                    break;
                }
            }
        }

        // No common edge, cannot merge.
        if (ea == -1 || eb == -1)
            return -1;

        // Check to see if the merged polygon would be convex.
        ushort va, vb, vc;

        va = pa[paStart + ((ea + na - 1) % na)];
        vb = pa[paStart + ea];
        vc = pb[pbStart + ((eb + 2) % nb)];
        if (!uleft(verts, va * 3, verts, vb * 3, verts, vc * 3))
            return -1;

        va = pb[pbStart + ((eb + nb - 1) % nb)];
        vb = pb[pbStart + eb];
        vc = pa[paStart + ((ea + 2) % na)];
        if (!uleft(verts, va * 3, verts, vb * 3, verts, vc * 3))
            return -1;

        va = pa[paStart + ea];
        vb = pa[paStart + ((ea + 1) % na)];

        int dx = (int)verts[va * 3 + 0] - (int)verts[vb * 3 + 0];
        int dy = (int)verts[va * 3 + 2] - (int)verts[vb * 3 + 2];

        return dx * dx + dy * dy;
    }

    public static void mergePolys(ushort[] pa, int paStart, ushort[] pb, int pbStart, int ea, int eb,
                           ushort[] tmp, int tmpStart, int nvp) {
        int na = countPolyVerts(pa, paStart, nvp);
        int nb = countPolyVerts(pb, pbStart, nvp);

        // Merge polygons.
        //memset(tmp, 0xff, sizeof(ushort)*nvp);
        for (int i = 0; i < nvp; ++i) {
            tmp[tmpStart + i] = 0xffff;
        }
        int n = 0;
        // Add pa
        for (int i = 0; i < na - 1; ++i)
            tmp[tmpStart + n++] = pa[paStart + ((ea + 1 + i) % na)];
        // Add pb
        for (int i = 0; i < nb - 1; ++i)
            tmp[tmpStart + n++] = pb[pbStart + ((eb + 1 + i) % nb)];

        //memcpy(pa, tmp, sizeof(ushort)*nvp);
        for (int i = 0; i < nvp; ++i) {
            pa[paStart + i] = tmp[tmpStart + i];
        }
    }


    public static void pushFront(int v, int[] arr, ref int an) {
        an++;
        for (int i = an - 1; i > 0; --i) {
            arr[i] = arr[i - 1];
        }
        arr[0] = v;
    }

    public static void pushBack(int v, int[] arr, ref int an) {
        arr[an] = v;
        an++;
    }

    public static bool canRemoveVertex(rcContext ctx, rcPolyMesh mesh, ushort rem) {
        int nvp = mesh.nvp;

        // Count number of polygons to remove.
        int numRemovedVerts = 0;
        int numTouchedVerts = 0;
        int numRemainingEdges = 0;
        for (int i = 0; i < mesh.npolys; ++i) {
            //ushort* p = &mesh.polys[i*nvp*2];
            int pIndex = i * nvp * 2;
            int nv = countPolyVerts(mesh.polys, i * nvp * 2, nvp);
            int numRemoved = 0;
            int numVerts = 0;
            for (int j = 0; j < nv; ++j) {
                if (mesh.polys[pIndex + j] == rem) {
                    numTouchedVerts++;
                    numRemoved++;
                }
                numVerts++;
            }
            if (numRemoved != 0) {
                numRemovedVerts += numRemoved;
                numRemainingEdges += numVerts - (numRemoved + 1);
            }
        }

        // There would be too few edges remaining to create a polygon.
        // This can happen for example when a tip of a triangle is marked
        // as deletion, but there are no other polys that share the vertex.
        // In this case, the vertex should not be removed.
        if (numRemainingEdges <= 2)
            return false;

        // Find edges which share the removed vertex.
        int maxEdges = numTouchedVerts * 2;
        int nedges = 0;
        //rcScopedDelete<int> edges = (int*)rcAlloc(sizeof(int)*maxEdges*3, RC_ALLOC_TEMP);
        int[] edges = new int[maxEdges * 3];
        if (edges == null) {
            ctx.log(rcLogCategory.RC_LOG_WARNING, "canRemoveVertex: Out of memory 'edges' " + maxEdges * 3);
            return false;
        }

        for (int i = 0; i < mesh.npolys; ++i) {
            //ushort* p = &mesh.polys[i*nvp*2];
            int pIndex = i * nvp * 2;
            int nv = countPolyVerts(mesh.polys, pIndex, nvp);

            // Collect edges which touches the removed vertex.
            for (int j = 0, k = nv - 1; j < nv; k = j++) {
                if (mesh.polys[pIndex + j] == rem || mesh.polys[pIndex + k] == rem) {
                    // Arrange edge so that a=rem.
                    int a = mesh.polys[pIndex + j], b = mesh.polys[pIndex + k];
                    if (b == rem) {
                        rcSwap(ref a, ref b);
                    }

                    // Check if the edge exists
                    bool exists = false;
                    for (int m = 0; m < nedges; ++m) {
                        //int* e = &edges[m*3];
                        int eIndex = m * 3;
                        if (edges[eIndex + 1] == b) {
                            // Exists, increment vertex share count.
                            edges[eIndex + 2]++;
                            exists = true;
                        }
                    }
                    // Add new edge.
                    if (!exists) {
                        //int* e = &edges[nedges*3];
                        int eIndex = nedges * 3;
                        edges[eIndex + 0] = a;
                        edges[eIndex + 1] = b;
                        edges[eIndex + 2] = 1;
                        nedges++;
                    }
                }
            }
        }

        // There should be no more than 2 open edges.
        // This catches the case that two non-adjacent polygons
        // share the removed vertex. In that case, do not remove the vertex.
        int numOpenEdges = 0;
        for (int i = 0; i < nedges; ++i) {
            if (edges[i * 3 + 2] < 2)
                numOpenEdges++;
        }
        if (numOpenEdges > 2)
            return false;

        return true;
    }

    public static bool removeVertex(rcContext ctx, rcPolyMesh mesh, ushort rem, int maxTris) {
        int nvp = mesh.nvp;

        // Count number of polygons to remove.
        int numRemovedVerts = 0;
        for (int i = 0; i < mesh.npolys; ++i) {
            //ushort* p = &mesh.polys[i*nvp*2];
            int pIndex = i * nvp * 2;
            int nv = countPolyVerts(mesh.polys, pIndex, nvp);
            for (int j = 0; j < nv; ++j) {
                if (mesh.polys[pIndex + j] == rem)
                    numRemovedVerts++;
            }
        }

        int nedges = 0;
        //rcScopedDelete<int> edges = (int*)rcAlloc(sizeof(int)*numRemovedVerts*nvp*4, RC_ALLOC_TEMP);
        int[] edges = new int[numRemovedVerts * nvp * 4];
        if (edges == null) {
            ctx.log(rcLogCategory.RC_LOG_WARNING, "removeVertex: Out of memory 'edges' " + numRemovedVerts * nvp * 4);
            return false;
        }

        int nhole = 0;
        //rcScopedDelete<int> hole = (int*)rcAlloc(sizeof(int)*numRemovedVerts*nvp, RC_ALLOC_TEMP);
        int[] hole = new int[numRemovedVerts * nvp];
        if (hole == null) {
            ctx.log(rcLogCategory.RC_LOG_WARNING, "removeVertex: Out of memory 'hole' " + numRemovedVerts * nvp);
            return false;
        }

        int nhreg = 0;
        //rcScopedDelete<int> hreg = (int*)rcAlloc(sizeof(int)*numRemovedVerts*nvp, RC_ALLOC_TEMP);
        int[] hreg = new int[numRemovedVerts * nvp];
        if (hreg == null) {
            ctx.log(rcLogCategory.RC_LOG_WARNING, "removeVertex: Out of memory 'hreg' " + numRemovedVerts * nvp);
            return false;
        }

        int nharea = 0;
        //rcScopedDelete<int> harea = (int*)rcAlloc(sizeof(int)*numRemovedVerts*nvp, RC_ALLOC_TEMP);
        int[] harea = new int[numRemovedVerts * nvp];
        if (harea == null) {
            ctx.log(rcLogCategory.RC_LOG_WARNING, "removeVertex: Out of memory 'harea' " + numRemovedVerts * nvp);
            return false;
        }

        for (int i = 0; i < mesh.npolys; ++i) {
            //ushort* p = &mesh.polys[i*nvp*2];
            int pIndex = i * nvp * 2;
            int nv = countPolyVerts(mesh.polys, pIndex, nvp);
            bool hasRem = false;
            for (int j = 0; j < nv; ++j)
                if (mesh.polys[pIndex + j] == rem) hasRem = true;
            if (hasRem) {
                // Collect edges which does not touch the removed vertex.
                for (int j = 0, k = nv - 1; j < nv; k = j++) {
                    if (mesh.polys[pIndex + j] != rem && mesh.polys[pIndex + k] != rem) {
                        //int[] e = &edges[nedges*4];
                        int eIndex = nedges * 4;
                        edges[eIndex + 0] = mesh.polys[pIndex + k];
                        edges[eIndex + 1] = mesh.polys[pIndex + j];
                        edges[eIndex + 2] = mesh.regs[i];
                        edges[eIndex + 3] = mesh.areas[i];
                        nedges++;
                    }
                }
                // Remove the polygon.
                //ushort* p2 = &mesh.polys[(mesh.npolys-1)*nvp*2];
                int p2Index = (mesh.npolys - 1) * nvp * 2;
                if (mesh.polys[pIndex] != mesh.polys[p2Index]) {
                    //memcpy(p,p2,sizeof(ushort)*nvp);
                    for (int j = 0; j < nvp; ++j) {
                        mesh.polys[pIndex + j] = mesh.polys[p2Index + j];
                    }
                }
                //memset(p+nvp,0xff,sizeof(ushort)*nvp);
                for (int j = 0; j < nvp; ++j) {
                    mesh.polys[pIndex + nvp + j] = 0xffff;
                }
                mesh.regs[i] = mesh.regs[mesh.npolys - 1];
                mesh.areas[i] = mesh.areas[mesh.npolys - 1];
                mesh.npolys--;
                --i;
            }
        }

        // Remove vertex.
        for (int i = (int)rem; i < mesh.nverts; ++i) {
            mesh.verts[i * 3 + 0] = mesh.verts[(i + 1) * 3 + 0];
            mesh.verts[i * 3 + 1] = mesh.verts[(i + 1) * 3 + 1];
            mesh.verts[i * 3 + 2] = mesh.verts[(i + 1) * 3 + 2];
        }
        mesh.nverts--;

        // Adjust indices to match the removed vertex layout.
        for (int i = 0; i < mesh.npolys; ++i) {
            //ushort* p = &mesh.polys[i*nvp*2];
            int pIndex = i * nvp * 2;
            int nv = countPolyVerts(mesh.polys, i * nvp * 2, nvp);
            for (int j = 0; j < nv; ++j) {
                if (mesh.polys[pIndex + j] > rem) {
                    mesh.polys[pIndex + j]--;
                }
            }
        }
        for (int i = 0; i < nedges; ++i) {
            if (edges[i * 4 + 0] > rem) {
                edges[i * 4 + 0]--;
            }
            if (edges[i * 4 + 1] > rem) {
                edges[i * 4 + 1]--;
            }
        }

        if (nedges == 0) {
            return true;
        }

        // Start with one vertex, keep appending connected
        // segments to the start and end of the hole.
        pushBack(edges[0], hole, ref nhole);
        pushBack(edges[2], hreg, ref nhreg);
        pushBack(edges[3], harea, ref nharea);

        while (nedges != 0) {
            bool match = false;

            for (int i = 0; i < nedges; ++i) {
                int ea = edges[i * 4 + 0];
                int eb = edges[i * 4 + 1];
                int r = edges[i * 4 + 2];
                int a = edges[i * 4 + 3];
                bool add = false;
                if (hole[0] == eb) {
                    // The segment matches the beginning of the hole boundary.
                    pushFront(ea, hole, ref nhole);
                    pushFront(r, hreg, ref nhreg);
                    pushFront(a, harea, ref nharea);
                    add = true;
                } else if (hole[nhole - 1] == ea) {
                    // The segment matches the end of the hole boundary.
                    pushBack(eb, hole, ref nhole);
                    pushBack(r, hreg, ref nhreg);
                    pushBack(a, harea, ref nharea);
                    add = true;
                }
                if (add) {
                    // The edge segment was added, remove it.
                    edges[i * 4 + 0] = edges[(nedges - 1) * 4 + 0];
                    edges[i * 4 + 1] = edges[(nedges - 1) * 4 + 1];
                    edges[i * 4 + 2] = edges[(nedges - 1) * 4 + 2];
                    edges[i * 4 + 3] = edges[(nedges - 1) * 4 + 3];
                    --nedges;
                    match = true;
                    --i;
                }
            }

            if (!match)
                break;
        }

        //rcScopedDelete<int> tris = (int*)rcAlloc(sizeof(int)*nhole*3, RC_ALLOC_TEMP);
        int[] tris = new int[nhole * 3];
        if (tris == null) {
            ctx.log(rcLogCategory.RC_LOG_WARNING, "removeVertex: Out of memory 'tris' " + nhole * 3);
            return false;
        }

        //rcScopedDelete<int> tverts = (int*)rcAlloc(sizeof(int)*nhole*4, RC_ALLOC_TEMP);
        int[] tverts = new int[nhole * 4];
        if (tverts == null) {
            ctx.log(rcLogCategory.RC_LOG_WARNING, "removeVertex: Out of memory 'tverts' " + nhole * 4);
            return false;
        }

        //rcScopedDelete<int> thole = (int*)rcAlloc(sizeof(int)*nhole, RC_ALLOC_TEMP);
        int[] thole = new int[nhole];
        if (tverts == null) {
            ctx.log(rcLogCategory.RC_LOG_WARNING, "removeVertex: Out of memory 'thole' " + nhole);
            return false;
        }

        // Generate temp vertex array for triangulation.
        for (int i = 0; i < nhole; ++i) {
            int pi = hole[i];
            tverts[i * 4 + 0] = mesh.verts[pi * 3 + 0];
            tverts[i * 4 + 1] = mesh.verts[pi * 3 + 1];
            tverts[i * 4 + 2] = mesh.verts[pi * 3 + 2];
            tverts[i * 4 + 3] = 0;
            thole[i] = i;
        }

        // Triangulate the hole.
        int ntris = triangulate(nhole, tverts, thole, tris);
        if (ntris < 0) {
            ntris = -ntris;
            ctx.log(rcLogCategory.RC_LOG_WARNING, "removeVertex: triangulate() returned bad results.");
        }

        // Merge the hole triangles back to polygons.
        //rcScopedDelete<ushort> polys = (ushort*)rcAlloc(sizeof(ushort)*(ntris+1)*nvp, RC_ALLOC_TEMP);
        ushort[] polys = new ushort[(ntris + 1) * nvp];
        if (polys == null) {
            ctx.log(rcLogCategory.RC_LOG_ERROR, "removeVertex: Out of memory 'polys' " + (ntris + 1) * nvp);
            return false;
        }
        //rcScopedDelete<ushort> pregs = (ushort*)rcAlloc(sizeof(ushort)*ntris, RC_ALLOC_TEMP);
        ushort[] pregs = new ushort[ntris];
        if (pregs == null) {
            ctx.log(rcLogCategory.RC_LOG_ERROR, "removeVertex: Out of memory 'pregs' " + ntris);
            return false;
        }
        //rcScopedDelete<byte> pareas = (byte*)rcAlloc(sizeof(byte)*ntris, RC_ALLOC_TEMP);
        byte[] pareas = new byte[ntris];
        if (pregs == null) {
            ctx.log(rcLogCategory.RC_LOG_ERROR, "removeVertex: Out of memory 'pareas' " + ntris);
            return false;
        }

        int tmpPolyIndex = ntris * nvp;
        //ushort* tmpPoly = &polys[ntris*nvp];

        // Build initial polygons.
        int npolys = 0;
        //memset(polys, 0xff, ntris*nvp*sizeof(ushort));
        for (int i = 0; i < ntris * nvp; ++i) {
            polys[i] = 0xffff;
        }
        for (int j = 0; j < ntris; ++j) {
            //int* t = &tris[j*3];
            int tIndex = j * 3;
            if (tris[tIndex + 0] != tris[tIndex + 1] && tris[tIndex + 0] != tris[tIndex + 2] && tris[tIndex + 1] != tris[tIndex + 2]) {
                polys[npolys * nvp + 0] = (ushort)hole[tris[tIndex + 0]];
                polys[npolys * nvp + 1] = (ushort)hole[tris[tIndex + 1]];
                polys[npolys * nvp + 2] = (ushort)hole[tris[tIndex + 2]];
                pregs[npolys] = (ushort)hreg[tris[tIndex + 0]];
                pareas[npolys] = (byte)harea[tris[tIndex + 0]];
                npolys++;
            }
        }
        if (npolys == 0) {
            return true;
        }

        // Merge polygons.
        if (nvp > 3) {
            for (; ; ) {
                // Find best polygons to merge.
                int bestMergeVal = 0;
                int bestPa = 0, bestPb = 0, bestEa = 0, bestEb = 0;

                for (int j = 0; j < npolys - 1; ++j) {
                    int pjIndex = j * nvp;
                    //ushort* pj = &polys[j*nvp];
                    for (int k = j + 1; k < npolys; ++k) {
                        int pkIndex = k * nvp;
                        //ushort* pk = &polys[k*nvp];
                        int ea = 0;
                        int eb = 0;
                        int v = getPolyMergeValue(polys, pjIndex, polys, pkIndex, mesh.verts, ref ea, ref eb, nvp);
                        if (v > bestMergeVal) {
                            bestMergeVal = v;
                            bestPa = j;
                            bestPb = k;
                            bestEa = ea;
                            bestEb = eb;
                        }
                    }
                }

                if (bestMergeVal > 0) {
                    // Found best, merge.

                    //ushort* pa = &polys[bestPa*nvp];
                    //ushort* pb = &polys[bestPb*nvp];
                    int paIndex = bestPa * nvp;
                    int pbIndex = bestPb * nvp;
                    mergePolys(polys, paIndex, polys, pbIndex, bestEa, bestEb, polys, tmpPolyIndex, nvp);
                    //ushort* last = &polys[(npolys-1)*nvp];
                    int lastIndex = (npolys - 1) * nvp;
                    if (polys[pbIndex] != polys[lastIndex]) {
                        //memcpy(pb, last, sizeof(ushort)*nvp);
                        for (int j = 0; j < nvp; ++j) {
                            polys[pbIndex + j] = polys[lastIndex + j];
                        }
                    }
                    pregs[bestPb] = pregs[npolys - 1];
                    pareas[bestPb] = pareas[npolys - 1];
                    npolys--;
                } else {
                    // Could not merge any polygons, stop.
                    break;
                }
            }
        }

        // Store polygons.
        for (int i = 0; i < npolys; ++i) {
            if (mesh.npolys >= maxTris) break;
            //ushort* p = &mesh.polys[mesh.npolys*nvp*2];
            int pIndex = mesh.npolys * nvp * 2;
            for (int j = 0; j < nvp * 2; ++j) {
                polys[pIndex + j] = 0xffff;
            }
            //memset(p,0xff,sizeof(ushort)*nvp*2);
            for (int j = 0; j < nvp; ++j) {
                polys[pIndex + j] = polys[i * nvp + j];
            }
            mesh.regs[mesh.npolys] = pregs[i];
            mesh.areas[mesh.npolys] = pareas[i];
            mesh.npolys++;
            if (mesh.npolys > maxTris) {
                ctx.log(rcLogCategory.RC_LOG_ERROR, "removeVertex: Too many polygons " + mesh.npolys + " (max:" + maxTris + ")");
                return false;
            }
        }

        return true;
    }

    /// @par
    ///
    /// @note If the mesh data is to be used to construct a Detour navigation mesh, then the upper 
    /// limit must be retricted to <= #DT_VERTS_PER_POLYGON.
    ///
    /// @see rcAllocPolyMesh, rcContourSet, rcPolyMesh, rcConfig
    public static bool rcBuildPolyMesh(rcContext ctx, rcContourSet cset, int nvp, rcPolyMesh mesh) {
        Debug.Assert(ctx != null, "rcContext is null");

        ctx.startTimer(rcTimerLabel.RC_TIMER_BUILD_POLYMESH);

        rcVcopy(mesh.bmin, cset.bmin);
        rcVcopy(mesh.bmax, cset.bmax);
        mesh.cs = cset.cs;
        mesh.ch = cset.ch;
        mesh.borderSize = cset.borderSize;

        int maxVertices = 0;
        int maxTris = 0;
        int maxVertsPerCont = 0;
        for (int i = 0; i < cset.nconts; ++i) {
            // Skip null contours.
            if (cset.conts[i].nverts < 3) continue;
            maxVertices += cset.conts[i].nverts;
            maxTris += cset.conts[i].nverts - 2;
            maxVertsPerCont = Math.Max(maxVertsPerCont, cset.conts[i].nverts);
        }

        if (maxVertices >= 0xfffe) {
            ctx.log(rcLogCategory.RC_LOG_ERROR, "rcBuildPolyMesh: Too many vertices " + maxVertices);
            return false;
        }

        //rcScopedDelete<byte> vflags = (byte*)rcAlloc(sizeof(byte)*maxVertices, RC_ALLOC_TEMP);
        byte[] vflags = new byte[maxVertices];
        if (vflags == null) {
            ctx.log(rcLogCategory.RC_LOG_ERROR, "rcBuildPolyMesh: Out of memory 'vflags' " + maxVertices);
            return false;
        }
        //memset(vflags, 0, maxVertices);

        //mesh.verts = (ushort*)rcAlloc(sizeof(ushort)*maxVertices*3, RC_ALLOC_PERM);
        mesh.verts = new ushort[maxVertices * 3];
        if (mesh.verts == null) {
            ctx.log(rcLogCategory.RC_LOG_ERROR, "rcBuildPolyMesh: Out of memory 'mesh.verts' " + maxVertices);
            return false;
        }
        //mesh.polys = (ushort*)rcAlloc(sizeof(ushort)*maxTris*nvp*2, RC_ALLOC_PERM);
        mesh.polys = new ushort[maxTris * nvp * 2];
        if (mesh.polys == null) {
            ctx.log(rcLogCategory.RC_LOG_ERROR, "rcBuildPolyMesh: Out of memory 'mesh.polys' " + maxTris * nvp * 2);
            return false;
        }
        //mesh.regs = (ushort*)rcAlloc(sizeof(ushort)*maxTris, RC_ALLOC_PERM);
        mesh.regs = new ushort[maxTris];
        if (mesh.regs == null) {
            ctx.log(rcLogCategory.RC_LOG_ERROR, "rcBuildPolyMesh: Out of memory 'mesh.regs' " + maxTris);
            return false;
        }
        //mesh.areas = (byte*)rcAlloc(sizeof(byte)*maxTris, RC_ALLOC_PERM);
        mesh.areas = new byte[maxTris];
        if (mesh.areas == null) {
            ctx.log(rcLogCategory.RC_LOG_ERROR, "rcBuildPolyMesh: Out of memory 'mesh.areas' " + maxTris);
            return false;
        }

        mesh.nverts = 0;
        mesh.npolys = 0;
        mesh.nvp = nvp;
        mesh.maxpolys = maxTris;

        //memset(mesh.verts, 0, sizeof(ushort)*maxVertices*3);
        //memset(mesh.polys, 0xff, sizeof(ushort)*maxTris*nvp*2);
        for (int i = 0; i < maxTris * nvp * 2; ++i) {
            mesh.polys[i] = 0xffff;
        }
        //memset(mesh.regs, 0, sizeof(ushort)*maxTris);
        //memset(mesh.areas, 0, sizeof(byte)*maxTris);

        //rcScopedDelete<int> nextVert = (int*)rcAlloc(sizeof(int)*maxVertices, RC_ALLOC_TEMP);
        int[] nextVert = new int[maxVertices];
        if (nextVert == null) {
            ctx.log(rcLogCategory.RC_LOG_ERROR, "rcBuildPolyMesh: Out of memory 'nextVert' " + maxVertices);
            return false;
        }
        //memset(nextVert, 0, sizeof(int)*maxVertices);

        //rcScopedDelete<int> firstVert = (int*)rcAlloc(sizeof(int)*VERTEX_BUCKET_COUNT, RC_ALLOC_TEMP);
        int[] firstVert = new int[VERTEX_BUCKET_COUNT];
        if (firstVert == null) {
            ctx.log(rcLogCategory.RC_LOG_ERROR, "rcBuildPolyMesh: Out of memory 'firstVert' " + VERTEX_BUCKET_COUNT);
            return false;
        }
        for (int i = 0; i < VERTEX_BUCKET_COUNT; ++i)
            firstVert[i] = -1;

        //rcScopedDelete<int> indices = (int*)rcAlloc(sizeof(int)*maxVertsPerCont, RC_ALLOC_TEMP);
        int[] indices = new int[maxVertsPerCont];
        if (indices == null) {
            ctx.log(rcLogCategory.RC_LOG_ERROR, "rcBuildPolyMesh: Out of memory 'indices' " + maxVertsPerCont);
            return false;
        }
        //rcScopedDelete<int> tris = (int*)rcAlloc(sizeof(int)*maxVertsPerCont*3, RC_ALLOC_TEMP);
        int[] tris = new int[maxVertsPerCont * 3];
        if (tris == null) {
            ctx.log(rcLogCategory.RC_LOG_ERROR, "rcBuildPolyMesh: Out of memory 'tris' " + maxVertsPerCont * 3);
            return false;
        }
        //rcScopedDelete<ushort> polys = (ushort*)rcAlloc(sizeof(ushort)*(maxVertsPerCont+1)*nvp, RC_ALLOC_TEMP);
        ushort[] polys = new ushort[(maxVertsPerCont + 1) * nvp];
        if (polys == null) {
            ctx.log(rcLogCategory.RC_LOG_ERROR, "rcBuildPolyMesh: Out of memory 'polys' " + (maxVertsPerCont + 1) * nvp);
            return false;
        }
        int tmpPolyIndex = maxVertsPerCont * nvp;
        //ushort[] tmpPoly = &polys[maxVertsPerCont*nvp];

        for (int i = 0; i < cset.nconts; ++i) {
            rcContour cont = cset.conts[i];

            // Skip null contours.
            if (cont.nverts < 3)
                continue;

            // Triangulate contour
            for (int j = 0; j < cont.nverts; ++j)
                indices[j] = j;

            int ntris = triangulate(cont.nverts, cont.verts, indices, tris);
            if (ntris <= 0) {
                // Bad triangulation, should not happen.
                /*			printf("\tconst float bmin[3] = {%ff,%ff,%ff};\n", cset.bmin[0], cset.bmin[1], cset.bmin[2]);
                            printf("\tconst float cs = %ff;\n", cset.cs);
                            printf("\tconst float ch = %ff;\n", cset.ch);
                            printf("\tconst int verts[] = {\n");
                            for (int k = 0; k < cont.nverts; ++k)
                            {
                                const int* v = &cont.verts[k*4];
                                printf("\t\t%d,%d,%d,%d,\n", v[0], v[1], v[2], v[3]);
                            }
                            printf("\t};\n\tconst int nverts = sizeof(verts)/(sizeof(int)*4);\n");*/
                ctx.log(rcLogCategory.RC_LOG_WARNING, "rcBuildPolyMesh: Bad triangulation Contour " + i);
                ntris = -ntris;
            }

            // Add and merge vertices.
            for (int j = 0; j < cont.nverts; ++j) {
                int vIndex = j * 4;
                //const int* v = &cont.verts[j*4];
                indices[j] = addVertex((ushort)cont.verts[vIndex + 0], (ushort)cont.verts[vIndex + 1], (ushort)cont.verts[vIndex + 2],
                                       mesh.verts, firstVert, nextVert, ref  mesh.nverts);
                if ((cont.verts[vIndex + 3] & RC_BORDER_VERTEX) != 0) {
                    // This vertex should be removed.
                    vflags[indices[j]] = 1;
                }
            }

            // Build initial polygons.
            int npolys = 0;
            //memset(polys, 0xff, maxVertsPerCont*nvp*sizeof(ushort));
            for (int j = 0; j < nvp * maxVertsPerCont; ++j) {
                polys[j] = 0xffff;
            }
            for (int j = 0; j < ntris; ++j) {
                int tIndex = j * 3;
                //int* t = &tris[j*3];
                if (tris[tIndex + 0] != tris[tIndex + 1] && tris[tIndex + 0] != tris[tIndex + 2] && tris[tIndex + 1] != tris[tIndex + 2]) {
                    polys[npolys * nvp + 0] = (ushort)indices[tris[tIndex + 0]];
                    polys[npolys * nvp + 1] = (ushort)indices[tris[tIndex + 1]];
                    polys[npolys * nvp + 2] = (ushort)indices[tris[tIndex + 2]];
                    npolys++;
                }
            }
            if (npolys == 0) {
                continue;
            }

            // Merge polygons.
            if (nvp > 3) {
                for (; ; ) {
                    // Find best polygons to merge.
                    int bestMergeVal = 0;
                    int bestPa = 0, bestPb = 0, bestEa = 0, bestEb = 0;

                    for (int j = 0; j < npolys - 1; ++j) {
                        int pjIndex = j * nvp;
                        //ushort* pj = &polys[j*nvp];
                        for (int k = j + 1; k < npolys; ++k) {
                            //ushort* pk = &polys[k*nvp];
                            int pkIndex = k * nvp;
                            int ea = 0, eb = 0;
                            int v = getPolyMergeValue(polys, pjIndex, polys, pkIndex, mesh.verts, ref ea, ref eb, nvp);
                            if (v > bestMergeVal) {
                                bestMergeVal = v;
                                bestPa = j;
                                bestPb = k;
                                bestEa = ea;
                                bestEb = eb;
                            }
                        }
                    }

                    if (bestMergeVal > 0) {
                        // Found best, merge.
                        //ushort* pa = &polys[bestPa*nvp];
                        //ushort* pb = &polys[bestPb*nvp];
                        int paIndex = bestPa * nvp;
                        int pbIndex = bestPb * nvp;
                        mergePolys(polys, paIndex, polys, pbIndex, bestEa, bestEb, polys, tmpPolyIndex, nvp);
                        //ushort* lastPoly = &polys[(npolys-1)*nvp];
                        int lastPolyIndex = (npolys - 1) * nvp;
                        if (pbIndex != lastPolyIndex) {
                            //memcpy(pb, lastPoly, sizeof(ushort)*nvp);
                            for (int j = 0; j < nvp; ++j) {
                                polys[pbIndex + j] = polys[lastPolyIndex + j];
                            }
                        }
                        npolys--;
                    } else {
                        // Could not merge any polygons, stop.
                        break;
                    }
                }
            }

            // Store polygons.
            for (int j = 0; j < npolys; ++j) {
                //ushort* p = &mesh.polys[mesh.npolys*nvp*2];
                //ushort* q = &polys[j*nvp];
                int pIndex = mesh.npolys * nvp * 2;
                int qIndex = j * nvp;
                for (int k = 0; k < nvp; ++k) {
                    mesh.polys[pIndex + k] = polys[qIndex + k];
                }
                mesh.regs[mesh.npolys] = cont.reg;
                mesh.areas[mesh.npolys] = cont.area;
                mesh.npolys++;
                if (mesh.npolys > maxTris) {
                    ctx.log(rcLogCategory.RC_LOG_ERROR, "rcBuildPolyMesh: Too many polygons " + mesh.npolys + " max " + maxTris);
                    return false;
                }
            }
        }


        // Remove edge vertices.
        for (int i = 0; i < mesh.nverts; ++i) {
            if (vflags[i] != 0) {
                if (!canRemoveVertex(ctx, mesh, (ushort)i)) {
                    continue;
                }
                if (!removeVertex(ctx, mesh, (ushort)i, maxTris)) {
                    // Failed to remove vertex
                    ctx.log(rcLogCategory.RC_LOG_ERROR, "rcBuildPolyMesh: Failed to remove edge vertex " + i);
                    return false;
                }
                // Remove vertex
                // Note: mesh.nverts is already decremented inside removeVertex()!
                // Fixup vertex flags
                for (int j = i; j < mesh.nverts; ++j)
                    vflags[j] = vflags[j + 1];
                --i;
            }
        }

        // Calculate adjacency.
        if (!buildMeshAdjacency(mesh.polys, mesh.npolys, mesh.nverts, nvp)) {
            ctx.log(rcLogCategory.RC_LOG_ERROR, "rcBuildPolyMesh: Adjacency failed.");
            return false;
        }

        // Find portal edges
        if (mesh.borderSize > 0) {
            int w = cset.width;
            int h = cset.height;
            for (int i = 0; i < mesh.npolys; ++i) {
                int pIndex = i * 2 * nvp;
                //ushort* p = &mesh.polys[i*2*nvp];
                for (int j = 0; j < nvp; ++j) {
                    if (mesh.polys[pIndex + j] == RC_MESH_NULL_IDX) {
                        break;
                    }
                    // Skip connected edges.
                    if (mesh.polys[pIndex + nvp + j] != RC_MESH_NULL_IDX) {
                        continue;
                    }
                    int nj = j + 1;
                    if (nj >= nvp || mesh.polys[pIndex + nj] == RC_MESH_NULL_IDX) nj = 0;
                    //ushort* va = &mesh.verts[mesh.polys[pIndex + j]*3];
                    //ushort* vb = &mesh.verts[mesh.polys[pIndex + nj]*3];
                    int vaIndex = mesh.polys[pIndex + j] * 3;
                    int vbIndex = mesh.polys[pIndex + nj] * 3;

                    if ((int)mesh.verts[vaIndex + 0] == 0 && (int)mesh.verts[vbIndex + 0] == 0)
                        mesh.polys[pIndex + nvp + j] = 0x8000 | 0;
                    else if ((int)mesh.verts[vaIndex + 2] == h && (int)mesh.verts[vbIndex + 2] == h)
                        mesh.polys[pIndex + nvp + j] = 0x8000 | 1;
                    else if ((int)mesh.verts[vaIndex + 0] == w && (int)mesh.verts[vbIndex + 0] == w)
                        mesh.polys[pIndex + nvp + j] = 0x8000 | 2;
                    else if ((int)mesh.verts[vaIndex + 2] == 0 && (int)mesh.verts[vbIndex + 2] == 0)
                        mesh.polys[pIndex + nvp + j] = 0x8000 | 3;
                }
            }
        }

        // Just allocate the mesh flags array. The user is resposible to fill it.
        //mesh.flags = (ushort*)rcAlloc(sizeof(ushort)*mesh.npolys, RC_ALLOC_PERM);
        mesh.flags = new ushort[mesh.npolys];
        if (mesh.flags == null) {
            ctx.log(rcLogCategory.RC_LOG_ERROR, "rcBuildPolyMesh: Out of memory 'mesh.flags' " + mesh.npolys);
            return false;
        }
        //memset(mesh.flags, 0, sizeof(ushort) * mesh.npolys);

        if (mesh.nverts > 0xffff) {
            ctx.log(rcLogCategory.RC_LOG_ERROR, "rcBuildPolyMesh: The resulting mesh has too many vertices " + mesh.nverts + "(max " + 0xffff + ") Data can be corrupted.");
        }
        if (mesh.npolys > 0xffff) {
            ctx.log(rcLogCategory.RC_LOG_ERROR, "rcBuildPolyMesh: The resulting mesh has too many polygons " + mesh.npolys + " (max " + 0xffff + "). Data can be corrupted.");
        }

        ctx.stopTimer(rcTimerLabel.RC_TIMER_BUILD_POLYMESH);

        return true;
    }

    /// @see rcAllocPolyMesh, rcPolyMesh
    public static bool rcMergePolyMeshes(rcContext ctx, ref rcPolyMesh[] meshes, int nmeshes, rcPolyMesh mesh) {
        Debug.Assert(ctx != null, "rcContext is null");

        if (nmeshes == 0 || meshes == null)
            return true;

        ctx.startTimer(rcTimerLabel.RC_TIMER_MERGE_POLYMESH);

        mesh.nvp = meshes[0].nvp;
        mesh.cs = meshes[0].cs;
        mesh.ch = meshes[0].ch;
        rcVcopy(mesh.bmin, meshes[0].bmin);
        rcVcopy(mesh.bmax, meshes[0].bmax);

        int maxVerts = 0;
        int maxPolys = 0;
        int maxVertsPerMesh = 0;
        for (int i = 0; i < nmeshes; ++i) {
            rcVmin(mesh.bmin, meshes[i].bmin);
            rcVmax(mesh.bmax, meshes[i].bmax);
            maxVertsPerMesh = Math.Max(maxVertsPerMesh, meshes[i].nverts);
            maxVerts += meshes[i].nverts;
            maxPolys += meshes[i].npolys;
        }

        mesh.nverts = 0;
        //mesh.verts = (ushort*)rcAlloc(sizeof(ushort)*maxVerts*3, RC_ALLOC_PERM);
        mesh.verts = new ushort[maxVerts * 3];
        if (mesh.verts == null) {
            ctx.log(rcLogCategory.RC_LOG_ERROR, "rcMergePolyMeshes: Out of memory 'mesh.verts' " + maxVerts * 3);
            return false;
        }

        mesh.npolys = 0;
        //mesh.polys = (ushort*)rcAlloc(sizeof(ushort)*maxPolys*2*mesh.nvp, RC_ALLOC_PERM);
        mesh.polys = new ushort[maxPolys * 2 * mesh.nvp];
        if (mesh.polys == null) {
            ctx.log(rcLogCategory.RC_LOG_ERROR, "rcMergePolyMeshes: Out of memory 'mesh.polys' " + maxPolys * 2 * mesh.nvp);
            return false;
        }
        //memset(mesh.polys, 0xff, sizeof(ushort)*maxPolys*2*mesh.nvp);
        for (int i = 0; i < maxPolys * 2 * mesh.nvp; ++i) {
            mesh.polys[i] = 0xffff;
        }

        //mesh.regs = (ushort*)rcAlloc(sizeof(ushort)*maxPolys, RC_ALLOC_PERM);
        mesh.regs = new ushort[maxPolys];
        if (mesh.regs == null) {
            ctx.log(rcLogCategory.RC_LOG_ERROR, "rcMergePolyMeshes: Out of memory 'mesh.regs' " + maxPolys);
            return false;
        }
        //memset(mesh.regs, 0, sizeof(ushort)*maxPolys);

        //mesh.areas = (byte*)rcAlloc(sizeof(byte)*maxPolys, RC_ALLOC_PERM);
        mesh.areas = new byte[maxPolys];
        if (mesh.areas == null) {
            ctx.log(rcLogCategory.RC_LOG_ERROR, "rcMergePolyMeshes: Out of memory 'mesh.areas' " + maxPolys);
            return false;
        }
        //memset(mesh.areas, 0, sizeof(byte)*maxPolys);

        //mesh.flags = (ushort*)rcAlloc(sizeof(ushort)*maxPolys, RC_ALLOC_PERM);
        mesh.flags = new ushort[maxPolys];
        if (mesh.flags == null) {
            ctx.log(rcLogCategory.RC_LOG_ERROR, "rcMergePolyMeshes: Out of memory 'mesh.flags' " + maxPolys);
            return false;
        }
        //memset(mesh.flags, 0, sizeof(ushort)*maxPolys);

        //rcScopedDelete<int> nextVert = (int*)rcAlloc(sizeof(int)*maxVerts, RC_ALLOC_TEMP);
        int[] nextVert = new int[maxVerts];
        if (nextVert == null) {
            ctx.log(rcLogCategory.RC_LOG_ERROR, "rcMergePolyMeshes: Out of memory 'nextVert' " + maxVerts);
            return false;
        }
        //memset(nextVert, 0, sizeof(int)*maxVerts);

        //rcScopedDelete<int> firstVert = (int*)rcAlloc(sizeof(int)*VERTEX_BUCKET_COUNT, RC_ALLOC_TEMP);
        int[] firstVert = new int[VERTEX_BUCKET_COUNT];
        if (firstVert == null) {
            ctx.log(rcLogCategory.RC_LOG_ERROR, "rcMergePolyMeshes: Out of memory 'firstVert' " + VERTEX_BUCKET_COUNT);
            return false;
        }
        for (int i = 0; i < VERTEX_BUCKET_COUNT; ++i) {
            firstVert[i] = -1;
        }

        //rcScopedDelete<ushort> vremap = (ushort*)rcAlloc(sizeof(ushort)*maxVertsPerMesh, RC_ALLOC_PERM);
        ushort[] vremap = new ushort[maxVertsPerMesh];
        if (vremap == null) {
            ctx.log(rcLogCategory.RC_LOG_ERROR, "rcMergePolyMeshes: Out of memory 'vremap' " + maxVertsPerMesh);
            return false;
        }
        //memset(vremap, 0, sizeof(ushort)*maxVertsPerMesh);

        for (int i = 0; i < nmeshes; ++i) {
            rcPolyMesh pmesh = meshes[i];

            ushort ox = (ushort)Math.Floor((pmesh.bmin[0] - mesh.bmin[0]) / mesh.cs + 0.5f);
            ushort oz = (ushort)Math.Floor((pmesh.bmin[2] - mesh.bmin[2]) / mesh.cs + 0.5f);

            bool isMinX = (ox == 0);
            bool isMinZ = (oz == 0);
            bool isMaxX = ((ushort)Math.Floor((mesh.bmax[0] - pmesh.bmax[0]) / mesh.cs + 0.5f)) == 0;
            bool isMaxZ = ((ushort)Math.Floor((mesh.bmax[2] - pmesh.bmax[2]) / mesh.cs + 0.5f)) == 0;
            bool isOnBorder = (isMinX || isMinZ || isMaxX || isMaxZ);

            for (int j = 0; j < pmesh.nverts; ++j) {
                //ushort* v = &pmesh.verts[j*3];
                int vIndex = j * 3;
                vremap[j] = addVertex((ushort)(pmesh.verts[vIndex + 0] + ox), pmesh.verts[vIndex + 1], (ushort)(pmesh.verts[vIndex + 2] + oz),
                                      mesh.verts, firstVert, nextVert, ref mesh.nverts);
            }

            for (int j = 0; j < pmesh.npolys; ++j) {
                //ushort* tgt = &mesh.polys[mesh.npolys*2*mesh.nvp];
                //ushort* src = &pmesh.polys[j*2*mesh.nvp];
                int tgtIndex = mesh.npolys * 2 * mesh.nvp;
                int srcIndex = j * 2 * mesh.nvp;

                mesh.regs[mesh.npolys] = pmesh.regs[j];
                mesh.areas[mesh.npolys] = pmesh.areas[j];
                mesh.flags[mesh.npolys] = pmesh.flags[j];
                mesh.npolys++;
                for (int k = 0; k < mesh.nvp; ++k) {
                    if (pmesh.polys[srcIndex + k] == RC_MESH_NULL_IDX) {
                        break;
                    }
                    mesh.polys[tgtIndex + k] = vremap[pmesh.polys[srcIndex + k]];
                }

                if (isOnBorder) {
                    for (int k = mesh.nvp; k < mesh.nvp * 2; ++k) {
                        if ((pmesh.polys[srcIndex + k] & 0x8000) != 0 && (pmesh.polys[srcIndex + k] != 0xffff)) {
                            ushort dir = (ushort)(pmesh.polys[srcIndex + k] & 0xf);
                            switch (dir) {
                                case 0: // Portal x-
                                    if (isMinX)
                                        mesh.polys[tgtIndex + k] = pmesh.polys[srcIndex + k];
                                    break;
                                case 1: // Portal z+
                                    if (isMaxZ)
                                        mesh.polys[tgtIndex + k] = pmesh.polys[srcIndex + k];
                                    break;
                                case 2: // Portal x+
                                    if (isMaxX)
                                        mesh.polys[tgtIndex + k] = pmesh.polys[srcIndex + k];
                                    break;
                                case 3: // Portal z-
                                    if (isMinZ)
                                        mesh.polys[tgtIndex + k] = pmesh.polys[srcIndex + k];
                                    break;
                            }
                        }
                    }
                }
            }
        }

        // Calculate adjacency.
        if (!buildMeshAdjacency(mesh.polys, mesh.npolys, mesh.nverts, mesh.nvp)) {
            ctx.log(rcLogCategory.RC_LOG_ERROR, "rcMergePolyMeshes: Adjacency failed.");
            return false;
        }

        if (mesh.nverts > 0xffff) {
            ctx.log(rcLogCategory.RC_LOG_ERROR, "rcMergePolyMeshes: The resulting mesh has too many vertices " + mesh.nverts + " (max " + 0xffff + "). Data can be corrupted.");
        }
        if (mesh.npolys > 0xffff) {
            ctx.log(rcLogCategory.RC_LOG_ERROR, "rcMergePolyMeshes: The resulting mesh has too many polygons " + mesh.npolys + " (max " + 0xffff + "). Data can be corrupted.");
        }

        ctx.stopTimer(rcTimerLabel.RC_TIMER_MERGE_POLYMESH);

        return true;
    }

    public static bool rcCopyPolyMesh(rcContext ctx, rcPolyMesh src, rcPolyMesh dst) {
        Debug.Assert(ctx != null, "rcContext is null");

        // Destination must be empty.
        Debug.Assert(dst.verts == null);
        Debug.Assert(dst.polys == null);
        Debug.Assert(dst.regs == null);
        Debug.Assert(dst.areas == null);
        Debug.Assert(dst.flags == null);

        dst.nverts = src.nverts;
        dst.npolys = src.npolys;
        dst.maxpolys = src.npolys;
        dst.nvp = src.nvp;
        rcVcopy(dst.bmin, src.bmin);
        rcVcopy(dst.bmax, src.bmax);
        dst.cs = src.cs;
        dst.ch = src.ch;
        dst.borderSize = src.borderSize;

        //dst.verts = (ushort*)rcAlloc(sizeof(ushort)*src.nverts*3, RC_ALLOC_PERM);
        dst.verts = new ushort[src.nverts * 3];
        if (dst.verts == null) {
            ctx.log(rcLogCategory.RC_LOG_ERROR, "rcCopyPolyMesh: Out of memory 'dst.verts' (" + src.nverts * 3 + ").");
            return false;
        }
        //memcpy(dst.verts, src.verts, sizeof(ushort)*src.nverts*3);
        for (int i = 0; i < src.nverts * 3; ++i) {
            dst.verts[i] = src.verts[i];
        }

        //dst.polys = (ushort*)rcAlloc(sizeof(ushort)*src.npolys*2*src.nvp, RC_ALLOC_PERM);
        dst.polys = new ushort[src.npolys * 2 * src.nvp];
        if (dst.polys == null) {
            ctx.log(rcLogCategory.RC_LOG_ERROR, "rcCopyPolyMesh: Out of memory 'dst.polys' (" + src.npolys * 2 * src.nvp + ").");
            return false;
        }
        //memcpy(dst.polys, src.polys, sizeof(ushort)*src.npolys*2*src.nvp);
        for (int i = 0; i < src.npolys * 2 * src.nvp; ++i) {
            dst.polys[i] = src.polys[i];
        }

        //dst.regs = (ushort*)rcAlloc(sizeof(ushort)*src.npolys, RC_ALLOC_PERM);
        dst.regs = new ushort[src.npolys];
        if (dst.regs == null) {
            ctx.log(rcLogCategory.RC_LOG_ERROR, "rcCopyPolyMesh: Out of memory 'dst.regs' (" + src.npolys + ").");
            return false;
        }
        //memcpy(dst.regs, src.regs, sizeof(ushort)*src.npolys);
        for (int i = 0; i < src.npolys; ++i) {
            dst.regs[i] = src.regs[i];
        }

        //dst.areas = (byte*)rcAlloc(sizeof(byte)*src.npolys, RC_ALLOC_PERM);
        dst.areas = new byte[src.npolys];
        if (dst.areas == null) {
            ctx.log(rcLogCategory.RC_LOG_ERROR, "rcCopyPolyMesh: Out of memory 'dst.areas' (" + src.npolys + ").");
            return false;
        }
        //memcpy(dst.areas, src.areas, sizeof(byte)*src.npolys);
        for (int i = 0; i < src.npolys; ++i) {
            dst.areas[i] = src.areas[i];
        }

        //dst.flags = (ushort*)rcAlloc(sizeof(ushort)*src.npolys, RC_ALLOC_PERM);
        dst.flags = new ushort[src.npolys];
        if (dst.flags != null) {
            ctx.log(rcLogCategory.RC_LOG_ERROR, "rcCopyPolyMesh: Out of memory 'dst.flags' (" + src.npolys + ").");
            return false;
        }
        //memcpy(dst.flags, src.flags, sizeof(byte)*src.npolys);
        for (int i = 0; i < src.npolys; ++i) {
            dst.flags[i] = src.flags[i];
        }

        return true;
    }
}