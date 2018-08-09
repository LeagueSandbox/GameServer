using System;
using System.Diagnostics;

public static partial class Recast {
    /// @par 
    /// 
    /// Basically, any spans that are closer to a boundary or obstruction than the specified radius 
    /// are marked as unwalkable.
    ///
    /// This method is usually called immediately after the heightfield has been built.
    ///
    /// @see rcCompactHeightfield, rcBuildCompactHeightfield, rcConfig::walkableRadius
    public static bool rcErodeWalkableArea(rcContext ctx, int radius, rcCompactHeightfield chf) {
        Debug.Assert(ctx != null, "rcContext is null");

        int w = chf.width;
        int h = chf.height;

        ctx.startTimer(rcTimerLabel.RC_TIMER_ERODE_AREA);

        byte[] dist = new byte[chf.spanCount];//(byte*)rcAlloc(sizeof(byte)*chf.spanCount, RC_ALLOC_TEMP);
        if (dist == null) {
            ctx.log(rcLogCategory.RC_LOG_ERROR, "erodeWalkableArea: Out of memory 'dist' " + chf.spanCount);
            return false;
        }

        // Init distance.
        for (int i=0; i < chf.spanCount; ++i) {
            dist[i] = 0xff;
        }
        //	memset(dist, 0xff, sizeof(byte)*chf.spanCount);

        // Mark boundary cells.
        for (int y = 0; y < h; ++y) {
            for (int x = 0; x < w; ++x) {
                rcCompactCell c = chf.cells[x + y * w];
                for (int i = (int)c.index, ni = (int)(c.index + c.count); i < ni; ++i) {
                    if (chf.areas[i] == RC_NULL_AREA) {
                        dist[i] = 0;
                    } else {
                        rcCompactSpan s = chf.spans[i];
                        int nc = 0;
                        for (int dir = 0; dir < 4; ++dir) {
                            if (rcGetCon(s, dir) != RC_NOT_CONNECTED) {
                                int nx = x + rcGetDirOffsetX(dir);
                                int ny = y + rcGetDirOffsetY(dir);
                                int nidx = (int)chf.cells[nx + ny * w].index + rcGetCon(s, dir);
                                if (chf.areas[nidx] != RC_NULL_AREA) {
                                    nc++;
                                }
                            }
                        }
                        // At least one missing neighbour.
                        if (nc != 4)
                            dist[i] = 0;
                    }
                }
            }
        }

        byte nd = 0;

        // Pass 1
        for (int y = 0; y < h; ++y) {
            for (int x = 0; x < w; ++x) {
                rcCompactCell c = chf.cells[x + y * w];
                for (int i = (int)c.index, ni = (int)(c.index + c.count); i < ni; ++i) {
                    rcCompactSpan s = chf.spans[i];

                    if (rcGetCon(s, 0) != RC_NOT_CONNECTED) {
                        // (-1,0)
                        int ax = x + rcGetDirOffsetX(0);
                        int ay = y + rcGetDirOffsetY(0);
                        int ai = (int)chf.cells[ax + ay * w].index + rcGetCon(s, 0);
                        rcCompactSpan aSpan = chf.spans[ai];
                        nd = (byte)Math.Min((int)dist[ai] + 2, 255);
                        if (nd < dist[i])
                            dist[i] = nd;

                        // (-1,-1)
                        if (rcGetCon(aSpan, 3) != RC_NOT_CONNECTED) {
                            int aax = ax + rcGetDirOffsetX(3);
                            int aay = ay + rcGetDirOffsetY(3);
                            int aai = (int)chf.cells[aax + aay * w].index + rcGetCon(aSpan, 3);
                            nd = (byte)Math.Min((int)dist[aai] + 3, 255);
                            if (nd < dist[i])
                                dist[i] = nd;
                        }
                    }
                    if (rcGetCon(s, 3) != RC_NOT_CONNECTED) {
                        // (0,-1)
                        int ax = x + rcGetDirOffsetX(3);
                        int ay = y + rcGetDirOffsetY(3);
                        int ai = (int)chf.cells[ax + ay * w].index + rcGetCon(s, 3);
                        rcCompactSpan aSpan = chf.spans[ai];
                        nd = (byte)Math.Min((int)dist[ai] + 2, 255);
                        if (nd < dist[i])
                            dist[i] = nd;

                        // (1,-1)
                        if (rcGetCon(aSpan, 2) != RC_NOT_CONNECTED) {
                            int aax = ax + rcGetDirOffsetX(2);
                            int aay = ay + rcGetDirOffsetY(2);
                            int aai = (int)chf.cells[aax + aay * w].index + rcGetCon(aSpan, 2);
                            nd = (byte)Math.Min((int)dist[aai] + 3, 255);
                            if (nd < dist[i])
                                dist[i] = nd;
                        }
                    }
                }
            }
        }

        // Pass 2
        for (int y = h - 1; y >= 0; --y) {
            for (int x = w - 1; x >= 0; --x) {
                rcCompactCell c = chf.cells[x + y * w];
                for (int i = (int)c.index, ni = (int)(c.index + c.count); i < ni; ++i) {
                    rcCompactSpan s = chf.spans[i];

                    if (rcGetCon(s, 2) != RC_NOT_CONNECTED) {
                        // (1,0)
                        int ax = x + rcGetDirOffsetX(2);
                        int ay = y + rcGetDirOffsetY(2);
                        int ai = (int)chf.cells[ax + ay * w].index + rcGetCon(s, 2);
                        rcCompactSpan aSpan = chf.spans[ai];
                        nd = (byte)Math.Min((int)dist[ai] + 2, 255);
                        if (nd < dist[i])
                            dist[i] = nd;

                        // (1,1)
                        if (rcGetCon(aSpan, 1) != RC_NOT_CONNECTED) {
                            int aax = ax + rcGetDirOffsetX(1);
                            int aay = ay + rcGetDirOffsetY(1);
                            int aai = (int)chf.cells[aax + aay * w].index + rcGetCon(aSpan, 1);
                            nd = (byte)Math.Min((int)dist[aai] + 3, 255);
                            if (nd < dist[i])
                                dist[i] = nd;
                        }
                    }
                    if (rcGetCon(s, 1) != RC_NOT_CONNECTED) {
                        // (0,1)
                        int ax = x + rcGetDirOffsetX(1);
                        int ay = y + rcGetDirOffsetY(1);
                        int ai = (int)chf.cells[ax + ay * w].index + rcGetCon(s, 1);
                        rcCompactSpan aSpan = chf.spans[ai];
                        nd = (byte)Math.Min((int)dist[ai] + 2, 255);
                        if (nd < dist[i])
                            dist[i] = nd;

                        // (-1,1)
                        if (rcGetCon(aSpan, 0) != RC_NOT_CONNECTED) {
                            int aax = ax + rcGetDirOffsetX(0);
                            int aay = ay + rcGetDirOffsetY(0);
                            int aai = (int)chf.cells[aax + aay * w].index + rcGetCon(aSpan, 0);
                            nd = (byte)Math.Min((int)dist[aai] + 3, 255);
                            if (nd < dist[i])
                                dist[i] = nd;
                        }
                    }
                }
            }
        }

        byte thr = (byte)(radius * 2);
        for (int i = 0; i < chf.spanCount; ++i)
            if (dist[i] < thr)
                chf.areas[i] = RC_NULL_AREA;

        ctx.stopTimer(rcTimerLabel.RC_TIMER_ERODE_AREA);

        return true;
    }

    static void insertSort(byte[] a, int n) {
        int i, j;
        for (i = 1; i < n; i++) {
            byte value = a[i];
            for (j = i - 1; j >= 0 && a[j] > value; j--)
                a[j + 1] = a[j];
            a[j + 1] = value;
        }
    }

    /// @par
    ///
    /// This filter is usually applied after applying area id's using functions
    /// such as #rcMarkBoxArea, #rcMarkConvexPolyArea, and #rcMarkCylinderArea.
    /// 
    /// @see rcCompactHeightfield
    public static bool rcMedianFilterWalkableArea(rcContext ctx, rcCompactHeightfield chf) {
        Debug.Assert(ctx != null, "rcContext is null");

        int w = chf.width;
        int h = chf.height;

        ctx.startTimer(rcTimerLabel.RC_TIMER_MEDIAN_AREA);

        byte[] areas = new byte[chf.spanCount];//(byte*)rcAlloc(sizeof(byte)*chf.spanCount, RC_ALLOC_TEMP);
        if (areas == null) {
            ctx.log(rcLogCategory.RC_LOG_ERROR, "medianFilterWalkableArea: Out of memory 'areas' " + chf.spanCount);
            return false;
        }

        // Init distance.
        for (int i = 0; i < chf.spanCount; ++i) {
            areas[i] = 0xff;
        }
        //memset(areas, 0xff, sizeof(byte)*chf.spanCount);

        for (int y = 0; y < h; ++y) {
            for (int x = 0; x < w; ++x) {
                rcCompactCell c = chf.cells[x + y * w];
                for (int i = (int)c.index, ni = (int)(c.index + c.count); i < ni; ++i) {
                    rcCompactSpan s = chf.spans[i];
                    if (chf.areas[i] == RC_NULL_AREA) {
                        areas[i] = chf.areas[i];
                        continue;
                    }

                    byte[] nei = new byte[9];
                    for (int j = 0; j < 9; ++j)
                        nei[j] = chf.areas[i];

                    for (int dir = 0; dir < 4; ++dir) {
                        if (rcGetCon(s, dir) != RC_NOT_CONNECTED) {
                            int ax = x + rcGetDirOffsetX(dir);
                            int ay = y + rcGetDirOffsetY(dir);
                            int ai = (int)chf.cells[ax + ay * w].index + rcGetCon(s, dir);
                            if (chf.areas[ai] != RC_NULL_AREA)
                                nei[dir * 2 + 0] = chf.areas[ai];

                            rcCompactSpan aSpan = chf.spans[ai];
                            int dir2 = (dir + 1) & 0x3;
                            if (rcGetCon(aSpan, dir2) != RC_NOT_CONNECTED) {
                                int ax2 = ax + rcGetDirOffsetX(dir2);
                                int ay2 = ay + rcGetDirOffsetY(dir2);
                                int ai2 = (int)chf.cells[ax2 + ay2 * w].index + rcGetCon(aSpan, dir2);
                                if (chf.areas[ai2] != RC_NULL_AREA)
                                    nei[dir * 2 + 1] = chf.areas[ai2];
                            }
                        }
                    }
                    insertSort(nei, 9);
                    areas[i] = nei[4];
                }
            }
        }

        chf.areas = areas;
        //memcpy(chf.areas, areas, sizeof(byte)*chf.spanCount);

        //rcFree(areas);

        ctx.stopTimer(rcTimerLabel.RC_TIMER_MEDIAN_AREA);

        return true;
    }

    /// @par
    ///
    /// The value of spacial parameters are in world units.
    /// 
    /// @see rcCompactHeightfield, rcMedianFilterWalkableArea
    public static void rcMarkBoxArea(rcContext ctx, float[] bmin, float[] bmax, byte areaId,
                       rcCompactHeightfield chf) {
        Debug.Assert(ctx != null, "rcContext is null");

        ctx.startTimer(rcTimerLabel.RC_TIMER_MARK_BOX_AREA);

        int minx = (int)((bmin[0] - chf.bmin[0]) / chf.cs);
        int miny = (int)((bmin[1] - chf.bmin[1]) / chf.ch);
        int minz = (int)((bmin[2] - chf.bmin[2]) / chf.cs);
        int maxx = (int)((bmax[0] - chf.bmin[0]) / chf.cs);
        int maxy = (int)((bmax[1] - chf.bmin[1]) / chf.ch);
        int maxz = (int)((bmax[2] - chf.bmin[2]) / chf.cs);

        if (maxx < 0) return;
        if (minx >= chf.width) return;
        if (maxz < 0) return;
        if (minz >= chf.height) return;

        if (minx < 0) minx = 0;
        if (maxx >= chf.width) maxx = chf.width - 1;
        if (minz < 0) minz = 0;
        if (maxz >= chf.height) maxz = chf.height - 1;

        for (int z = minz; z <= maxz; ++z) {
            for (int x = minx; x <= maxx; ++x) {
                rcCompactCell c = chf.cells[x + z * chf.width];
                for (int i = (int)c.index, ni = (int)(c.index + c.count); i < ni; ++i) {
                    rcCompactSpan s = chf.spans[i];
                    if ((int)s.y >= miny && (int)s.y <= maxy) {
                        if (chf.areas[i] != RC_NULL_AREA)
                            chf.areas[i] = areaId;
                    }
                }
            }
        }

        ctx.stopTimer(rcTimerLabel.RC_TIMER_MARK_BOX_AREA);

    }


    public static bool pointInPoly(int nvert, float[] verts, float[] p) {
        bool c = false;
        int i = 0;
        int j = 0;
        for (i = 0, j = nvert - 1; i < nvert; j = i++) {
            int viStart = i * 3;
            int vjStart = j * 3;
            if (((verts[viStart + 2] > p[2]) != (verts[vjStart + 2] > p[2])) &&
                (p[0] < (verts[vjStart + 0] - verts[viStart + 0]) * (p[2] - verts[viStart + 2]) / (verts[vjStart + 2] - verts[viStart + 2]) + verts[viStart + 0])) {
                c = !c;
            }
        }
        return c;
    }

    /// @par
    ///
    /// The value of spacial parameters are in world units.
    /// 
    /// The y-values of the polygon vertices are ignored. So the polygon is effectively 
    /// projected onto the xz-plane at @p hmin, then extruded to @p hmax.
    /// 
    /// @see rcCompactHeightfield, rcMedianFilterWalkableArea
    public static void rcMarkConvexPolyArea(rcContext ctx, float[] verts, int nverts,
                              float hmin, float hmax, byte areaId,
                              rcCompactHeightfield chf) {
        Debug.Assert(ctx != null, "rcContext is null");

        ctx.startTimer(rcTimerLabel.RC_TIMER_MARK_CONVEXPOLY_AREA);

        float[] bmin = new float[3];
        float[] bmax = new float[3];
        rcVcopy(bmin, verts);
        rcVcopy(bmax, verts);
        for (int i = 1; i < nverts; ++i) {
            int vStart = i * 3;
            rcVmin(bmin, 0, verts, vStart);
            rcVmax(bmax, 0, verts, vStart);
        }
        bmin[1] = hmin;
        bmax[1] = hmax;

        int minx = (int)((bmin[0] - chf.bmin[0]) / chf.cs);
        int miny = (int)((bmin[1] - chf.bmin[1]) / chf.ch);
        int minz = (int)((bmin[2] - chf.bmin[2]) / chf.cs);
        int maxx = (int)((bmax[0] - chf.bmin[0]) / chf.cs);
        int maxy = (int)((bmax[1] - chf.bmin[1]) / chf.ch);
        int maxz = (int)((bmax[2] - chf.bmin[2]) / chf.cs);

        if (maxx < 0) return;
        if (minx >= chf.width) return;
        if (maxz < 0) return;
        if (minz >= chf.height) return;

        if (minx < 0) minx = 0;
        if (maxx >= chf.width) maxx = chf.width - 1;
        if (minz < 0) minz = 0;
        if (maxz >= chf.height) maxz = chf.height - 1;


        // TODO: Optimize.
        for (int z = minz; z <= maxz; ++z) {
            for (int x = minx; x <= maxx; ++x) {
                rcCompactCell c = chf.cells[x + z * chf.width];
                for (int i = (int)c.index, ni = (int)(c.index + c.count); i < ni; ++i) {
                    rcCompactSpan s = chf.spans[i];
                    if (chf.areas[i] == RC_NULL_AREA)
                        continue;
                    if ((int)s.y >= miny && (int)s.y <= maxy) {
                        float[] p = new float[3];
                        p[0] = chf.bmin[0] + (x + 0.5f) * chf.cs;
                        p[1] = 0;
                        p[2] = chf.bmin[2] + (z + 0.5f) * chf.cs;

                        if (pointInPoly(nverts, verts, p)) {
                            chf.areas[i] = areaId;
                        }
                    }
                }
            }
        }

        ctx.stopTimer(rcTimerLabel.RC_TIMER_MARK_CONVEXPOLY_AREA);
    }

    static int rcOffsetPoly(float[] verts, int nverts, float offset,
                     float[] outVerts, int maxOutVerts) {
        const float MITER_LIMIT = 1.20f;

        int n = 0;

        for (int i = 0; i < nverts; i++) {
            int a = (i + nverts - 1) % nverts;
            int b = i;
            int c = (i + 1) % nverts;
            int vaStart = a * 3;
            int vbStart = b * 3;
            int vcStart = c * 3;
            float dx0 = verts[vbStart + 0] - verts[vaStart + 0];
            float dy0 = verts[vbStart + 2] - verts[vaStart + 2];
            float d0 = dx0 * dx0 + dy0 * dy0;
            if (d0 > 1e-6f) {
                d0 = 1.0f / (float)Math.Sqrt(d0);
                dx0 *= d0;
                dy0 *= d0;
            }
            float dx1 = verts[vcStart + 0] - verts[vbStart + 0];
            float dy1 = verts[vcStart + 2] - verts[vbStart + 2];
            float d1 = dx1 * dx1 + dy1 * dy1;
            if (d1 > 1e-6f) {
                d1 = 1.0f / (float)Math.Sqrt(d1);
                dx1 *= d1;
                dy1 *= d1;
            }
            float dlx0 = -dy0;
            float dly0 = dx0;
            float dlx1 = -dy1;
            float dly1 = dx1;
            float cross = dx1 * dy0 - dx0 * dy1;
            float dmx = (dlx0 + dlx1) * 0.5f;
            float dmy = (dly0 + dly1) * 0.5f;
            float dmr2 = dmx * dmx + dmy * dmy;
            bool bevel = dmr2 * MITER_LIMIT * MITER_LIMIT < 1.0f;
            if (dmr2 > 1e-6f) {
                float scale = 1.0f / dmr2;
                dmx *= scale;
                dmy *= scale;
            }

            if (bevel && cross < 0.0f) {
                if (n + 2 >= maxOutVerts)
                    return 0;
                float d = (1.0f - (dx0 * dx1 + dy0 * dy1)) * 0.5f;
                outVerts[n * 3 + 0] = verts[vbStart + 0] + (-dlx0 + dx0 * d) * offset;
                outVerts[n * 3 + 1] = verts[vbStart + 1];
                outVerts[n * 3 + 2] = verts[vbStart + 2] + (-dly0 + dy0 * d) * offset;
                n++;
                outVerts[n * 3 + 0] = verts[vbStart + 0] + (-dlx1 - dx1 * d) * offset;
                outVerts[n * 3 + 1] = verts[vbStart + 1];
                outVerts[n * 3 + 2] = verts[vbStart + 2] + (-dly1 - dy1 * d) * offset;
                n++;
            } else {
                if (n + 1 >= maxOutVerts)
                    return 0;
                outVerts[n * 3 + 0] = verts[vbStart + 0] - dmx * offset;
                outVerts[n * 3 + 1] = verts[vbStart + 1];
                outVerts[n * 3 + 2] = verts[vbStart + 2] - dmy * offset;
                n++;
            }
        }

        return n;
    }


    /// @par
    ///
    /// The value of spacial parameters are in world units.
    /// 
    /// @see rcCompactHeightfield, rcMedianFilterWalkableArea
    static public void rcMarkCylinderArea(rcContext ctx, float[] pos,
                            float r, float h, byte areaId,
                            rcCompactHeightfield chf) {
        Debug.Assert(ctx != null, "rcContext is null");

        ctx.startTimer(rcTimerLabel.RC_TIMER_MARK_CYLINDER_AREA);

        float[] bmin = new float[3];
        float[] bmax = new float[3];
        bmin[0] = pos[0] - r;
        bmin[1] = pos[1];
        bmin[2] = pos[2] - r;
        bmax[0] = pos[0] + r;
        bmax[1] = pos[1] + h;
        bmax[2] = pos[2] + r;
        float r2 = r * r;

        int minx = (int)((bmin[0] - chf.bmin[0]) / chf.cs);
        int miny = (int)((bmin[1] - chf.bmin[1]) / chf.ch);
        int minz = (int)((bmin[2] - chf.bmin[2]) / chf.cs);
        int maxx = (int)((bmax[0] - chf.bmin[0]) / chf.cs);
        int maxy = (int)((bmax[1] - chf.bmin[1]) / chf.ch);
        int maxz = (int)((bmax[2] - chf.bmin[2]) / chf.cs);

        if (maxx < 0) return;
        if (minx >= chf.width) return;
        if (maxz < 0) return;
        if (minz >= chf.height) return;

        if (minx < 0) minx = 0;
        if (maxx >= chf.width) maxx = chf.width - 1;
        if (minz < 0) minz = 0;
        if (maxz >= chf.height) maxz = chf.height - 1;


        for (int z = minz; z <= maxz; ++z) {
            for (int x = minx; x <= maxx; ++x) {
                rcCompactCell c = chf.cells[x + z * chf.width];
                for (int i = (int)c.index, ni = (int)(c.index + c.count); i < ni; ++i) {
                    rcCompactSpan s = chf.spans[i];

                    if (chf.areas[i] == RC_NULL_AREA)
                        continue;

                    if ((int)s.y >= miny && (int)s.y <= maxy) {
                        float sx = chf.bmin[0] + (x + 0.5f) * chf.cs;
                        float sz = chf.bmin[2] + (z + 0.5f) * chf.cs;
                        float dx = sx - pos[0];
                        float dz = sz - pos[2];

                        if (dx * dx + dz * dz < r2) {
                            chf.areas[i] = areaId;
                        }
                    }
                }
            }
        }

        ctx.stopTimer(rcTimerLabel.RC_TIMER_MARK_CYLINDER_AREA);
    }
}
