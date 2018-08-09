using System;
using System.Diagnostics;

public static partial class Recast{
    static bool overlapBounds(float[] amin, float[] amax, float[] bmin, float[] bmax)
    {
	    bool overlap = true;
	    overlap = (amin[0] > bmax[0] || amax[0] < bmin[0]) ? false : overlap;
	    overlap = (amin[1] > bmax[1] || amax[1] < bmin[1]) ? false : overlap;
	    overlap = (amin[2] > bmax[2] || amax[2] < bmin[2]) ? false : overlap;
	    return overlap;
    }

    static bool overlapInterval(ushort amin, ushort amax,
							    ushort bmin, ushort bmax)
    {
	    if (amax < bmin) return false;
	    if (amin > bmax) return false;
	    return true;
    }


    static rcSpan allocSpan(rcHeightfield hf)
    {
	    // If running out of memory, allocate new page and update the freelist.
	    if (hf.freelist == null || hf.freelist.next == null)
	    {
		    // Create new page.
		    // Allocate memory for the new pool.
		    //rcSpanPool* pool = (rcSpanPool*)rcAlloc(sizeof(rcSpanPool), RC_ALLOC_PERM);
            rcSpanPool pool = new rcSpanPool();
		    if (pool == null) 
                return null;
		    pool.next = null;
		    // Add the pool into the list of pools.
		    pool.next = hf.pools;
		    hf.pools = pool;
		    // Add new items to the free list.
		    rcSpan freelist = hf.freelist;
		    //rcSpan head = pool.items[0];
		    //rcSpan it = pool.items[RC_SPANS_PER_POOL];
            int itIndex = RC_SPANS_PER_POOL;
		    do
		    {
			    --itIndex;
			    pool.items[itIndex].next = freelist;
			    freelist = pool.items[itIndex];
		    }
		    while (itIndex != 0);
		    hf.freelist = pool.items[itIndex];
	    }
	
	    // Pop item from in front of the free list.
	    rcSpan it = hf.freelist;
	    hf.freelist = hf.freelist.next;
	    return it;
    }

    static void freeSpan(rcHeightfield hf, rcSpan ptr)
    {
	    if (ptr == null) {
            return;
        }
	    // Add the node in front of the free list.
	    ptr.next = hf.freelist;
	    hf.freelist = ptr;
    }

    static void addSpan(rcHeightfield hf, int x, int y,
					    ushort smin, ushort smax,
					    byte area, int flagMergeThr)
    {
	
	    int idx = x + y*hf.width;
	
	    rcSpan s = allocSpan(hf);
	    s.smin = smin;
	    s.smax = smax;
	    s.area = area;
	    s.next = null;
	
	    // Empty cell, add the first span.
	    if (hf.spans[idx] == null)
	    {
		    hf.spans[idx] = s;
		    return;
	    }
	    rcSpan prev = null;
	    rcSpan cur = hf.spans[idx];
	
	    // Insert and merge spans.
	    while (cur != null)
	    {
		    if (cur.smin > s.smax)
		    {
			    // Current span is further than the new span, break.
			    break;
		    }
		    else if (cur.smax < s.smin)
		    {
			    // Current span is before the new span advance.
			    prev = cur;
			    cur = cur.next;
		    }
		    else
		    {
			    // Merge spans.
			    if (cur.smin < s.smin)
				    s.smin = cur.smin;
			    if (cur.smax > s.smax)
				    s.smax = cur.smax;
			
			    // Merge flags.
			    if (Math.Abs((int)s.smax - (int)cur.smax) <= flagMergeThr){
				    s.area = Math.Max(s.area, cur.area);
                }
			
			    // Remove current span.
			    rcSpan next = cur.next;
			    freeSpan(hf, cur);
			    if (prev != null)
				    prev.next = next;
			    else
				    hf.spans[idx] = next;
			    cur = next;
		    }
	    }
	
	    // Insert new span.
	    if (prev != null)
	    {
		    s.next = prev.next;
		    prev.next = s;
	    }
	    else
	    {
		    s.next = hf.spans[idx];
		    hf.spans[idx] = s;
	    }
    }

    /// @par
    ///
    /// The span addition can be set to favor flags. If the span is merged to
    /// another span and the new @p smax is within @p flagMergeThr units
    /// from the existing span, the span flags are merged.
    ///
    /// @see rcHeightfield, rcSpan.
    static void rcAddSpan(rcContext ctx, rcHeightfield hf, int x, int y,
			       ushort smin, ushort smax,
			       byte area, int flagMergeThr)
    {
    //	Debug.Assert(ctx != null, "rcContext is null");
	    addSpan(hf, x,y, smin, smax, area, flagMergeThr);
    }

    // divides a convex polygons into two convex polygons on both sides of a line
    static void dividePoly(float[] _in, int nin,
					      float[] out1, ref int nout1,
					      float[] out2, ref int nout2,
					      float x, int axis)
    {
	    float[] d = new float[12];
	    for (int i = 0; i < nin; ++i){
		    d[i] = x - _in[i*3+axis];
        }

	    int m = 0, n = 0;
	    for (int i = 0, j = nin-1; i < nin; j=i, ++i)
	    {
		    bool ina = d[j] >= 0;
		    bool inb = d[i] >= 0;
		    if (ina != inb)
		    {
			    float s = d[j] / (d[j] - d[i]);
			    out1[m*3+0] = _in[j*3+0] + (_in[i*3+0] - _in[j*3+0])*s;
			    out1[m*3+1] = _in[j*3+1] + (_in[i*3+1] - _in[j*3+1])*s;
			    out1[m*3+2] = _in[j*3+2] + (_in[i*3+2] - _in[j*3+2])*s;
			    rcVcopy(out2, n*3, out1, m*3);
			    m++;
			    n++;
			    // add the i'th point to the right polygon. Do NOT add points that are on the dividing line
			    // since these were already added above
			    if (d[i] > 0)
			    {
				    rcVcopy(out1,m*3, _in, i*3);
				    m++;
			    }
			    else if (d[i] < 0)
			    {
				    rcVcopy(out2,n*3, _in, i*3);
				    n++;
			    }
		    }
		    else // same side
		    {
			    // add the i'th point to the right polygon. Addition is done even for points on the dividing line
			    if (d[i] >= 0)
			    {
				    rcVcopy(out1, m*3, _in, i*3);
				    m++;
				    if (d[i] != 0)
					    continue;
			    }
			    rcVcopy(out2, n*3, _in, i*3);
			    n++;
		    }
	    }

	    nout1 = m;
	    nout2 = n;
    }



    static void rasterizeTri(float[] v0, int v0Start, float[] v1, int v1Start, float[] v2, int v2Start,
						     byte area, rcHeightfield hf,
						     float[] bmin, float[] bmax,
						     float cs, float ics, float ich,
						     int flagMergeThr)
    {
	    int w = hf.width;
	    int h = hf.height;
	    float[] tmin = new float[3];
        float[] tmax = new float[3];
	    float by = bmax[1] - bmin[1];
	
	    // Calculate the bounding box of the triangle.
	    rcVcopy(tmin, 0, v0, v0Start);
        rcVcopy(tmax, 0, v0, v0Start);
	    rcVmin(tmin, 0, v1, v1Start);
	    rcVmin(tmin, 0, v2, v2Start);
	    rcVmax(tmax, 0, v1, v1Start);
	    rcVmax(tmax, 0, v2, v2Start);
	
	    // If the triangle does not touch the bbox of the heightfield, skip the triagle.
	    if (!overlapBounds(bmin, bmax, tmin, tmax))
		    return;
	
	    // Calculate the footprint of the triangle on the grid's y-axis
	    int y0 = (int)((tmin[2] - bmin[2])*ics);
	    int y1 = (int)((tmax[2] - bmin[2])*ics);
	    y0 = rcClamp(y0, 0, h-1);
	    y1 = rcClamp(y1, 0, h-1);
	
	    // Clip the triangle into all grid cells it touches.
	    //float[] buf = new float[7*3*4];

	    float[] _in = new float[7*3];
        float[] inrow = new float[7*3];
        float[] p1 = new float[7*3];
        float[] p2 = new float[7*3];

	    rcVcopy(_in,0  , v0, v0Start);
	    rcVcopy(_in,1*3, v1, v1Start);
	    rcVcopy(_in,2*3, v2, v2Start);

	    int nvrow = 0;
        int nvIn = 3;
	
	    for (int y = y0; y <= y1; ++y)
	    {
		    // Clip polygon to row. Store the remaining polygon as well
		    float cz = bmin[2] + y*cs;
		    dividePoly(_in, nvIn, inrow, ref nvrow, p1, ref nvIn, cz+cs, 2);
		    //rcSwap(_in, p1);
            float[] tmp = _in;
            _in = p1;
            p1 = tmp;

		    if (nvrow < 3) 
                continue;
		
		    // find the horizontal bounds in the row
		    float minX = inrow[0], maxX = inrow[0];
		    for (int i=1; i<nvrow; ++i)
		    {
			    if (minX > inrow[i*3])	minX = inrow[i*3];
			    if (maxX < inrow[i*3])	maxX = inrow[i*3];
		    }
		    int x0 = (int)((minX - bmin[0])*ics);
		    int x1 = (int)((maxX - bmin[0])*ics);
		    x0 = rcClamp(x0, 0, w-1);
		    x1 = rcClamp(x1, 0, w-1);

		    int nv = 0;
            int nv2 = nvrow;

		    for (int x = x0; x <= x1; ++x)
		    {
			    // Clip polygon to column. store the remaining polygon as well
			    float cx = bmin[0] + x*cs;
			    dividePoly(inrow, nv2, p1, ref nv, p2, ref nv2, cx+cs, 0);
			    //rcSwap(inrow, p2);
                tmp = inrow;
                inrow = p2;
                p2 = tmp;
			    if (nv < 3) {
                    continue;
                }
			
			    // Calculate min and max of the span.
			    float smin = p1[1], smax = p1[1];
			    for (int i = 1; i < nv; ++i)
			    {
				    smin = Math.Min(smin, p1[i*3+1]);
				    smax = Math.Max(smax, p1[i*3+1]);
			    }
			    smin -= bmin[1];
			    smax -= bmin[1];
			    // Skip the span if it is outside the heightfield bbox
			    if (smax < 0.0f) continue;
			    if (smin > by) continue;
			    // Clamp the span to the heightfield bbox.
			    if (smin < 0.0f) smin = 0;
			    if (smax > by) smax = by;
			
			    // Snap the span to the heightfield height grid.
			    ushort ismin = (ushort)rcClamp((int)Math.Floor(smin * ich), 0, RC_SPAN_MAX_HEIGHT);
			    ushort ismax = (ushort)rcClamp((int)Math.Ceiling(smax * ich), (int)ismin+1, RC_SPAN_MAX_HEIGHT);
			
			    addSpan(hf, x, y, ismin, ismax, area, flagMergeThr);
		    }
	    }
    }

    /// @par
    ///
    /// No spans will be added if the triangle does not overlap the heightfield grid.
    ///
    /// @see rcHeightfield
    public static void rcRasterizeTriangle(rcContext ctx, float[] v0, int v0Start, float[] v1, int v1Start, float[] v2, int v2Start,
						     byte area, rcHeightfield solid,
						     int flagMergeThr)
    {
	    Debug.Assert(ctx != null, "rcContext is null");

	    ctx.startTimer(rcTimerLabel.RC_TIMER_RASTERIZE_TRIANGLES);

	    float ics = 1.0f/solid.cs;
	    float ich = 1.0f/solid.ch;
	    rasterizeTri(v0, v0Start, v1, v1Start, v2, v2Start, area, solid, solid.bmin, solid.bmax, solid.cs, ics, ich, flagMergeThr);

	    ctx.stopTimer(rcTimerLabel.RC_TIMER_RASTERIZE_TRIANGLES);
    }

    /// @par
    ///
    /// Spans will only be added for triangles that overlap the heightfield grid.
    ///
    /// @see rcHeightfield
    public static void rcRasterizeTriangles(rcContext ctx, float[] verts, int nv,
						      int[] tris, byte[] areas, int nt,
						      rcHeightfield solid, int flagMergeThr)
    {
	    Debug.Assert(ctx != null, "rcContext is null");

	    ctx.startTimer(rcTimerLabel.RC_TIMER_RASTERIZE_TRIANGLES);
	
	    float ics = 1.0f/solid.cs;
	    float ich = 1.0f/solid.ch;
	    // Rasterize triangles.
	    for (int i = 0; i < nt; ++i)
	    {
            int v0Start = tris[i*3+0]*3;
            int v1Start = tris[i*3+1]*3;
            int v2Start = tris[i*3+2]*3;
		    // Rasterize.
            rasterizeTri(verts, v0Start, verts, v1Start, verts, v2Start, areas[i], solid, solid.bmin, solid.bmax, solid.cs, ics, ich, flagMergeThr);
	    }
	
	    ctx.stopTimer(rcTimerLabel.RC_TIMER_RASTERIZE_TRIANGLES);
    }

    /// @par
    ///
    /// Spans will only be added for triangles that overlap the heightfield grid.
    ///
    /// @see rcHeightfield
    public static void rcRasterizeTriangles(rcContext ctx, float[] verts, int nv,
						      ushort[] tris, byte[] areas, int nt,
						      rcHeightfield solid, int flagMergeThr)
    {
	    Debug.Assert(ctx != null, "rcContext is null");

	    ctx.startTimer(rcTimerLabel.RC_TIMER_RASTERIZE_TRIANGLES);
	
	    float ics = 1.0f/solid.cs;
	    float ich = 1.0f/solid.ch;
	    // Rasterize triangles.
	    for (int i = 0; i < nt; ++i)
	    {
		    int v0Start = tris[i*3+0]*3;
            int v1Start = tris[i*3+1]*3;
            int v2Start = tris[i*3+2]*3;

		    // Rasterize.
		    rasterizeTri(verts, v0Start, verts, v1Start, verts, v2Start, areas[i], solid, solid.bmin, solid.bmax, solid.cs, ics, ich, flagMergeThr);
	    }
	
	    ctx.stopTimer(rcTimerLabel.RC_TIMER_RASTERIZE_TRIANGLES);
    }

    /// @par
    ///
    /// Spans will only be added for triangles that overlap the heightfield grid.
    ///
    /// @see rcHeightfield
    public static void rcRasterizeTriangles(rcContext ctx, float[] verts, byte[] areas, int nt,
						      rcHeightfield solid, int flagMergeThr)
    {
	    Debug.Assert(ctx != null, "rcContext is null");
	
	    ctx.startTimer(rcTimerLabel.RC_TIMER_RASTERIZE_TRIANGLES);
	
	    float ics = 1.0f/solid.cs;
	    float ich = 1.0f/solid.ch;
	    // Rasterize triangles.
	    for (int i = 0; i < nt; ++i)
	    {
		    int v0Start = (i*3+0)*3;
		    int v1Start = (i*3+1)*3;
		    int v2Start = (i*3+2)*3;
		    // Rasterize.
		    rasterizeTri(verts, v0Start, verts, v1Start, verts, v2Start, areas[i], solid, solid.bmin, solid.bmax, solid.cs, ics, ich, flagMergeThr);
	    }
	
	    ctx.stopTimer(rcTimerLabel.RC_TIMER_RASTERIZE_TRIANGLES);
    }
}