using System;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

public static partial class Recast {

    /// The value of PI used by Recast.
    public const float RC_PI = 3.14159265f;

    /// Defines the number of bits allocated to rcSpan::smin and rcSpan::smax.
    public const int RC_SPAN_HEIGHT_BITS = 13;
    /// Defines the maximum value for rcSpan::smin and rcSpan::smax.
    public const int RC_SPAN_MAX_HEIGHT = (1 << RC_SPAN_HEIGHT_BITS) - 1;

    /// Recast log categories.
    /// @see rcContext
    public enum rcLogCategory {
        RC_LOG_PROGRESS = 1,	//< A progress log entry.
        RC_LOG_WARNING,			//< A warning log entry.
        RC_LOG_ERROR,			//< An error log entry.
    };

    /// Recast performance timer categories.
    /// @see rcContext
    public enum rcTimerLabel {
        /// The user defined total time of the build.
        RC_TIMER_TOTAL,
        /// A user defined build time.
        RC_TIMER_TEMP,
        /// The time to rasterize the triangles. (See: #rcRasterizeTriangle)
        RC_TIMER_RASTERIZE_TRIANGLES,
        /// The time to build the compact heightfield. (See: #rcBuildCompactHeightfield)
        RC_TIMER_BUILD_COMPACTHEIGHTFIELD,
        /// The total time to build the contours. (See: #rcBuildContours)
        RC_TIMER_BUILD_CONTOURS,
        /// The time to trace the boundaries of the contours. (See: #rcBuildContours)
        RC_TIMER_BUILD_CONTOURS_TRACE,
        /// The time to simplify the contours. (See: #rcBuildContours)
        RC_TIMER_BUILD_CONTOURS_SIMPLIFY,
        /// The time to filter ledge spans. (See: #rcFilterLedgeSpans)
        RC_TIMER_FILTER_BORDER,
        /// The time to filter low height spans. (See: #rcFilterWalkableLowHeightSpans)
        RC_TIMER_FILTER_WALKABLE,
        /// The time to apply the median filter. (See: #rcMedianFilterWalkableArea)
        RC_TIMER_MEDIAN_AREA,
        /// The time to filter low obstacles. (See: #rcFilterLowHangingWalkableObstacles)
        RC_TIMER_FILTER_LOW_OBSTACLES,
        /// The time to build the polygon mesh. (See: #rcBuildPolyMesh)
        RC_TIMER_BUILD_POLYMESH,
        /// The time to merge polygon meshes. (See: #rcMergePolyMeshes)
        RC_TIMER_MERGE_POLYMESH,
        /// The time to erode the walkable area. (See: #rcErodeWalkableArea)
        RC_TIMER_ERODE_AREA,
        /// The time to mark a box area. (See: #rcMarkBoxArea)
        RC_TIMER_MARK_BOX_AREA,
        /// The time to mark a cylinder area. (See: #rcMarkCylinderArea)
        RC_TIMER_MARK_CYLINDER_AREA,
        /// The time to mark a convex polygon area. (See: #rcMarkConvexPolyArea)
        RC_TIMER_MARK_CONVEXPOLY_AREA,
        /// The total time to build the distance field. (See: #rcBuildDistanceField)
        RC_TIMER_BUILD_DISTANCEFIELD,
        /// The time to build the distances of the distance field. (See: #rcBuildDistanceField)
        RC_TIMER_BUILD_DISTANCEFIELD_DIST,
        /// The time to blur the distance field. (See: #rcBuildDistanceField)
        RC_TIMER_BUILD_DISTANCEFIELD_BLUR,
        /// The total time to build the regions. (See: #rcBuildRegions, #rcBuildRegionsMonotone)
        RC_TIMER_BUILD_REGIONS,
        /// The total time to apply the watershed algorithm. (See: #rcBuildRegions)
        RC_TIMER_BUILD_REGIONS_WATERSHED,
        /// The time to expand regions while applying the watershed algorithm. (See: #rcBuildRegions)
        RC_TIMER_BUILD_REGIONS_EXPAND,
        /// The time to flood regions while applying the watershed algorithm. (See: #rcBuildRegions)
        RC_TIMER_BUILD_REGIONS_FLOOD,
        /// The time to filter out small regions. (See: #rcBuildRegions, #rcBuildRegionsMonotone)
        RC_TIMER_BUILD_REGIONS_FILTER,
        /// The time to build heightfield layers. (See: #rcBuildHeightfieldLayers)
        RC_TIMER_BUILD_LAYERS,
        /// The time to build the polygon mesh detail. (See: #rcBuildPolyMeshDetail)
        RC_TIMER_BUILD_POLYMESHDETAIL,
        /// The time to merge polygon mesh details. (See: #rcMergePolyMeshDetails)
        RC_TIMER_MERGE_POLYMESHDETAIL,
        /// The maximum number of timers.  (Used for iterating timers.)
        RC_MAX_TIMERS
    };


    public static void rcCalcBounds(float[] verts, int nv, float[] bmin, float[] bmax) {
        // Calculate bounding box.
        rcVcopy(bmin, verts);
        rcVcopy(bmax, verts);
        for (int i = 1; i < nv; ++i) {
            int vStart = i * 3;
            rcVmin(bmin, 0, verts, vStart);
            rcVmax(bmax, 0, verts, vStart);
        }
    }

    public static void rcCalcGridSize(float[] bmin, float[] bmax, float cs, out int w, out int h) {
        w = (int)((bmax[0] - bmin[0]) / cs + 0.5f);
        h = (int)((bmax[2] - bmin[2]) / cs + 0.5f);
    }


    /// @par
    ///
    /// See the #rcConfig documentation for more information on the configuration parameters.
    /// 
    /// @see rcAllocHeightfield, rcHeightfield 
    public static bool rcCreateHeightfield(rcContext ctx, rcHeightfield hf, int width, int height,
                         float[] bmin, float[] bmax,
                         float cs, float ch) {
        rcIgnoreUnused(ctx);

        hf.width = width;
        hf.height = height;
        rcVcopy(hf.bmin, bmin);
        rcVcopy(hf.bmax, bmax);
        hf.cs = cs;
        hf.ch = ch;
        hf.spans = new rcSpan[hf.width * hf.height];//(rcSpan**)rcAlloc(sizeof(rcSpan*)*hf.width*hf.height, RC_ALLOC_PERM);
        if (hf.spans == null)
            return false;
        //memset(hf.spans, 0, sizeof(rcSpan*)*hf.width*hf.height);
        return true;
    }

    public static void calcTriNormal(float[] v0, float[] v1, float[] v2, float[] norm) {
        float[] e0 = new float[3];
        float[] e1 = new float[3];
        rcVsub(e0, v1, v0);
        rcVsub(e1, v2, v0);
        rcVcross(norm, e0, e1);
        rcVnormalize(norm);
    }

    public static void calcTriNormal(float[] v0, int v0Start, float[] v1, int v1Start, float[] v2, int v2Start, float[] norm) {
        float[] e0 = new float[3];
        float[] e1 = new float[3];
        rcVsub(e0, 0, v1, v1Start, v0, v0Start);
        rcVsub(e1, 0, v2, v2Start, v0, v0Start);
        rcVcross(norm, 0, e0, 0, e1, 0);
        rcVnormalize(norm);
    }

    /// @par
    ///
    /// Only sets the aread id's for the walkable triangles.  Does not alter the
    /// area id's for unwalkable triangles.
    /// 
    /// See the #rcConfig documentation for more information on the configuration parameters.
    /// 
    /// @see rcHeightfield, rcClearUnwalkableTriangles, rcRasterizeTriangles
    public static void rcMarkWalkableTriangles(rcContext ctx, float walkableSlopeAngle,
                                 float[] verts, int nv,
                                 int[] tris, int nt,
                                 byte[] areas) {
        rcIgnoreUnused(ctx);

        float walkableThr = (float)Math.Cos(walkableSlopeAngle / 180.0f * RC_PI);

        float[] norm = new float[3];

        for (int i = 0; i < nt; ++i) {
            int triStart = i * 3;
            calcTriNormal(verts, tris[triStart + 0] * 3, verts, tris[triStart + 1] * 3, verts, tris[triStart + 2] * 3, norm);
            // Check if the face is walkable.
            if (norm[1] > walkableThr)
                areas[i] = RC_WALKABLE_AREA;
        }
    }

    /// @par
    ///
    /// Only sets the aread id's for the unwalkable triangles.  Does not alter the
    /// area id's for walkable triangles.
    /// 
    /// See the #rcConfig documentation for more information on the configuration parameters.
    /// 
    /// @see rcHeightfield, rcClearUnwalkableTriangles, rcRasterizeTriangles
    public static void rcClearUnwalkableTriangles(rcContext ctx, float walkableSlopeAngle,
                                    float[] verts, int nv,
                                    int[] tris, int nt,
                                    byte[] areas) {
        rcIgnoreUnused(ctx);

        float walkableThr = (float)Math.Cos(walkableSlopeAngle / 180.0f * RC_PI);

        float[] norm = new float[3];

        for (int i = 0; i < nt; ++i) {
            int triStart = i * 3;
            calcTriNormal(verts, tris[triStart + 0] * 3, verts, tris[triStart + 1] * 3, verts, tris[triStart + 2] * 3, norm);
            // Check if the face is walkable.
            if (norm[1] <= walkableThr)
                areas[i] = RC_NULL_AREA;
        }
    }

    public static int rcGetHeightFieldSpanCount(rcContext ctx, rcHeightfield hf) {
        rcIgnoreUnused(ctx);

        int w = hf.width;
        int h = hf.height;
        int spanCount = 0;
        for (int y = 0; y < h; ++y) {
            for (int x = 0; x < w; ++x) {
                for (rcSpan s = hf.spans[x + y * w]; s != null; s = s.next) {
                    if (s.area != RC_NULL_AREA)
                        spanCount++;
                }
            }
        }
        return spanCount;
    }

    public static void rccsArrayItemsCreate<T>(T[] array) where T : class, new() {
        for (int i = 0; i < array.Length; ++i) {
            array[i] = new T();
        }
    }

    /// @par
    ///
    /// This is just the beginning of the process of fully building a compact heightfield.
    /// Various filters may be applied applied, then the distance field and regions built.
    /// E.g: #rcBuildDistanceField and #rcBuildRegions
    ///
    /// See the #rcConfig documentation for more information on the configuration parameters.
    ///
    /// @see rcAllocCompactHeightfield, rcHeightfield, rcCompactHeightfield, rcConfig
    public static bool rcBuildCompactHeightfield(rcContext ctx, int walkableHeight, int walkableClimb,
                                   rcHeightfield hf, rcCompactHeightfield chf) {
        Debug.Assert(ctx != null, "rcBuildCompactHeightfield Assert(ctx != null)");

        ctx.startTimer(rcTimerLabel.RC_TIMER_BUILD_COMPACTHEIGHTFIELD);

        int w = hf.width;
        int h = hf.height;
        int spanCount = rcGetHeightFieldSpanCount(ctx, hf);

        // Fill in header.
        chf.width = w;
        chf.height = h;
        chf.spanCount = spanCount;
        chf.walkableHeight = walkableHeight;
        chf.walkableClimb = walkableClimb;
        chf.maxRegions = 0;
        rcVcopy(chf.bmin, hf.bmin);
        rcVcopy(chf.bmax, hf.bmax);
        chf.bmax[1] += walkableHeight * hf.ch;
        chf.cs = hf.cs;
        chf.ch = hf.ch;
        chf.cells = new rcCompactCell[w * h];
        //chf.cells = (rcCompactCell*)rcAlloc(sizeof(rcCompactCell)*w*h, RC_ALLOC_PERM);

        if (chf.cells == null) {
            ctx.log(rcLogCategory.RC_LOG_ERROR, "rcBuildCompactHeightfield: Out of memory 'chf.cells' (" + (w * h) + ")");
            return false;
        }
        chf.spans = new rcCompactSpan[spanCount];// (rcCompactSpan*)rcAlloc(sizeof(rcCompactSpan)*spanCount, RC_ALLOC_PERM);
        if (chf.spans == null) {
            ctx.log(rcLogCategory.RC_LOG_ERROR, "rcBuildCompactHeightfield: Out of memory 'chf.spans' (" + spanCount + ")");
            return false;
        }
        chf.areas = new byte[spanCount]; //(byte*)rcAlloc(sizeof(byte)*spanCount, RC_ALLOC_PERM);
        if (chf.areas == null) {
            ctx.log(rcLogCategory.RC_LOG_ERROR, "rcBuildCompactHeightfield: Out of memory 'chf.areas' (" + spanCount + ")"); ;
            return false;
        }

        int MAX_HEIGHT = 0xffff;

        // Fill in cells and spans.
        int idx = 0;
        for (int y = 0; y < h; ++y) {
            for (int x = 0; x < w; ++x) {
                rcSpan s = hf.spans[x + y * w];
                // If there are no spans at this cell, just leave the data to index=0, count=0.
                if (s == null) {
                    continue;
                }
				rcCompactCell c;// =  chf.cells[x + y * w];
                c.index = (uint)idx;
				c.count = 0;

                while (s != null) {
                    if (s.area != RC_NULL_AREA) {
                        int bot = (int)s.smax;
                        int top = s.next != null ? (int)s.next.smin : MAX_HEIGHT;
                        chf.spans[idx].y = (ushort)rcClamp(bot, 0, 0xffff);
                        chf.spans[idx].h = (byte)rcClamp(top - bot, 0, 0xff);
                        chf.areas[idx] = s.area;
                        idx++;
						c.count++;
                    }
                    s = s.next;
                }
				chf.cells[x + y * w] = c;
            }
        }

        // Find neighbour connections.
        int MAX_LAYERS = RC_NOT_CONNECTED - 1;
        int tooHighNeighbour = 0;
        for (int y = 0; y < h; ++y) {
            for (int x = 0; x < w; ++x) {
                rcCompactCell c = chf.cells[x + y * w];
                for (int i = (int)c.index, ni = (int)(c.index + c.count); i < ni; ++i) {
					rcCompactSpan s =  chf.spans[i];

                    for (int dir = 0; dir < 4; ++dir) {
                        rcSetCon(ref s, dir, RC_NOT_CONNECTED);
                        int nx = x + rcGetDirOffsetX(dir);
                        int ny = y + rcGetDirOffsetY(dir);
                        // First check that the neighbour cell is in bounds.
                        if (nx < 0 || ny < 0 || nx >= w || ny >= h)
                            continue;

                        // Iterate over all neighbour spans and check if any of the is
                        // accessible from current cell.
                        rcCompactCell nc = chf.cells[nx + ny * w];
                        for (int k = (int)nc.index, nk = (int)(nc.index + nc.count); k < nk; ++k) {
                            rcCompactSpan ns = chf.spans[k];
                            int bot = Math.Max(s.y, ns.y);
                            int top = Math.Min(s.y + s.h, ns.y + ns.h);

                            // Check that the gap between the spans is walkable,
                            // and that the climb height between the gaps is not too high.
                            if ((top - bot) >= walkableHeight && Math.Abs((int)ns.y - (int)s.y) <= walkableClimb) {
                                // Mark direction as walkable.
                                int lidx = k - (int)nc.index;
                                if (lidx < 0 || lidx > MAX_LAYERS) {
                                    tooHighNeighbour = Math.Max(tooHighNeighbour, lidx);
                                    continue;
                                }
                                rcSetCon(ref s, dir, lidx);
                                break;
                            }
                        }
						
                    }

					chf.spans[i] = s;
                }
            }
        }

        if (tooHighNeighbour > MAX_LAYERS) {
            ctx.log(rcLogCategory.RC_LOG_ERROR, "rcBuildCompactHeightfield: Heightfield has too many layers " + tooHighNeighbour + " (max: " + MAX_LAYERS + ")");
        }

        ctx.stopTimer(rcTimerLabel.RC_TIMER_BUILD_COMPACTHEIGHTFIELD);

        return true;
    }


    /*
    static int getHeightfieldMemoryUsage(const rcHeightfield& hf)
    {
        int size = 0;
        size += sizeof(hf);
        size += hf.width * hf.height * sizeof(rcSpan*);
	
        rcSpanPool* pool = hf.pools;
        while (pool)
        {
            size += (sizeof(rcSpanPool) - sizeof(rcSpan)) + sizeof(rcSpan)*RC_SPANS_PER_POOL;
            pool = pool.next;
        }
        return size;
    }

    static int getCompactHeightFieldMemoryusage(const rcCompactHeightfield& chf)
    {
        int size = 0;
        size += sizeof(rcCompactHeightfield);
        size += sizeof(rcCompactSpan) * chf.spanCount;
        size += sizeof(rcCompactCell) * chf.width * chf.height;
        return size;
    }
    */

    /// @class rcContext
    /// @par
    ///
    /// This class does not provide logging or timer functionality on its 
    /// own.  Both must be provided by a concrete implementation 
    /// by overriding the protected member functions.  Also, this class does not 
    /// provide an interface for extracting log messages. (Only adding them.) 
    /// So concrete implementations must provide one.
    ///
    /// If no logging or timers are required, just pass an instance of this 
    /// class through the Recast build process.
    ///

    /// @par
    ///
    /// Example:
    /// @code
    /// // Where ctx is an instance of rcContext and filepath is a char array.
    /// ctx.log(rcLogCategory.RC_LOG_ERROR, "buildTiledNavigation: Could not load '%s'", filepath);
    /// @endcode
    /// Provides an interface for optional logging and performance tracking of the Recast 
    /// build process.
    /// @ingroup recast
    public class rcContext {

        /// True if logging is enabled.
        bool m_logEnabled = true;

        /// True if the performance timers are enabled.
        bool m_timerEnabled = true;

        /// Contructor.
        ///  @param[in]		state	TRUE if the logging and performance timers should be enabled.  [Default: true]
        public rcContext(bool state = true) {
            m_logEnabled = state;
            m_timerEnabled = state;
        }

        /// Enables or disables logging.
        ///  @param[in]		state	TRUE if logging should be enabled.
        public void enableLog(bool state) {
            m_logEnabled = state;
        }

        /// Clears all log entries.
        public void resetLog() {
            if (m_logEnabled) {
                doResetLog();
            }
        }

        /// Logs a message.
        ///  @param[in]		category	The category of the message.
        ///  @param[in]		message		The message.
        //public void log(rcLogCategory category, string message);
        public void log(rcLogCategory category, string message) {
            if (!m_logEnabled) {
                return;
            }

            doLog(category, message);
        }

        /// Enables or disables the performance timers.
        ///  @param[in]		state	TRUE if timers should be enabled.
        public void enableTimer(bool state) {
            m_timerEnabled = state;
        }

        /// Clears all peformance timers. (Resets all to unused.)
        public void resetTimers() {
            if (m_timerEnabled) {
                doResetTimers();
            }
        }

        /// Starts the specified performance timer.
        ///  @param	label	The category of timer.
        public void startTimer(rcTimerLabel label) {
            if (m_timerEnabled) {
                doStartTimer(label);
            }
        }

        /// Stops the specified performance timer.
        ///  @param	label	The category of the timer.
        public void stopTimer(rcTimerLabel label) {
            if (m_timerEnabled) {
                doStopTimer(label);
            }
        }

        /// Returns the total accumulated time of the specified performance timer.
        ///  @param	label	The category of the timer.
        ///  @return The accumulated time of the timer, or -1 if timers are disabled or the timer has never been started.
        public long getAccumulatedTime(rcTimerLabel label) {
            return m_timerEnabled ? doGetAccumulatedTime(label) : -1;
        }
        public double getAccumulatedTimeHiResolution(rcTimerLabel label) {
            return m_timerEnabled ? doGetAccumulatedTimeHiResolution(label) : -1.0;
        }


        /// Clears all log entries.
        protected virtual void doResetLog() { }

        /// Logs a message.
        ///  @param[in]		category	The category of the message.
        ///  @param[in]		msg			The formatted message.
        protected virtual void doLog(rcLogCategory category, string msg) { //int len unnecessary because c# string
        }

        /// Clears all timers. (Resets all to unused.)
        protected virtual void doResetTimers() {
        }

        /// Starts the specified performance timer.
        ///  @param[in]		label	The category of timer.
        protected virtual void doStartTimer(rcTimerLabel label) {
        }

        /// Stops the specified performance timer.
        ///  @param[in]		label	The category of the timer.
        protected virtual void doStopTimer(rcTimerLabel label) {
        }

        /// Returns the total accumulated time of the specified performance timer.
        ///  @param[in]		label	The category of the timer.
        ///  @return The accumulated time of the timer, or -1 if timers are disabled or the timer has never been started.
        protected virtual long doGetAccumulatedTime(rcTimerLabel label) {
            return -1;
        }

        //C# port: alternate return type to use high precision timer on platforms where it's available
        /// Returns the total accumulated time of the specified performance timer.
        ///  @param[in]		label	The category of the timer.
        ///  @return The accumulated time of the timer, or -1 if timers are disabled or the timer has never been started.
        protected virtual double doGetAccumulatedTimeHiResolution(rcTimerLabel label) {
            return -1.0;
        }

    };

    /// Specifies a configuration to use when performing Recast builds.
    /// @ingroup recast
    public class rcConfig {
        /// The width of the field along the x-axis. [Limit: >= 0] [Units: vx]
        public int width;

        /// The height of the field along the z-axis. [Limit: >= 0] [Units: vx]
        public int height;

        /// The width/height size of tile's on the xz-plane. [Limit: >= 0] [Units: vx]
        public int tileSize;

        /// The size of the non-navigable border around the heightfield. [Limit: >=0] [Units: vx]
        public int borderSize;

        /// The xz-plane cell size to use for fields. [Limit: > 0] [Units: wu] 
        public float cs;

        /// The y-axis cell size to use for fields. [Limit: > 0] [Units: wu]
        public float ch;

        /// The minimum bounds of the field's AABB. [(x, y, z)] [Units: wu]
        public float[] bmin = new float[3];

        /// The maximum bounds of the field's AABB. [(x, y, z)] [Units: wu]
        public float[] bmax = new float[3];

        /// The maximum slope that is considered walkable. [Limits: 0 <= value < 90] [Units: Degrees] 
        public float walkableSlopeAngle;

        /// Minimum floor to 'ceiling' height that will still allow the floor area to 
        /// be considered walkable. [Limit: >= 3] [Units: vx] 
        public int walkableHeight;

        /// Maximum ledge height that is considered to still be traversable. [Limit: >=0] [Units: vx] 
        public int walkableClimb;

        /// The distance to erode/shrink the walkable area of the heightfield away from 
        /// obstructions.  [Limit: >=0] [Units: vx] 
        public int walkableRadius;

        /// The maximum allowed length for contour edges along the border of the mesh. [Limit: >=0] [Units: vx] 
        public int maxEdgeLen;

        /// The maximum distance a simplfied contour's border edges should deviate 
        /// the original raw contour. [Limit: >=0] [Units: vx]
        public float maxSimplificationError;

        /// The minimum number of cells allowed to form isolated island areas. [Limit: >=0] [Units: vx] 
        public int minRegionArea;

        /// Any regions with a span count smaller than this value will, if possible, 
        /// be merged with larger regions. [Limit: >=0] [Units: vx] 
        public int mergeRegionArea;

        /// The maximum number of vertices allowed for polygons generated during the 
        /// contour to polygon conversion process. [Limit: >= 3] 
        public int maxVertsPerPoly;

        /// Sets the sampling distance to use when generating the detail mesh.
        /// (For height detail only.) [Limits: 0 or >= 0.9] [Units: wu] 
        public float detailSampleDist;

        /// The maximum distance the detail mesh surface should deviate from heightfield
        /// data. (For height detail only.) [Limit: >=0] [Units: wu] 
        public float detailSampleMaxError;
    };



    /// Represents a span in a heightfield.
    /// @see rcHeightfield
    public class rcSpan {
        public ushort smin;// : 13;			//< The lower limit of the span. [Limit: < #smax]
        public ushort smax;// : 13;			//< The upper limit of the span. [Limit: <= #RC_SPAN_MAX_HEIGHT]
        public byte area;// : 6;			//< The area id assigned to the span.
        public rcSpan next = null; 			//< The next span higher up in column.
    }
    // A memory pool used for quick allocation of spans within a heightfield.
    // @see rcHeightfield

    public class rcSpanPool {
        public rcSpanPool next = null;					//< The next span pool.
        public rcSpan[] items = new rcSpan[RC_SPANS_PER_POOL];	//< Array of spans in the pool.
        ///
        public rcSpanPool() {
            for (int i = 0; i < items.Length; ++i) {
                items[i] = new rcSpan();
            }
        }
    };

    /// A dynamic heightfield representing obstructed space.
    /// @ingroup recast
    public class rcHeightfield {
        public int width;			//< The width of the heightfield. (Along the x-axis in cell units.)
        public int height;			//< The height of the heightfield. (Along the z-axis in cell units.)
        public float[] bmin = new float[3];  	//< The minimum bounds in world space. [(x, y, z)]
        public float[] bmax = new float[3];		//< The maximum bounds in world space. [(x, y, z)]
        public float cs;			//< The size of each cell. (On the xz-plane.)
        public float ch;			//< The height of each cell. (The minimum increment along the y-axis.)
        public rcSpan[] spans = null;//**  //< Heightfield of spans (width*height).
        public rcSpanPool pools = null;	//< Linked list of span pools.
        public rcSpan freelist = null;	//< The next free span.
    };

    /// Provides information on the content of a cell column in a compact heightfield. 
    public struct rcCompactCell {
        public uint index;// : 24;	//< Index to the first span in the column.
        public ushort count;// : 8;		//< Number of spans in the column.
    };

    /// Represents a span of unobstructed space within a compact heightfield.
    public struct rcCompactSpan {
        public ushort y;			//< The lower extent of the span. (Measured from the heightfield's base.)
        public ushort reg;			//< The id of the region the span belongs to. (Or zero if not in a region.)
        public uint con;// : 24;		//< Packed neighbor connection data.
        public ushort h;// : 8;			//< The height of the span.  (Measured from #y.)
    };

    /// A compact, static heightfield representing unobstructed space.
    /// @ingroup recast
    public class rcCompactHeightfield {
        public int width;					//< The width of the heightfield. (Along the x-axis in cell units.)
        public int height;					//< The height of the heightfield. (Along the z-axis in cell units.)
        public int spanCount;				//< The number of spans in the heightfield.
        public int walkableHeight;			//< The walkable height used during the build of the field.  (See: rcConfig::walkableHeight)
        public int walkableClimb;			//< The walkable climb used during the build of the field. (See: rcConfig::walkableClimb)
        public int borderSize;				//< The AABB border size used during the build of the field. (See: rcConfig::borderSize)
        public ushort maxDistance;	//< The maximum distance value of any span within the field. 
        public ushort maxRegions;	//< The maximum region id of any span within the field. 
        public float[] bmin = new float[3];				//< The minimum bounds in world space. [(x, y, z)]
        public float[] bmax = new float[3];				//< The maximum bounds in world space. [(x, y, z)]
        public float cs;					//< The size of each cell. (On the xz-plane.)
        public float ch;					//< The height of each cell. (The minimum increment along the y-axis.)
        public rcCompactCell[] cells = null;		//< Array of cells. [Size: #width*#height]
        public rcCompactSpan[] spans = null;		//< Array of spans. [Size: #spanCount]
        public ushort[] dist = null;		//< Array containing border distance data. [Size: #spanCount]
        public byte[] areas = null;		//< Array containing area id data. [Size: #spanCount]
    };

    /// Represents a heightfield layer within a layer set.
    /// @see rcHeightfieldLayerSet
    public class rcHeightfieldLayer {
        public float[] bmin = new float[3];				//< The minimum bounds in world space. [(x, y, z)]
        public float[] bmax = new float[3];				//< The maximum bounds in world space. [(x, y, z)]
        public float cs;					//< The size of each cell. (On the xz-plane.)
        public float ch;					//< The height of each cell. (The minimum increment along the y-axis.)
        public int width;					//< The width of the heightfield. (Along the x-axis in cell units.)
        public int height;					//< The height of the heightfield. (Along the z-axis in cell units.)
        public int minx;					//< The minimum x-bounds of usable data.
        public int maxx;					//< The maximum x-bounds of usable data.
        public int miny;					//< The minimum y-bounds of usable data. (Along the z-axis.)
        public int maxy;					//< The maximum y-bounds of usable data. (Along the z-axis.)
        public int hmin;					//< The minimum height bounds of usable data. (Along the y-axis.)
        public int hmax;					//< The maximum height bounds of usable data. (Along the y-axis.)
        public byte[] heights;		//< The heightfield. [Size: (width - borderSize*2) * (h - borderSize*2)]
        public byte[] areas;		//< Area ids. [Size: Same as #heights]
        public byte[] cons;		//< Packed neighbor connection information. [Size: Same as #heights]
    };

    /// Represents a set of heightfield layers.
    /// @ingroup recast
    /// @see rcAllocHeightfieldLayerSet, rcFreeHeightfieldLayerSet 
    public class rcHeightfieldLayerSet {
        public rcHeightfieldLayer[] layers = null;			//< The layers in the set. [Size: #nlayers]
        public int nlayers;						//< The number of layers in the set.
    };

    /// Represents a simple, non-overlapping contour in field space.
    public struct rcContour {
        public int[] verts;			//< Simplified contour vertex and connection data. [Size: 4 * #nverts]
        public int nverts;			//< The number of vertices in the simplified contour. 
        public int[] rverts;		//< Raw contour vertex and connection data. [Size: 4 * #nrverts]
        public int nrverts;		//< The number of vertices in the raw contour. 
        public ushort reg;	//< The region id of the contour.
        public byte area;	//< The area id of the contour.
        ///
        public void dumpToTxt(StreamWriter stream) {
            stream.WriteLine("\treg: " + reg);
            stream.WriteLine("\tarea: " + area);
            stream.WriteLine("\tnverts: " + nverts);
            for (int i = 0; i < nverts; ++i) {
                int vIndex = i * 4;
                stream.WriteLine("\t\tverts[" + i + "]: x:" + verts[vIndex] + " y:" + verts[vIndex + 1] + " z:" + verts[vIndex + 2] + " ?:" + verts[vIndex + 3]);
            }
            stream.WriteLine("\tnrverts: " + nrverts);
            for (int i = 0; i < nrverts; ++i) {
                int vIndex = i * 4;
                stream.WriteLine("\t\trverts[" + i + "]: x:" + rverts[vIndex] + " y:" + rverts[vIndex + 1] + " z:" + rverts[vIndex + 2] + " ?:" + rverts[vIndex + 3]);
            }
        }
		public string dumpToString() {
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("\treg: " + reg);
			sb.AppendLine("\tarea: " + area);
			sb.AppendLine("\tnverts: " + nverts);
			for (int i = 0; i < nverts; ++i) {
				int vIndex = i * 4;
				sb.AppendLine("\t\tverts[" + i + "]: x:" + verts[vIndex] + " y:" + verts[vIndex + 1] + " z:" + verts[vIndex + 2] + " ?:" + verts[vIndex + 3]);
			}
			sb.AppendLine("\tnrverts: " + nrverts);
			for (int i = 0; i < nrverts; ++i) {
				int vIndex = i * 4;
				sb.AppendLine("\t\trverts[" + i + "]: x:" + rverts[vIndex] + " y:" + rverts[vIndex + 1] + " z:" + rverts[vIndex + 2] + " ?:" + rverts[vIndex + 3]);
			}
			return sb.ToString();
		}
    };

    /// Represents a group of related contours.
    /// @ingroup recast
    public class rcContourSet {
        public rcContour[] conts = null;	//< An array of the contours in the set. [Size: #nconts]
        public int nconts;			//< The number of contours in the set.
        public float[] bmin = new float[3];  	//< The minimum bounds in world space. [(x, y, z)]
        public float[] bmax = new float[3];		//< The maximum bounds in world space. [(x, y, z)]
        public float cs;			//< The size of each cell. (On the xz-plane.)
        public float ch;			//< The height of each cell. (The minimum increment along the y-axis.)
        public int width;			//< The width of the set. (Along the x-axis in cell units.) 
        public int height;			//< The height of the set. (Along the z-axis in cell units.) 
        public int borderSize;		//< The AABB border size used to generate the source data from which the contours were derived.
        ///
		public override string ToString() {
			StringBuilder sb = new StringBuilder();
			
			sb.AppendLine("nconts: " + nconts);
			sb.AppendLine("bmin: " + bmin[0] + " " + bmin[1] + " " + bmin[2]);
			sb.AppendLine("bmax: " + bmax[0] + " " + bmax[1] + " " + bmax[2]);
			sb.AppendLine("cs: " + cs);
			sb.AppendLine("ch: " + ch);
			sb.AppendLine("width: " + width);
			sb.AppendLine("height: " + height);
			sb.AppendLine("bordersize: " + borderSize);
			
			for (int i = 0; i < nconts; ++i) {
				sb.Append("contour[" + i + "]: ");
				sb.AppendLine(conts[i].ToString());
			}
			
			return sb.ToString();
		}
    };

    /// Represents a polygon mesh suitable for use in building a navigation mesh. 
    /// @ingroup recast
    public class rcPolyMesh {
        public ushort[] verts = null;	//< The mesh vertices. [Form: (x, y, z) * #nverts]
        public ushort[] polys = null;	//< Polygon and neighbor data. [Length: #maxpolys * 2 * #nvp]
        public ushort[] regs = null;	//< The region id assigned to each polygon. [Length: #maxpolys]
        public ushort[] flags = null;	//< The user defined flags for each polygon. [Length: #maxpolys]
        public byte[] areas = null;	//< The area id assigned to each polygon. [Length: #maxpolys]
        public int nverts;				//< The number of vertices.
        public int npolys;				//< The number of polygons.
        public int maxpolys;			//< The number of allocated polygons.
        public int nvp;				//< The maximum number of vertices per polygon.
        public float[] bmin = new float[3];			//< The minimum bounds in world space. [(x, y, z)]
        public float[] bmax = new float[3];			//< The maximum bounds in world space. [(x, y, z)]
        public float cs;				//< The size of each cell. (On the xz-plane.)
        public float ch;				//< The height of each cell. (The minimum increment along the y-axis.)
        public int borderSize;			//< The AABB border size used to generate the source data from which the mesh was derived.
        ///
		public override string ToString() {
			StringBuilder sb = new StringBuilder();
			
			sb.AppendLine("bmin: " + bmin[0] + " " + bmin[1] + " " + bmin[2]);
			sb.AppendLine("bmax: " + bmax[0] + " " + bmax[1] + " " + bmax[2]);
			sb.AppendLine("cs: " + cs);
			sb.AppendLine("ch: " + ch);
			sb.AppendLine("bordersize: " + borderSize);
			
			sb.AppendLine("nverts: " + nverts);
			for (int i = 0; i < nverts; ++i) {
				int vIndex = i * 3;
				sb.AppendLine("\tverts[" + i + "]: x:" + verts[vIndex] + " y:" + verts[vIndex + 1] + " z:" + verts[vIndex + 2]);
			}
			sb.AppendLine("\tmaxpolys: " + maxpolys);
			sb.AppendLine("\tnvp: " + nvp);
			sb.AppendLine("\tnpolys: " + npolys);
			for (int i = 0; i < maxpolys; ++i) {
				int vIndex = i * nvp;
				sb.Append("\t\tpolys[" + i + "]: ");
				for (int j = 0; j < nvp; ++j) {
					sb.Append(" " + j + ":" + polys[vIndex + j]);
				}
				
				vIndex += nvp;
				sb.AppendLine();
				sb.Append("\t\tneighbor[" + i + "]: ");
				for (int j = 0; j < nvp; ++j) {
					sb.Append(" " + j + ":" + polys[vIndex + j]);
				}
				sb.AppendLine();
			}
			
			for (int i = 0; i < maxpolys; ++i) {
				sb.AppendLine("regs[" + i + "]: " + regs[i]);
			}
			sb.AppendLine();
			for (int i = 0; i < flags.Length; ++i) {
				sb.AppendLine("flags[" + i + "]: " + flags[i]);
			}
			
			return sb.ToString();
		}

		public string ToObj(){
			StringBuilder sb = new StringBuilder();
			
			sb.AppendLine("# Recast Navmesh");
			sb.AppendLine("o NavMesh");
			
			sb.AppendLine();
			
			for (int i = 0; i < nverts; ++i) {
				//ushort* v = &pmesh.verts[i*3];
				int vIndex = i * 3;
				float x = bmin[0] + verts[vIndex + 0] * cs;
				float y = bmin[1] + (verts[vIndex + 1] + 1) * ch + 0.1f;
				float z = bmin[2] + verts[vIndex + 2] * cs;
				//ioprintf(io, "v %f %f %f\n", x,y,z);
				sb.AppendLine("v " + x + " " + y + " " + z);
			}
			
			sb.AppendLine();
			
			for (int i = 0; i < npolys; ++i) {
				//const unsigned short* p = &pmesh.polys[i*nvp*2];
				int pIndex = i * nvp * 2;
				for (int j = 2; j < nvp; ++j) {
					if (polys[pIndex + j] == RC_MESH_NULL_IDX)
						break;
					//ioprintf(io, "f %d %d %d\n", p[0]+1, p[j-1]+1, p[j]+1); 
					int a = polys[pIndex] + 1;
					int b = polys[pIndex + j - 1] + 1;
					int c = polys[pIndex + j] + 1;
					sb.AppendLine("f " + a + " " + b + " " + c);
				}
			}
			return sb.ToString ();
		}
    };

    /// Contains triangle meshes that represent detailed height data associated 
    /// with the polygons in its associated polygon mesh object.
    /// @ingroup recast
    public class rcPolyMeshDetail {
        public uint[] meshes = null;	//< The sub-mesh data. [Size: 4*#nmeshes] 
        public float[] verts = null;			//< The mesh vertices. [Size: 3*#nverts] 
        public byte[] tris = null;	//< The mesh triangles. [Size: 4*#ntris] 
        public int nmeshes;			//< The number of sub-meshes defined by #meshes.
        public int nverts;				//< The number of vertices in #verts.
        public int ntris;				//< The number of triangles in #tris.
        
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine("nmeshes: " + nmeshes);
			for (int i = 0; i < nmeshes; ++i) {
				int vIndex = i * 4;
				sb.AppendLine("\tmeshes[" + i + "]: a:" + meshes[vIndex] + " b:" + meshes[vIndex + 1] + " c:" + meshes[vIndex + 2] + " d:" + meshes[vIndex + 3]);
			}
			sb.AppendLine("nverts: " + nverts);
			for (int i = 0; i < nverts; ++i) {
				int vIndex = i * 3;
				sb.AppendLine("\tverts[" + i + "]: x:" + verts[vIndex] + " y:" + verts[vIndex + 1] + " z:" + verts[vIndex + 2]);
			}
			sb.AppendLine("ntris: " + ntris);
			for (int i = 0; i < ntris; ++i) {
				int vIndex = i * 4;
				sb.AppendLine("\ttris[" + i + "]: a:" + tris[vIndex] + " b:" + tris[vIndex + 1] + " c:" + tris[vIndex + 2] + " d:" + tris[vIndex + 3]);
			}

			return sb.ToString();
		}

		public string ToObj()
		{
			StringBuilder sb = new StringBuilder();
			
			sb.AppendLine("# Recast C# Navmesh\n");
			sb.AppendLine("o NavMesh\n");
			
			sb.AppendLine("\n");
			
			for (int i = 0; i < nverts; ++i) {
				int vIndex = i * 3;
				sb.AppendLine("v " + verts[vIndex + 0] + " " + verts[vIndex + 1] + " " + verts[vIndex + 2]);
			}
			
			sb.AppendLine();
			
			for (int i = 0; i < nmeshes; ++i) {
				//uint* m = &dmesh.meshes[i*4];
				int mIndex = i * 4;
				uint bverts = meshes[mIndex + 0];
				uint btris = meshes[mIndex + 2];
				uint _ntris = meshes[mIndex + 3];
				uint trisIndex = btris * 4;
				for (uint j = 0; j < _ntris; ++j) {
					sb.AppendLine("f "
					                 + ((int)(bverts + tris[trisIndex + j * 4 + 0]) + 1) + " "
					                 + ((int)(bverts + tris[trisIndex + j * 4 + 1]) + 1) + " "
					                 + ((int)(bverts + tris[trisIndex + j * 4 + 2]) + 1) + " ");
				}
			}
			
			return sb.ToString();
		}
    };

    /// @name Allocation Functions
    /// Functions used to allocate and de-allocate Recast objects.
    /// @see rcAllocSetCustom
    /// @{

    /// Allocates a heightfield object using the Recast allocator.
    ///  @return A heightfield that is ready for initialization, or null on failure.
    ///  @ingroup recast
    ///  @see rcCreateHeightfield, rcFreeHeightField
    //rcHeightfield* rcAllocHeightfield();

    /// Frees the specified heightfield object using the Recast allocator.
    ///  @param[in]		hf	A heightfield allocated using #rcAllocHeightfield
    ///  @ingroup recast
    ///  @see rcAllocHeightfield
    //void rcFreeHeightField(rcHeightfield* hf);

    /// Allocates a compact heightfield object using the Recast allocator.
    ///  @return A compact heightfield that is ready for initialization, or null on failure.
    ///  @ingroup recast
    ///  @see rcBuildCompactHeightfield, rcFreeCompactHeightfield
    //rcCompactHeightfield* rcAllocCompactHeightfield();

    /// Frees the specified compact heightfield object using the Recast allocator.
    ///  @param[in]		chf		A compact heightfield allocated using #rcAllocCompactHeightfield
    ///  @ingroup recast
    ///  @see rcAllocCompactHeightfield
    //void rcFreeCompactHeightfield(rcCompactHeightfield* chf);

    /// Allocates a heightfield layer set using the Recast allocator.
    ///  @return A heightfield layer set that is ready for initialization, or null on failure.
    ///  @ingroup recast
    ///  @see rcBuildHeightfieldLayers, rcFreeHeightfieldLayerSet
    //rcHeightfieldLayerSet* rcAllocHeightfieldLayerSet();

    /// Frees the specified heightfield layer set using the Recast allocator.
    ///  @param[in]		lset	A heightfield layer set allocated using #rcAllocHeightfieldLayerSet
    ///  @ingroup recast
    ///  @see rcAllocHeightfieldLayerSet
    //void rcFreeHeightfieldLayerSet(rcHeightfieldLayerSet* lset);

    /// Allocates a contour set object using the Recast allocator.
    ///  @return A contour set that is ready for initialization, or null on failure.
    ///  @ingroup recast
    ///  @see rcBuildContours, rcFreeContourSet
    //rcContourSet* rcAllocContourSet();

    /// Frees the specified contour set using the Recast allocator.
    ///  @param[in]		cset	A contour set allocated using #rcAllocContourSet
    ///  @ingroup recast
    ///  @see rcAllocContourSet
    //void rcFreeContourSet(rcContourSet* cset);

    /// Allocates a polygon mesh object using the Recast allocator.
    ///  @return A polygon mesh that is ready for initialization, or null on failure.
    ///  @ingroup recast
    ///  @see rcBuildPolyMesh, rcFreePolyMesh
    //rcPolyMesh* rcAllocPolyMesh();

    /// Frees the specified polygon mesh using the Recast allocator.
    ///  @param[in]		pmesh	A polygon mesh allocated using #rcAllocPolyMesh
    ///  @ingroup recast
    ///  @see rcAllocPolyMesh
    //void rcFreePolyMesh(rcPolyMesh* pmesh);

    /// Allocates a detail mesh object using the Recast allocator.
    ///  @return A detail mesh that is ready for initialization, or null on failure.
    ///  @ingroup recast
    ///  @see rcBuildPolyMeshDetail, rcFreePolyMeshDetail
    //rcPolyMeshDetail* rcAllocPolyMeshDetail();

    /// Frees the specified detail mesh using the Recast allocator.
    ///  @param[in]		dmesh	A detail mesh allocated using #rcAllocPolyMeshDetail
    ///  @ingroup recast
    ///  @see rcAllocPolyMeshDetail
    //void rcFreePolyMeshDetail(rcPolyMeshDetail* dmesh);

    /// The number of spans allocated per span spool.
    /// @see rcSpanPool
    public const int RC_SPANS_PER_POOL = 2048;

    /// Heighfield border flag.
    /// If a heightfield region ID has this bit set, then the region is a border 
    /// region and its spans are considered unwalkable.
    /// (Used during the region and contour build process.)
    /// @see rcCompactSpan::reg

    public const ushort RC_BORDER_REG = 0x8000;

    /// Border vertex flag.
    /// If a region ID has this bit set, then the associated element lies on
    /// a tile border. If a contour vertex's region ID has this bit set, the 
    /// vertex will later be removed in order to match the segments and vertices 
    /// at tile boundaries.
    /// (Used during the build process.)
    /// @see rcCompactSpan::reg, #rcContour::verts, #rcContour::rverts
    public const int RC_BORDER_VERTEX = 0x10000;

    /// Area border flag.
    /// If a region ID has this bit set, then the associated element lies on
    /// the border of an area.
    /// (Used during the region and contour build process.)
    /// @see rcCompactSpan::reg, #rcContour::verts, #rcContour::rverts
    public const int RC_AREA_BORDER = 0x20000;

    /// Contour build flags.
    /// @see rcBuildContours
    public enum rcBuildContoursFlags {
        RC_CONTOUR_TESS_WALL_EDGES = 0x01,	//< Tessellate solid (impassable) edges during contour simplification.
        RC_CONTOUR_TESS_AREA_EDGES = 0x02,	//< Tessellate edges between areas during contour simplification.
    };

    /// Applied to the region id field of contour vertices in order to extract the region id.
    /// The region id field of a vertex may have several flags applied to it.  So the
    /// fields value can't be used directly.
    /// @see rcContour::verts, rcContour::rverts
    public const int RC_CONTOUR_REG_MASK = 0xffff;

    /// An value which indicates an invalid index within a mesh.
    /// @note This does not necessarily indicate an error.
    /// @see rcPolyMesh::polys
    public const ushort RC_MESH_NULL_IDX = 0xffff;

    /// Represents the null area.
    /// When a data element is given this value it is considered to no longer be 
    /// assigned to a usable area.  (E.g. It is unwalkable.)
    public const byte RC_NULL_AREA = 0;

    /// The default area id used to indicate a walkable polygon. 
    /// This is also the maximum allowed area id, and the only non-null area id 
    /// recognized by some steps in the build process. 
    public const byte RC_WALKABLE_AREA = 63;

    /// The value returned by #rcGetCon if the specified direction is not connected
    /// to another span. (Has no neighbor.)
    public const int RC_NOT_CONNECTED = 0x3f;

    /// @name General helper functions
    /// @{

    /// Used to ignore a function parameter.  VS complains about unused parameters
    /// and this silences the warning.
    ///  @param [in] _ Unused parameter
    public static void rcIgnoreUnused<T>(T t) { }

    //Use C# for this kind of things
    /// Swaps the values of the two parameters.
    ///  @param[in,out]	a	Value A
    ///  @param[in,out]	b	Value B
    //public void rcSwap<T>(T a, T b) { T t = a; a = b; b = t; }
    static void rcSwap<T>(ref T lhs, ref T rhs) {
        T temp = lhs;
        lhs = rhs;
        rhs = temp;
    }

    /// Returns the minimum of two values.
    ///  @param[in]		a	Value A
    ///  @param[in]		b	Value B
    ///  @return The minimum of the two values.
    //public T Math.Min<T>(T a, T b) { 
    //    return a < b ? a : b; 
    //}

    /// Returns the maximum of two values.
    ///  @param[in]		a	Value A
    ///  @param[in]		b	Value B
    ///  @return The maximum of the two values.
    //template<class T> inline T Math.Max(T a, T b) { return a > b ? a : b; }

    /// Returns the absolute value.
    ///  @param[in]		a	The value.
    ///  @return The absolute value of the specified value.
    //template<class T> inline T rcAbs(T a) { return a < 0 ? -a : a; }

    /// Returns the square of the value.
    ///  @param[in]		a	The value.
    ///  @return The square of the value.
    //template<class T> inline T rcSqr(T a) { return a*a; }

    /// Clamps the value to the specified range.
    ///  @param[in]		v	The value to clamp.
    ///  @param[in]		mn	The minimum permitted return value.
    ///  @param[in]		mx	The maximum permitted return value.
    ///  @return The value, clamped to the specified range.
    //template<class T> inline T rcClamp(T v, T mn, T mx) { return v < mn ? mn : (v > mx ? mx : v); }
    public static int rcClamp(int v, int mn, int mx) {
        return v < mn ? mn : (v > mx ? mx : v);
    }

    /// Returns the square root of the value.
    ///  @param[in]		x	The value.
    ///  @return The square root of the vlaue.
    //float rcSqrt(float x);

    /// @}
    /// @name Vector helper functions.
    /// @{

    /// Derives the cross product of two vectors. (@p v1 x @p v2)
    ///  @param[out]	dest	The cross product. [(x, y, z)]
    ///  @param[in]		v1		A Vector [(x, y, z)]
    ///  @param[in]		v2		A vector [(x, y, z)]

    public static void rcVcross(float[] dest, float[] v1, float[] v2) {
        dest[0] = v1[1] * v2[2] - v1[2] * v2[1];
        dest[1] = v1[2] * v2[0] - v1[0] * v2[2];
        dest[2] = v1[0] * v2[1] - v1[1] * v2[0];
    }
    public static void rcVcross(float[] dest, int destStart, float[] v1, int v1Start, float[] v2, int v2Start) {
        dest[destStart + 0] = v1[v1Start + 1] * v2[v2Start + 2] - v1[v1Start + 2] * v2[v2Start + 1];
        dest[destStart + 1] = v1[v1Start + 2] * v2[v2Start + 0] - v1[v1Start + 0] * v2[v2Start + 2];
        dest[destStart + 2] = v1[v1Start + 0] * v2[v2Start + 1] - v1[v1Start + 1] * v2[v2Start + 0];
    }

    /// Derives the dot product of two vectors. (@p v1 . @p v2)
    ///  @param[in]		v1	A Vector [(x, y, z)]
    ///  @param[in]		v2	A vector [(x, y, z)]
    /// @return The dot product.
    public static float rcVdot(float[] v1, float[] v2) {
        return v1[0] * v2[0] + v1[1] * v2[1] + v1[2] * v2[2];
    }

    /// Performs a scaled vector addition. (@p v1 + (@p v2 * @p s))
    ///  @param[out]	dest	The result vector. [(x, y, z)]
    ///  @param[in]		v1		The base vector. [(x, y, z)]
    ///  @param[in]		v2		The vector to scale and add to @p v1. [(x, y, z)]
    ///  @param[in]		s		The amount to scale @p v2 by before adding to @p v1.
    public static void rcVmad(float[] dest, float[] v1, float[] v2, float s) {
        dest[0] = v1[0] + v2[0] * s;
        dest[1] = v1[1] + v2[1] * s;
        dest[2] = v1[2] + v2[2] * s;
    }

    /// Performs a vector addition. (@p v1 + @p v2)
    ///  @param[out]	dest	The result vector. [(x, y, z)]
    ///  @param[in]		v1		The base vector. [(x, y, z)]
    ///  @param[in]		v2		The vector to add to @p v1. [(x, y, z)]
    public static void rcVadd(float[] dest, float[] v1, float[] v2) {
        dest[0] = v1[0] + v2[0];
        dest[1] = v1[1] + v2[1];
        dest[2] = v1[2] + v2[2];
    }

    /// Performs a vector subtraction. (@p v1 - @p v2)
    ///  @param[out]	dest	The result vector. [(x, y, z)]
    ///  @param[in]		v1		The base vector. [(x, y, z)]
    ///  @param[in]		v2		The vector to subtract from @p v1. [(x, y, z)]
    public static void rcVsub(float[] dest, float[] v1, float[] v2) {
        dest[0] = v1[0] - v2[0];
        dest[1] = v1[1] - v2[1];
        dest[2] = v1[2] - v2[2];
    }

    public static void rcVsub(float[] dest, int destStart, float[] v1, int v1Start, float[] v2, int v2Start) {
        dest[destStart + 0] = v1[v1Start + 0] - v2[v2Start + 0];
        dest[destStart + 1] = v1[v1Start + 1] - v2[v2Start + 1];
        dest[destStart + 2] = v1[v1Start + 2] - v2[v2Start + 2];
    }

    /// Selects the minimum value of each element from the specified vectors.
    ///  @param[in,out]	mn	A vector.  (Will be updated with the result.) [(x, y, z)]
    ///  @param[in]		v	A vector. [(x, y, z)]
    public static void rcVmin(float[] mn, float[] v) {
        mn[0] = Math.Min(mn[0], v[0]);
        mn[1] = Math.Min(mn[1], v[1]);
        mn[2] = Math.Min(mn[2], v[2]);
    }

    public static void rcVmin(float[] mn, int mnStart, float[] v, int vStart) {
        mn[0 + mnStart] = Math.Min(mn[0 + mnStart], v[0 + vStart]);
        mn[1 + mnStart] = Math.Min(mn[1 + mnStart], v[1 + vStart]);
        mn[2 + mnStart] = Math.Min(mn[2 + mnStart], v[2 + vStart]);
    }


    /// Selects the maximum value of each element from the specified vectors.
    ///  @param[in,out]	mx	A vector.  (Will be updated with the result.) [(x, y, z)]
    ///  @param[in]		v	A vector. [(x, y, z)]
    public static void rcVmax(float[] mx, float[] v) {
        mx[0] = Math.Max(mx[0], v[0]);
        mx[1] = Math.Max(mx[1], v[1]);
        mx[2] = Math.Max(mx[2], v[2]);
    }

    public static void rcVmax(float[] mx, int mxStart, float[] v, int vStart) {
        mx[0 + mxStart] = Math.Max(mx[0 + mxStart], v[0 + vStart]);
        mx[1 + mxStart] = Math.Max(mx[1 + mxStart], v[1 + vStart]);
        mx[2 + mxStart] = Math.Max(mx[2 + mxStart], v[2 + vStart]);
    }

    /// Performs a vector copy.
    ///  @param[out]	dest	The result. [(x, y, z)]
    ///  @param[in]		v		The vector to copy. [(x, y, z)]
    public static void rcVcopy(float[] dest, float[] v) {
        dest[0] = v[0];
        dest[1] = v[1];
        dest[2] = v[2];
    }
    public static void rcVcopy(float[] dest, int destStart, float[] v, int vStart) {
        dest[destStart + 0] = v[vStart + 0];
        dest[destStart + 1] = v[vStart + 1];
        dest[destStart + 2] = v[vStart + 2];
    }

    /// Returns the distance between two points.
    ///  @param[in]		v1	A point. [(x, y, z)]
    ///  @param[in]		v2	A point. [(x, y, z)]
    /// @return The distance between the two points.
    public static float rcVdist(float[] v1, float[] v2) {
        float dx = v2[0] - v1[0];
        float dy = v2[1] - v1[1];
        float dz = v2[2] - v1[2];
        return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    /// Returns the square of the distance between two points.
    ///  @param[in]		v1	A point. [(x, y, z)]
    ///  @param[in]		v2	A point. [(x, y, z)]
    /// @return The square of the distance between the two points.
    public static float rcVdistSqr(float[] v1, float[] v2) {
        float dx = v2[0] - v1[0];
        float dy = v2[1] - v1[1];
        float dz = v2[2] - v1[2];
        return dx * dx + dy * dy + dz * dz;
    }

    /// Normalizes the vector.
    ///  @param[in,out]	v	The vector to normalize. [(x, y, z)]
    public static void rcVnormalize(float[] v) {
        float d = 1.0f / (float)Math.Sqrt((v[0] * v[0]) + (v[1] * v[1]) + (v[2] * v[2]));
        v[0] *= d;
        v[1] *= d;
        v[2] *= d;
    }

    /// @}
    /// @name Heightfield Functions
    /// @see rcHeightfield
    /// @{

    /// Calculates the bounding box of an array of vertices.
    ///  @ingroup recast
    ///  @param[in]		verts	An array of vertices. [(x, y, z) * @p nv]
    ///  @param[in]		nv		The number of vertices in the @p verts array.
    ///  @param[out]	bmin	The minimum bounds of the AABB. [(x, y, z)] [Units: wu]
    ///  @param[out]	bmax	The maximum bounds of the AABB. [(x, y, z)] [Units: wu]
    //void rcCalcBounds(const float* verts, int nv, float* bmin, float* bmax);

    /// Calculates the grid size based on the bounding box and grid cell size.
    ///  @ingroup recast
    ///  @param[in]		bmin	The minimum bounds of the AABB. [(x, y, z)] [Units: wu]
    ///  @param[in]		bmax	The maximum bounds of the AABB. [(x, y, z)] [Units: wu]
    ///  @param[in]		cs		The xz-plane cell size. [Limit: > 0] [Units: wu]
    ///  @param[out]	w		The width along the x-axis. [Limit: >= 0] [Units: vx]
    ///  @param[out]	h		The height along the z-axis. [Limit: >= 0] [Units: vx]
    //void rcCalcGridSize(const float* bmin, const float* bmax, float cs, int* w, int* h);

    /// Initializes a new heightfield.
    ///  @ingroup recast
    ///  @param[in,out]	ctx		The build context to use during the operation.
    ///  @param[in,out]	hf		The allocated heightfield to initialize.
    ///  @param[in]		width	The width of the field along the x-axis. [Limit: >= 0] [Units: vx]
    ///  @param[in]		height	The height of the field along the z-axis. [Limit: >= 0] [Units: vx]
    ///  @param[in]		bmin	The minimum bounds of the field's AABB. [(x, y, z)] [Units: wu]
    ///  @param[in]		bmax	The maximum bounds of the field's AABB. [(x, y, z)] [Units: wu]
    ///  @param[in]		cs		The xz-plane cell size to use for the field. [Limit: > 0] [Units: wu]
    ///  @param[in]		ch		The y-axis cell size to use for field. [Limit: > 0] [Units: wu]
    //bool rcCreateHeightfield(rcContext* ctx, rcHeightfield& hf, int width, int height,
    //						 const float* bmin, const float* bmax,
    //						 float cs, float ch);

    /// Sets the area id of all triangles with a slope below the specified value
    /// to #RC_WALKABLE_AREA.
    ///  @ingroup recast
    ///  @param[in,out]	ctx					The build context to use during the operation.
    ///  @param[in]		walkableSlopeAngle	The maximum slope that is considered walkable.
    ///  									[Limits: 0 <= value < 90] [Units: Degrees]
    ///  @param[in]		verts				The vertices. [(x, y, z) * @p nv]
    ///  @param[in]		nv					The number of vertices.
    ///  @param[in]		tris				The triangle vertex indices. [(vertA, vertB, vertC) * @p nt]
    ///  @param[in]		nt					The number of triangles.
    ///  @param[out]	areas				The triangle area ids. [Length: >= @p nt]
    //void rcMarkWalkableTriangles(rcContext* ctx, const float walkableSlopeAngle, const float* verts, int nv,
    //							 const int* tris, int nt, byte* areas); 

    /// Sets the area id of all triangles with a slope greater than or equal to the specified value to #RC_NULL_AREA.
    ///  @ingroup recast
    ///  @param[in,out]	ctx					The build context to use during the operation.
    ///  @param[in]		walkableSlopeAngle	The maximum slope that is considered walkable.
    ///  									[Limits: 0 <= value < 90] [Units: Degrees]
    ///  @param[in]		verts				The vertices. [(x, y, z) * @p nv]
    ///  @param[in]		nv					The number of vertices.
    ///  @param[in]		tris				The triangle vertex indices. [(vertA, vertB, vertC) * @p nt]
    ///  @param[in]		nt					The number of triangles.
    ///  @param[out]	areas				The triangle area ids. [Length: >= @p nt]
    //void rcClearUnwalkableTriangles(rcContext* ctx, const float walkableSlopeAngle, const float* verts, int nv,
    //const int* tris, int nt, byte* areas); 

    /// Adds a span to the specified heightfield.
    ///  @ingroup recast
    ///  @param[in,out]	ctx				The build context to use during the operation.
    ///  @param[in,out]	hf				An initialized heightfield.
    ///  @param[in]		x				The width index where the span is to be added.
    ///  								[Limits: 0 <= value < rcHeightfield::width]
    ///  @param[in]		y				The height index where the span is to be added.
    ///  								[Limits: 0 <= value < rcHeightfield::height]
    ///  @param[in]		smin			The minimum height of the span. [Limit: < @p smax] [Units: vx]
    ///  @param[in]		smax			The maximum height of the span. [Limit: <= #RC_SPAN_MAX_HEIGHT] [Units: vx]
    ///  @param[in]		area			The area id of the span. [Limit: <= #RC_WALKABLE_AREA)
    ///  @param[in]		flagMergeThr	The merge theshold. [Limit: >= 0] [Units: vx]
    //void rcAddSpan(rcContext* ctx, rcHeightfield& hf, const int x, const int y,
    //			   const ushort smin, const ushort smax,
    //			   const byte area, const int flagMergeThr);

    /// Rasterizes a triangle into the specified heightfield.
    ///  @ingroup recast
    ///  @param[in,out]	ctx				The build context to use during the operation.
    ///  @param[in]		v0				Triangle vertex 0 [(x, y, z)]
    ///  @param[in]		v1				Triangle vertex 1 [(x, y, z)]
    ///  @param[in]		v2				Triangle vertex 2 [(x, y, z)]
    ///  @param[in]		area			The area id of the triangle. [Limit: <= #RC_WALKABLE_AREA]
    ///  @param[in,out]	solid			An initialized heightfield.
    ///  @param[in]		flagMergeThr	The distance where the walkable flag is favored over the non-walkable flag.
    ///  								[Limit: >= 0] [Units: vx]
    //void rcRasterizeTriangle(rcContext* ctx, const float* v0, const float* v1, const float* v2,
    //						 const byte area, rcHeightfield& solid,
    //						 const int flagMergeThr = 1);

    /// Rasterizes an indexed triangle mesh into the specified heightfield.
    ///  @ingroup recast
    ///  @param[in,out]	ctx				The build context to use during the operation.
    ///  @param[in]		verts			The vertices. [(x, y, z) * @p nv]
    ///  @param[in]		nv				The number of vertices.
    ///  @param[in]		tris			The triangle indices. [(vertA, vertB, vertC) * @p nt]
    ///  @param[in]		areas			The area id's of the triangles. [Limit: <= #RC_WALKABLE_AREA] [Size: @p nt]
    ///  @param[in]		nt				The number of triangles.
    ///  @param[in,out]	solid			An initialized heightfield.
    ///  @param[in]		flagMergeThr	The distance where the walkable flag is favored over the non-walkable flag. 
    ///  								[Limit: >= 0] [Units: vx]
    //void rcRasterizeTriangles(rcContext* ctx, const float* verts, const int nv,
    //						  const int* tris, const byte* areas, const int nt,
    //						  rcHeightfield& solid, const int flagMergeThr = 1);

    /// Rasterizes an indexed triangle mesh into the specified heightfield.
    ///  @ingroup recast
    ///  @param[in,out]	ctx			The build context to use during the operation.
    ///  @param[in]		verts		The vertices. [(x, y, z) * @p nv]
    ///  @param[in]		nv			The number of vertices.
    ///  @param[in]		tris		The triangle indices. [(vertA, vertB, vertC) * @p nt]
    ///  @param[in]		areas		The area id's of the triangles. [Limit: <= #RC_WALKABLE_AREA] [Size: @p nt]
    ///  @param[in]		nt			The number of triangles.
    ///  @param[in,out]	solid		An initialized heightfield.
    ///  @param[in]		flagMergeThr	The distance where the walkable flag is favored over the non-walkable flag. 
    ///  							[Limit: >= 0] [Units: vx]
    //void rcRasterizeTriangles(rcContext* ctx, const float* verts, const int nv,
    //						  const ushort* tris, const byte* areas, const int nt,
    //						  rcHeightfield& solid, const int flagMergeThr = 1);

    /// Rasterizes triangles into the specified heightfield.
    ///  @ingroup recast
    ///  @param[in,out]	ctx				The build context to use during the operation.
    ///  @param[in]		verts			The triangle vertices. [(ax, ay, az, bx, by, bz, cx, by, cx) * @p nt]
    ///  @param[in]		areas			The area id's of the triangles. [Limit: <= #RC_WALKABLE_AREA] [Size: @p nt]
    ///  @param[in]		nt				The number of triangles.
    ///  @param[in,out]	solid			An initialized heightfield.
    ///  @param[in]		flagMergeThr	The distance where the walkable flag is favored over the non-walkable flag. 
    ///  								[Limit: >= 0] [Units: vx]
    //void rcRasterizeTriangles(rcContext* ctx, const float* verts, const byte* areas, const int nt,
    //						  rcHeightfield& solid, const int flagMergeThr = 1);

    /// Marks non-walkable spans as walkable if their maximum is within @p walkableClimp of a walkable neihbor. 
    ///  @ingroup recast
    ///  @param[in,out]	ctx				The build context to use during the operation.
    ///  @param[in]		walkableClimb	Maximum ledge height that is considered to still be traversable. 
    ///  								[Limit: >=0] [Units: vx]
    ///  @param[in,out]	solid			A fully built heightfield.  (All spans have been added.)
    //void rcFilterLowHangingWalkableObstacles(rcContext* ctx, const int walkableClimb, rcHeightfield& solid);

    /// Marks spans that are ledges as not-walkable. 
    ///  @ingroup recast
    ///  @param[in,out]	ctx				The build context to use during the operation.
    ///  @param[in]		walkableHeight	Minimum floor to 'ceiling' height that will still allow the floor area to 
    ///  								be considered walkable. [Limit: >= 3] [Units: vx]
    ///  @param[in]		walkableClimb	Maximum ledge height that is considered to still be traversable. 
    ///  								[Limit: >=0] [Units: vx]
    ///  @param[in,out]	solid			A fully built heightfield.  (All spans have been added.)
    //void rcFilterLedgeSpans(rcContext* ctx, const int walkableHeight,
    //						const int walkableClimb, rcHeightfield& solid);

    /// Marks walkable spans as not walkable if the clearence above the span is less than the specified height. 
    ///  @ingroup recast
    ///  @param[in,out]	ctx				The build context to use during the operation.
    ///  @param[in]		walkableHeight	Minimum floor to 'ceiling' height that will still allow the floor area to 
    ///  								be considered walkable. [Limit: >= 3] [Units: vx]
    ///  @param[in,out]	solid			A fully built heightfield.  (All spans have been added.)
    //void rcFilterWalkableLowHeightSpans(rcContext* ctx, int walkableHeight, rcHeightfield& solid);

    /// Returns the number of spans contained in the specified heightfield.
    ///  @ingroup recast
    ///  @param[in,out]	ctx		The build context to use during the operation.
    ///  @param[in]		hf		An initialized heightfield.
    ///  @returns The number of spans in the heightfield.
    //int rcGetHeightFieldSpanCount(rcContext* ctx, rcHeightfield& hf);

    /// @}
    /// @name Compact Heightfield Functions
    /// @see rcCompactHeightfield
    /// @{

    /// Builds a compact heightfield representing open space, from a heightfield representing solid space.
    ///  @ingroup recast
    ///  @param[in,out]	ctx				The build context to use during the operation.
    ///  @param[in]		walkableHeight	Minimum floor to 'ceiling' height that will still allow the floor area 
    ///  								to be considered walkable. [Limit: >= 3] [Units: vx]
    ///  @param[in]		walkableClimb	Maximum ledge height that is considered to still be traversable. 
    ///  								[Limit: >=0] [Units: vx]
    ///  @param[in]		hf				The heightfield to be compacted.
    ///  @param[out]	chf				The resulting compact heightfield. (Must be pre-allocated.)
    ///  @returns True if the operation completed successfully.
    //bool rcBuildCompactHeightfield(rcContext* ctx, const int walkableHeight, const int walkableClimb,
    //							   rcHeightfield& hf, rcCompactHeightfield& chf);
	 

    /// Erodes the walkable area within the heightfield by the specified radius. 
    ///  @ingroup recast
    ///  @param[in,out]	ctx		The build context to use during the operation.
	///  @param[in]		radius	The radius of erosion. [Limits: 0 < value < 255] [Units: vx]
    ///  @param[in,out]	chf		The populated compact heightfield to erode.
    ///  @returns True if the operation completed successfully.
	//bool rcErodeWalkableArea(rcContext* ctx, int radius, rcCompactHeightfield& chf);

    /// Applies a median filter to walkable area types (based on area id), removing noise.
    ///  @ingroup recast
    ///  @param[in,out]	ctx		The build context to use during the operation.
    ///  @param[in,out]	chf		A populated compact heightfield.
    ///  @returns True if the operation completed successfully.
    //bool rcMedianFilterWalkableArea(rcContext* ctx, rcCompactHeightfield& chf);

    /// Applies an area id to all spans within the specified bounding box. (AABB) 
    ///  @ingroup recast
    ///  @param[in,out]	ctx		The build context to use during the operation.
    ///  @param[in]		bmin	The minimum of the bounding box. [(x, y, z)]
    ///  @param[in]		bmax	The maximum of the bounding box. [(x, y, z)]
    ///  @param[in]		areaId	The area id to apply. [Limit: <= #RC_WALKABLE_AREA]
    ///  @param[in,out]	chf		A populated compact heightfield.
    //void rcMarkBoxArea(rcContext* ctx, const float* bmin, const float* bmax, byte areaId,
    //				   rcCompactHeightfield& chf);

    /// Applies the area id to the all spans within the specified convex polygon. 
    ///  @ingroup recast
    ///  @param[in,out]	ctx		The build context to use during the operation.
    ///  @param[in]		verts	The vertices of the polygon [Fomr: (x, y, z) * @p nverts]
    ///  @param[in]		nverts	The number of vertices in the polygon.
    ///  @param[in]		hmin	The height of the base of the polygon.
    ///  @param[in]		hmax	The height of the top of the polygon.
    ///  @param[in]		areaId	The area id to apply. [Limit: <= #RC_WALKABLE_AREA]
    ///  @param[in,out]	chf		A populated compact heightfield.
    //void rcMarkConvexPolyArea(rcContext* ctx, const float* verts, const int nverts,
    //						  const float hmin, const float hmax, byte areaId,
    //						  rcCompactHeightfield& chf);

    /// Helper function to offset voncex polygons for rcMarkConvexPolyArea.
    ///  @ingroup recast
    ///  @param[in]		verts		The vertices of the polygon [Form: (x, y, z) * @p nverts]
    ///  @param[in]		nverts		The number of vertices in the polygon.
    ///  @param[out]	outVerts	The offset vertices (should hold up to 2 * @p nverts) [Form: (x, y, z) * return value]
    ///  @param[in]		maxOutVerts	The max number of vertices that can be stored to @p outVerts.
    ///  @returns Number of vertices in the offset polygon or 0 if too few vertices in @p outVerts.
    //int rcOffsetPoly(const float* verts, const int nverts, const float offset,
    //				 float* outVerts, const int maxOutVerts);

    /// Applies the area id to all spans within the specified cylinder.
    ///  @ingroup recast
    ///  @param[in,out]	ctx		The build context to use during the operation.
    ///  @param[in]		pos		The center of the base of the cylinder. [Form: (x, y, z)] 
    ///  @param[in]		r		The radius of the cylinder.
    ///  @param[in]		h		The height of the cylinder.
    ///  @param[in]		areaId	The area id to apply. [Limit: <= #RC_WALKABLE_AREA]
    ///  @param[in,out]	chf	A populated compact heightfield.
    //void rcMarkCylinderArea(rcContext* ctx, const float* pos,
    //						const float r, const float h, byte areaId,
    //						rcCompactHeightfield& chf);

    /// Builds the distance field for the specified compact heightfield. 
    ///  @ingroup recast
    ///  @param[in,out]	ctx		The build context to use during the operation.
    ///  @param[in,out]	chf		A populated compact heightfield.
    ///  @returns True if the operation completed successfully.
    //bool rcBuildDistanceField(rcContext* ctx, rcCompactHeightfield& chf);

    /// Builds region data for the heightfield using watershed partitioning. 
    ///  @ingroup recast
    ///  @param[in,out]	ctx				The build context to use during the operation.
    ///  @param[in,out]	chf				A populated compact heightfield.
    ///  @param[in]		borderSize		The size of the non-navigable border around the heightfield.
    ///  								[Limit: >=0] [Units: vx]
    ///  @param[in]		minRegionArea	The minimum number of cells allowed to form isolated island areas.
    ///  								[Limit: >=0] [Units: vx].
    ///  @param[in]		mergeRegionArea		Any regions with a span count smaller than this value will, if possible,
    ///  								be merged with larger regions. [Limit: >=0] [Units: vx] 
    ///  @returns True if the operation completed successfully.
    //bool rcBuildRegions(rcContext* ctx, rcCompactHeightfield& chf,
    //					const int borderSize, const int minRegionArea, const int mergeRegionArea);

    /// Builds region data for the heightfield using simple monotone partitioning.
    ///  @ingroup recast 
    ///  @param[in,out]	ctx				The build context to use during the operation.
    ///  @param[in,out]	chf				A populated compact heightfield.
    ///  @param[in]		borderSize		The size of the non-navigable border around the heightfield.
    ///  								[Limit: >=0] [Units: vx]
    ///  @param[in]		minRegionArea	The minimum number of cells allowed to form isolated island areas.
    ///  								[Limit: >=0] [Units: vx].
    ///  @param[in]		mergeRegionArea	Any regions with a span count smaller than this value will, if possible, 
    ///  								be merged with larger regions. [Limit: >=0] [Units: vx] 
    ///  @returns True if the operation completed successfully.
    //bool rcBuildRegionsMonotone(rcContext* ctx, rcCompactHeightfield& chf,
    //							const int borderSize, const int minRegionArea, const int mergeRegionArea);


    /// Sets the neighbor connection data for the specified direction.
    ///  @param[in]		s		The span to update.
    ///  @param[in]		dir		The direction to set. [Limits: 0 <= value < 4]
    ///  @param[in]		i		The index of the neighbor span.
    public static void rcSetCon(ref rcCompactSpan s, int dir, int i) {
        uint udir = (uint)dir;
        int shift = (int)(udir * 6);
        uint con = s.con;
        s.con = (uint)(con & ~(0x3f << shift)) | (((uint)i & 0x3f) << shift);
    }

    /// Gets neighbor connection data for the specified direction.
    ///  @param[in]		s		The span to check.
    ///  @param[in]		dir		The direction to check. [Limits: 0 <= value < 4]
    ///  @return The neighbor connection data for the specified direction,
    ///  	or #RC_NOT_CONNECTED if there is no connection.
    public static int rcGetCon(rcCompactSpan s, int dir) {
        uint udir = (uint)dir;
        int shift = (int)(udir * 6);
        return (int)((s.con >> shift) & 0x3f);
    }

    /// Gets the standard width (x-axis) offset for the specified direction.
    ///  @param[in]		dir		The direction. [Limits: 0 <= value < 4]
    ///  @return The width offset to apply to the current cell position to move
    ///  	in the direction.
    public static int rcGetDirOffsetX(int dir) {
        int[] offset = new int[] { -1, 0, 1, 0, };
        return offset[dir & 0x03];
    }

    /// Gets the standard height (z-axis) offset for the specified direction.
    ///  @param[in]		dir		The direction. [Limits: 0 <= value < 4]
    ///  @return The height offset to apply to the current cell position to move
    ///  	in the direction.
    public static int rcGetDirOffsetY(int dir) {
        int[] offset = new int[] { 0, 1, 0, -1 };
        return offset[dir & 0x03];
    }

    /// @}
    /// @name Layer, Contour, Polymesh, and Detail Mesh Functions
    /// @see rcHeightfieldLayer, rcContourSet, rcPolyMesh, rcPolyMeshDetail
    /// @{

    /// Builds a layer set from the specified compact heightfield.
    ///  @ingroup recast
    ///  @param[in,out]	ctx			The build context to use during the operation.
    ///  @param[in]		chf			A fully built compact heightfield.
    ///  @param[in]		borderSize	The size of the non-navigable border around the heightfield. [Limit: >=0] 
    ///  							[Units: vx]
    ///  @param[in]		walkableHeight	Minimum floor to 'ceiling' height that will still allow the floor area 
    ///  							to be considered walkable. [Limit: >= 3] [Units: vx]
    ///  @param[out]	lset		The resulting layer set. (Must be pre-allocated.)
    ///  @returns True if the operation completed successfully.
    //bool rcBuildHeightfieldLayers(rcContext* ctx, rcCompactHeightfield& chf, 
    //							  const int borderSize, const int walkableHeight,
    //							  rcHeightfieldLayerSet& lset);

    /// Builds a contour set from the region outlines in the provided compact heightfield.
    ///  @ingroup recast
    ///  @param[in,out]	ctx			The build context to use during the operation.
    ///  @param[in]		chf			A fully built compact heightfield.
    ///  @param[in]		maxError	The maximum distance a simplfied contour's border edges should deviate 
    ///  							the original raw contour. [Limit: >=0] [Units: wu]
    ///  @param[in]		maxEdgeLen	The maximum allowed length for contour edges along the border of the mesh. 
    ///  							[Limit: >=0] [Units: vx]
    ///  @param[out]	cset		The resulting contour set. (Must be pre-allocated.)
    ///  @param[in]		buildFlags	The build flags. (See: #rcBuildContoursFlags)
    ///  @returns True if the operation completed successfully.
    //bool rcBuildContours(rcContext* ctx, rcCompactHeightfield& chf,
    //					 const float maxError, const int maxEdgeLen,
    //					 rcContourSet& cset, const int flags = RC_CONTOUR_TESS_WALL_EDGES);

    /// Builds a polygon mesh from the provided contours.
    ///  @ingroup recast
    ///  @param[in,out]	ctx		The build context to use during the operation.
    ///  @param[in]		cset	A fully built contour set.
    ///  @param[in]		nvp		The maximum number of vertices allowed for polygons generated during the 
    ///  						contour to polygon conversion process. [Limit: >= 3] 
    ///  @param[out]	mesh	The resulting polygon mesh. (Must be re-allocated.)
    ///  @returns True if the operation completed successfully.
    //bool rcBuildPolyMesh(rcContext* ctx, rcContourSet& cset, const int nvp, rcPolyMesh& mesh);

    /// Merges multiple polygon meshes into a single mesh.
    ///  @ingroup recast
    ///  @param[in,out]	ctx		The build context to use during the operation.
    ///  @param[in]		meshes	An array of polygon meshes to merge. [Size: @p nmeshes]
    ///  @param[in]		nmeshes	The number of polygon meshes in the meshes array.
    ///  @param[in]		mesh	The resulting polygon mesh. (Must be pre-allocated.)
    ///  @returns True if the operation completed successfully.
    //bool rcMergePolyMeshes(rcContext* ctx, rcPolyMesh** meshes, const int nmeshes, rcPolyMesh& mesh);

    /// Builds a detail mesh from the provided polygon mesh.
    ///  @ingroup recast
    ///  @param[in,out]	ctx				The build context to use during the operation.
    ///  @param[in]		mesh			A fully built polygon mesh.
    ///  @param[in]		chf				The compact heightfield used to build the polygon mesh.
    ///  @param[in]		sampleDist		Sets the distance to use when samping the heightfield. [Limit: >=0] [Units: wu]
    ///  @param[in]		sampleMaxError	The maximum distance the detail mesh surface should deviate from 
    ///  								heightfield data. [Limit: >=0] [Units: wu]
    ///  @param[out]	dmesh			The resulting detail mesh.  (Must be pre-allocated.)
    ///  @returns True if the operation completed successfully.
    //bool rcBuildPolyMeshDetail(rcContext* ctx, const rcPolyMesh& mesh, const rcCompactHeightfield& chf,
    //						   const float sampleDist, const float sampleMaxError,
    //						   rcPolyMeshDetail& dmesh);

    /// Copies the poly mesh data from src to dst.
    ///  @ingroup recast
    ///  @param[in,out]	ctx		The build context to use during the operation.
    ///  @param[in]		src		The source mesh to copy from.
    ///  @param[out]	dst		The resulting detail mesh. (Must be pre-allocated, must be empty mesh.)
    ///  @returns True if the operation completed successfully.
    //bool rcCopyPolyMesh(rcContext* ctx, const rcPolyMesh& src, rcPolyMesh& dst);

    /// Merges multiple detail meshes into a single detail mesh.
    ///  @ingroup recast
    ///  @param[in,out]	ctx		The build context to use during the operation.
    ///  @param[in]		meshes	An array of detail meshes to merge. [Size: @p nmeshes]
    ///  @param[in]		nmeshes	The number of detail meshes in the meshes array.
    ///  @param[out]	mesh	The resulting detail mesh. (Must be pre-allocated.)
    ///  @returns True if the operation completed successfully.
    //bool rcMergePolyMeshDetails(rcContext* ctx, rcPolyMeshDetail** meshes, const int nmeshes, rcPolyMeshDetail& mesh);

    /// @}

}

///////////////////////////////////////////////////////////////////////////

// Due to the large amount of detail documentation for this file, 
// the content normally located at the end of the header file has been separated
// out to a file in /Docs/Extern.
