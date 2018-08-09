using System;
using System.Collections.Generic;
using System.Diagnostics;

public static partial class Recast{
static int getCornerHeight(int x, int y, int i, int dir,
						   rcCompactHeightfield chf,
						   ref bool isBorderVertex)
{
	rcCompactSpan s = chf.spans[i];
	int ch = (int)s.y;
	int dirp = (dir+1) & 0x3;
	
	uint[] regs = new uint[] {0,0,0,0};
	
	// Combine region and area codes in order to prevent
	// border vertices which are in between two areas to be removed. 
	regs[0] = (uint)( chf.spans[i].reg | (chf.areas[i] << 16) );
	
	if (rcGetCon(s, dir) != RC_NOT_CONNECTED)
	{
		int ax = x + rcGetDirOffsetX(dir);
		int ay = y + rcGetDirOffsetY(dir);
		int ai = (int)chf.cells[ax+ay*chf.width].index + rcGetCon(s, dir);
		rcCompactSpan aSpan = chf.spans[ai];
		ch = Math.Max(ch, (int)aSpan.y);
		regs[1] = (uint)( chf.spans[ai].reg | (chf.areas[ai] << 16) );
		if (rcGetCon(aSpan, dirp) != RC_NOT_CONNECTED)
		{
			int ax2 = ax + rcGetDirOffsetX(dirp);
			int ay2 = ay + rcGetDirOffsetY(dirp);
			int ai2 = (int)chf.cells[ax2+ay2*chf.width].index + rcGetCon(aSpan, dirp);
			rcCompactSpan as2 = chf.spans[ai2];
			ch = Math.Max(ch, (int)as2.y);
			regs[2] =  (uint)(chf.spans[ai2].reg | (chf.areas[ai2] << 16));
		}
	}
	if (rcGetCon(s, dirp) != RC_NOT_CONNECTED)
	{
		int ax = x + rcGetDirOffsetX(dirp);
		int ay = y + rcGetDirOffsetY(dirp);
		int ai = (int)chf.cells[ax+ay*chf.width].index + rcGetCon(s, dirp);
		rcCompactSpan aSpan = chf.spans[ai];
		ch = Math.Max(ch, (int)aSpan.y);
		regs[3] = (uint)(chf.spans[ai].reg | (chf.areas[ai] << 16));
		if (rcGetCon(aSpan, dir) != RC_NOT_CONNECTED)
		{
			int ax2 = ax + rcGetDirOffsetX(dir);
			int ay2 = ay + rcGetDirOffsetY(dir);
			int ai2 = (int)chf.cells[ax2+ay2*chf.width].index + rcGetCon(aSpan, dir);
			rcCompactSpan as2 = chf.spans[ai2];
			ch = Math.Max(ch, (int)as2.y);
			regs[2] = (uint)(chf.spans[ai2].reg | (chf.areas[ai2] << 16));
		}
	}

	// Check if the vertex is special edge vertex, these vertices will be removed later.
	for (int j = 0; j < 4; ++j)
	{
		int a = j;
		int b = (j+1) & 0x3;
		int c = (j+2) & 0x3;
		int d = (j+3) & 0x3;
		
		// The vertex is a border vertex there are two same exterior cells in a row,
		// followed by two interior cells and none of the regions are out of bounds.
		bool twoSameExts = (regs[a] & regs[b] & RC_BORDER_REG) != 0 && regs[a] == regs[b];
		bool twoInts = ((regs[c] | regs[d]) & RC_BORDER_REG) == 0;
		bool intsSameArea = (regs[c]>>16) == (regs[d]>>16);
		bool noZeros = regs[a] != 0 && regs[b] != 0 && regs[c] != 0 && regs[d] != 0;
		if (twoSameExts && twoInts && intsSameArea && noZeros)
		{
			isBorderVertex = true;
			break;
		}
	}
	
	return ch;
}

public static void walkContour(int x, int y, int i,
						rcCompactHeightfield chf,
						byte[] flags, List<int> points)
{
	// Choose the first non-connected edge
	byte dir = 0;
	while ((flags[i] & (1 << dir)) == 0)
		dir++;
	
	byte startDir = dir;
	int starti = i;
	
	byte area = chf.areas[i];
	
	int iter = 0;
	while (++iter < 40000)
	{
		if ((flags[i] & (1 << dir)) != 0)
		{
			// Choose the edge corner
			bool isBorderVertex = false;
			bool isAreaBorder = false;
			int px = x;
			int py = getCornerHeight(x, y, i, dir, chf,ref isBorderVertex);
			int pz = y;
			switch(dir)
			{
				case 0: pz++; break;
				case 1: px++; pz++; break;
				case 2: px++; break;
			}
			int r = 0;
			rcCompactSpan s = chf.spans[i];
			if (rcGetCon(s, dir) != RC_NOT_CONNECTED)
			{
				int ax = x + rcGetDirOffsetX(dir);
				int ay = y + rcGetDirOffsetY(dir);
				int ai = (int)chf.cells[ax+ay*chf.width].index + rcGetCon(s, dir);
				r = (int)chf.spans[ai].reg;
				if (area != chf.areas[ai])
					isAreaBorder = true;
			}
			if (isBorderVertex)
				r |= RC_BORDER_VERTEX;
			if (isAreaBorder)
				r |= RC_AREA_BORDER;
			points.Add(px);
			points.Add(py);
			points.Add(pz);
			points.Add(r);
			
			flags[i] &= (byte)( ~(1 << dir) ); // Remove visited edges
			dir = (byte)( (dir+1) & 0x3);  // Rotate CW
		}
		else
		{
			int ni = -1;
			int nx = x + rcGetDirOffsetX(dir);
			int ny = y + rcGetDirOffsetY(dir);
			rcCompactSpan s = chf.spans[i];
			if (rcGetCon(s, dir) != RC_NOT_CONNECTED)
			{
				rcCompactCell nc = chf.cells[nx+ny*chf.width];
				ni = (int)nc.index + rcGetCon(s, dir);
			}
			if (ni == -1)
			{
				// Should not happen.
				return;
			}
			x = nx;
			y = ny;
			i = ni;
			dir = (byte)((dir+3) & 0x3);	// Rotate CCW
		}
		
		if (starti == i && startDir == dir)
		{
			break;
		}
	}
}

public static float distancePtSeg(int x, int z,
						   int px, int pz,
						   int qx, int qz)
{
/*	float pqx = (float)(qx - px);
	float pqy = (float)(qy - py);
	float pqz = (float)(qz - pz);
	float dx = (float)(x - px);
	float dy = (float)(y - py);
	float dz = (float)(z - pz);
	float d = pqx*pqx + pqy*pqy + pqz*pqz;
	float t = pqx*dx + pqy*dy + pqz*dz;
	if (d > 0)
		t /= d;
	if (t < 0)
		t = 0;
	else if (t > 1)
		t = 1;
	
	dx = px + t*pqx - x;
	dy = py + t*pqy - y;
	dz = pz + t*pqz - z;
	
	return dx*dx + dy*dy + dz*dz;*/

	float pqx = (float)(qx - px);
	float pqz = (float)(qz - pz);
	float dx = (float)(x - px);
	float dz = (float)(z - pz);
	float d = pqx*pqx + pqz*pqz;
	float t = pqx*dx + pqz*dz;
	if (d > 0)
		t /= d;
	if (t < 0)
		t = 0;
	else if (t > 1)
		t = 1;
	
	dx = px + t*pqx - x;
	dz = pz + t*pqz - z;
	
	return dx*dx + dz*dz;
}

public static void simplifyContour(List<int> points, List<int> simplified,
							float maxError, int maxEdgeLen, int buildFlags)
{
	// Add initial points.
	bool hasConnections = false;
	for (int i = 0; i < points.Count; i += 4)
	{
		if ((points[i+3] & RC_CONTOUR_REG_MASK) != 0)
		{
			hasConnections = true;
			break;
		}
	}
	
	if (hasConnections)
	{
		// The contour has some portals to other regions.
		// Add a new point to every location where the region changes.
		for (int i = 0, ni = points.Count /4; i < ni; ++i)
		{
			int ii = (i+1) % ni;
			bool differentRegs = (points[i*4+3] & RC_CONTOUR_REG_MASK) != (points[ii*4+3] & RC_CONTOUR_REG_MASK);
			bool areaBorders = (points[i*4+3] & RC_AREA_BORDER) != (points[ii*4+3] & RC_AREA_BORDER);
			if (differentRegs || areaBorders)
			{
				simplified.Add(points[i*4+0]);
				simplified.Add(points[i*4+1]);
				simplified.Add(points[i*4+2]);
				simplified.Add(i);
			}
		}       
	}
	
	if (simplified.Count == 0)
	{
		// If there is no connections at all,
		// create some initial points for the simplification process. 
		// Find lower-left and upper-right vertices of the contour.
		int llx = points[0];
		int lly = points[1];
		int llz = points[2];
		int lli = 0;
		int urx = points[0];
		int ury = points[1];
		int urz = points[2];
		int uri = 0;
		for (int i = 0; i < points.Count; i += 4)
		{
			int x = points[i+0];
			int y = points[i+1];
			int z = points[i+2];
			if (x < llx || (x == llx && z < llz))
			{
				llx = x;
				lly = y;
				llz = z;
				lli = i/4;
			}
			if (x > urx || (x == urx && z > urz))
			{
				urx = x;
				ury = y;
				urz = z;
				uri = i/4;
			}
		}
		simplified.Add(llx);
		simplified.Add(lly);
		simplified.Add(llz);
		simplified.Add(lli);
		
		simplified.Add(urx);
		simplified.Add(ury);
		simplified.Add(urz);
		simplified.Add(uri);
	}
	
	// Add points until all raw points are within
	// error tolerance to the simplified shape.
	int pn = points.Count/4;
	for (int i = 0; i < simplified.Count/4; )
	{
		int ii = (i+1) % (simplified.Count/4);
		
		int ax = simplified[i*4+0];
		int az = simplified[i*4+2];
		int ai = simplified[i*4+3];
		
		int bx = simplified[ii*4+0];
		int bz = simplified[ii*4+2];
		int bi = simplified[ii*4+3];

		// Find maximum deviation from the segment.
		float maxd = 0;
		int maxi = -1;
		int ci, cinc, endi;
		
		// Traverse the segment in lexilogical order so that the
		// max deviation is calculated similarly when traversing
		// opposite segments.
		if (bx > ax || (bx == ax && bz > az))
		{
			cinc = 1;
			ci = (ai+cinc) % pn;
			endi = bi;
		}
		else
		{
			cinc = pn-1;
			ci = (bi+cinc) % pn;
			endi = ai;
		}
		
		// Tessellate only outer edges or edges between areas.
		if ((points[ci*4+3] & RC_CONTOUR_REG_MASK) == 0 ||
			(points[ci*4+3] & RC_AREA_BORDER) != 0)
		{
			while (ci != endi)
			{
				float d = distancePtSeg(points[ci*4+0], points[ci*4+2], ax, az, bx, bz);
				if (d > maxd)
				{
					maxd = d;
					maxi = ci;
				}
				ci = (ci+cinc) % pn;
			}
		}
		
		
		// If the max deviation is larger than accepted error,
		// add new point, else continue to next segment.
		if (maxi != -1 && maxd > (maxError*maxError))
		{
			// Add space for the new point.
			//simplified.resize(simplified.Count+4);
            rccsResizeList(simplified, simplified.Count + 4);
			int n = simplified.Count/4;
			for (int j = n-1; j > i; --j)
			{
				simplified[j*4+0] = simplified[(j-1)*4+0];
				simplified[j*4+1] = simplified[(j-1)*4+1];
				simplified[j*4+2] = simplified[(j-1)*4+2];
				simplified[j*4+3] = simplified[(j-1)*4+3];
			}
			// Add the point.
			simplified[(i+1)*4+0] = points[maxi*4+0];
			simplified[(i+1)*4+1] = points[maxi*4+1];
			simplified[(i+1)*4+2] = points[maxi*4+2];
			simplified[(i+1)*4+3] = maxi;
		}
		else
		{
			++i;
		}
	}
	
	// Split too long edges.
	if (maxEdgeLen > 0 && (buildFlags & (int)(rcBuildContoursFlags.RC_CONTOUR_TESS_WALL_EDGES|rcBuildContoursFlags.RC_CONTOUR_TESS_AREA_EDGES)) != 0)
	{
		for (int i = 0; i < simplified.Count/4; )
		{
			int ii = (i+1) % (simplified.Count/4);
			
			int ax = simplified[i*4+0];
			int az = simplified[i*4+2];
			int ai = simplified[i*4+3];
			
			int bx = simplified[ii*4+0];
			int bz = simplified[ii*4+2];
			int bi = simplified[ii*4+3];

			// Find maximum deviation from the segment.
			int maxi = -1;
			int ci = (ai+1) % pn;

			// Tessellate only outer edges or edges between areas.
			bool tess = false;
			// Wall edges.
			if ((buildFlags & (int)rcBuildContoursFlags.RC_CONTOUR_TESS_WALL_EDGES) != 0 && (points[ci*4+3] & RC_CONTOUR_REG_MASK) == 0)
				tess = true;
			// Edges between areas.
			if ((buildFlags & (int)rcBuildContoursFlags.RC_CONTOUR_TESS_AREA_EDGES) != 0 && (points[ci*4+3] & RC_AREA_BORDER) != 0)
				tess = true;
			
			if (tess)
			{
				int dx = bx - ax;
				int dz = bz - az;
				if (dx*dx + dz*dz > maxEdgeLen*maxEdgeLen)
				{
					// Round based on the segments in lexilogical order so that the
					// max tesselation is consistent regardles in which direction
					// segments are traversed.
					int n = bi < ai ? (bi+pn - ai) : (bi - ai);
					if (n > 1)
					{
						if (bx > ax || (bx == ax && bz > az))
							maxi = (ai + n/2) % pn;
						else
							maxi = (ai + (n+1)/2) % pn;
					}
				}
			}
			
			// If the max deviation is larger than accepted error,
			// add new point, else continue to next segment.
			if (maxi != -1)
			{
				// Add space for the new point.
                rccsResizeList(simplified, simplified.Count + 4);
				int n = simplified.Count/4;
				for (int j = n-1; j > i; --j)
				{
					simplified[j*4+0] = simplified[(j-1)*4+0];
					simplified[j*4+1] = simplified[(j-1)*4+1];
					simplified[j*4+2] = simplified[(j-1)*4+2];
					simplified[j*4+3] = simplified[(j-1)*4+3];
				}
				// Add the point.
				simplified[(i+1)*4+0] = points[maxi*4+0];
				simplified[(i+1)*4+1] = points[maxi*4+1];
				simplified[(i+1)*4+2] = points[maxi*4+2];
				simplified[(i+1)*4+3] = maxi;
			}
			else
			{
				++i;
			}
		}
	}
	
	for (int i = 0; i < simplified.Count/4; ++i)
	{
		// The edge vertex flag is take from the current raw point,
		// and the neighbour region is take from the next raw point.
		int ai = (simplified[i*4+3]+1) % pn;
		int bi = simplified[i*4+3];
		simplified[i*4+3] = (points[ai*4+3] & (RC_CONTOUR_REG_MASK|RC_AREA_BORDER)) | (points[bi*4+3] & RC_BORDER_VERTEX);
	}
	
}

public static void removeDegenerateSegments(List<int> simplified)
{
	// Remove adjacent vertices which are equal on xz-plane,
	// or else the triangulator will get confused.
	for (int i = 0; i < simplified.Count/4; ++i)
	{
		int ni = i+1;
		if (ni >= (simplified.Count/4))
			ni = 0;
			
		if (simplified[i*4+0] == simplified[ni*4+0] &&
			simplified[i*4+2] == simplified[ni*4+2])
		{
			// Degenerate segment, remove.
			for (int j = i; j < simplified.Count/4-1; ++j)
			{
				simplified[j*4+0] = simplified[(j+1)*4+0];
				simplified[j*4+1] = simplified[(j+1)*4+1];
				simplified[j*4+2] = simplified[(j+1)*4+2];
				simplified[j*4+3] = simplified[(j+1)*4+3];
			}
			//simplified.Capacity = (simplified.Count-4);
            rccsResizeList(simplified, simplified.Count - 4);
		}
	}
}

public static int calcAreaOfPolygon2D(int[] verts, int nverts)
{
	int area = 0;
	for (int i = 0, j = nverts-1; i < nverts; j=i++)
	{
        int viStart = i * 4;
        int vjStart = j * 4;
        area += verts[viStart + 0] * verts[vjStart + 2] - verts[vjStart + 0] * verts[viStart + 2];
	}
	return (area+1) / 2;
}

public static bool ileft(int[] a, int[] b, int[] c)
{
	return (b[0] - a[0]) * (c[2] - a[2]) - (c[0] - a[0]) * (b[2] - a[2]) <= 0;
}


public static bool ileft(int[] a,int aStart, int[] b, int bStart, int[] c, int cStart) {
    return (b[bStart + 0] - a[aStart + 0]) * (c[cStart + 2] - a[aStart + 2]) - (c[cStart + 0] - a[aStart + 0]) * (b[bStart + 2] - a[aStart + 2]) <= 0;
}

public static void getClosestIndices(int[] vertsa, int nvertsa,
							  int[] vertsb, int nvertsb,
							  ref int ia, ref int ib)
{
	int closestDist = 0xfffffff;
	ia = -1;
    ib = -1;
	for (int i = 0; i < nvertsa; ++i)
	{
		int i_n = (i+1) % nvertsa;
		int ip = (i+nvertsa-1) % nvertsa;
        int vaStart = i * 4;
        int vanStart = i_n * 4;
        int vapStart = ip * 4;
		
		for (int j = 0; j < nvertsb; ++j)
		{
            int vbStart = j * 4;
			// vb must be "infront" of va.
			if (ileft(vertsa,vapStart,vertsa,vaStart,vertsb,vbStart) && ileft(vertsa,vaStart,vertsa,vanStart,vertsb,vbStart))
			{
				int dx = vertsb[vbStart+0] - vertsa[vaStart + 0];
				int dz = vertsb[vbStart+2] - vertsa[vaStart+2];
				int d = dx*dx + dz*dz;
				if (d < closestDist)
				{
					ia = i;
					ib = j;
					closestDist = d;
				}
			}
		}
	}
}

public static bool mergeContours(ref rcContour ca, ref rcContour cb, int ia, int ib)
{
	int maxVerts = ca.nverts + cb.nverts + 2;
	int[] verts = new int[maxVerts * 4];//(int*)rcAlloc(sizeof(int)*maxVerts*4, RC_ALLOC_PERM);
	if (verts == null)
		return false;

	int nv = 0;

	// Copy contour A.
	for (int i = 0; i <= ca.nverts; ++i)
	{
		//int* dst = &verts[nv*4];
        int dstIndex = nv*4;
        int srcIndex = ((ia+i)%ca.nverts)*4;
        for (int j=0;i<4;++i){
            verts[dstIndex + j] = ca.verts[srcIndex + j];
        }
		nv++;
	}

	// Copy contour B
	for (int i = 0; i <= cb.nverts; ++i)
	{
        int dstIndex = nv*4;
        int srcIndex = ((ib+i)%cb.nverts)*4;
		//int* dst = &verts[nv*4];
		//const int* src = &cb.verts[((ib+i)%cb.nverts)*4];
        for (int j=0;j<4;++j){
            verts[dstIndex + j] = cb.verts[srcIndex + j];
        }
		nv++;
	}
	
	ca.verts = verts;
	ca.nverts = nv;
	
	cb.verts = null;
	cb.nverts = 0;
	
	return true;
}

/// @par
///
/// The raw contours will match the region outlines exactly. The @p maxError and @p maxEdgeLen
/// parameters control how closely the simplified contours will match the raw contours.
///
/// Simplified contours are generated such that the vertices for portals between areas match up. 
/// (They are considered mandatory vertices.)
///
/// Setting @p maxEdgeLength to zero will disabled the edge length feature.
/// 
/// See the #rcConfig documentation for more information on the configuration parameters.
/// 
/// @see rcAllocContourSet, rcCompactHeightfield, rcContourSet, rcConfig
public static bool rcBuildContours(rcContext ctx, rcCompactHeightfield chf,
					 float maxError, int maxEdgeLen,
					 rcContourSet cset, int buildFlags)
{
	Debug.Assert(ctx != null, "rcContext is null");
	
	int w = chf.width;
	int h = chf.height;
	int borderSize = chf.borderSize;
	
	ctx.startTimer(rcTimerLabel.RC_TIMER_BUILD_CONTOURS);
	
	rcVcopy(cset.bmin, chf.bmin);
	rcVcopy(cset.bmax, chf.bmax);
	if (borderSize > 0)
	{
		// If the heightfield was build with bordersize, remove the offset.
		float pad = borderSize*chf.cs;
		cset.bmin[0] += pad;
		cset.bmin[2] += pad;
		cset.bmax[0] -= pad;
		cset.bmax[2] -= pad;
	}
	cset.cs = chf.cs;
	cset.ch = chf.ch;
	cset.width = chf.width - chf.borderSize*2;
	cset.height = chf.height - chf.borderSize*2;
	cset.borderSize = chf.borderSize;
	
	int maxContours = Math.Max((int)chf.maxRegions, 8);
	//cset.conts = (rcContour*)rcAlloc(sizeof(rcContour)*maxContours, RC_ALLOC_PERM);
    cset.conts = new rcContour[maxContours];
	//if (cset.conts == null)
//		return false;
	cset.nconts = 0;
	
	//rcScopedDelete<byte> flags = (byte*)rcAlloc(sizeof(byte)*chf.spanCount, RC_ALLOC_TEMP);
    byte[] flags = new byte[chf.spanCount];
	if (flags == null)
	{
		ctx.log(rcLogCategory.RC_LOG_ERROR, "rcBuildContours: Out of memory 'flags' " + chf.spanCount);
		return false;
	}
	
	ctx.startTimer(rcTimerLabel.RC_TIMER_BUILD_CONTOURS_TRACE);
	
	// Mark boundaries.
	for (int y = 0; y < h; ++y)
	{
		for (int x = 0; x < w; ++x)
		{
			rcCompactCell c = chf.cells[x+y*w];
			for (int i = (int)c.index, ni = (int)(c.index+c.count); i < ni; ++i)
			{
				byte res = 0;
				rcCompactSpan s = chf.spans[i];
				if (chf.spans[i].reg == 0 || (chf.spans[i].reg & RC_BORDER_REG) != 0)
				{
					flags[i] = 0;
					continue;
				}
				for (int dir = 0; dir < 4; ++dir)
				{
					ushort r = 0;
					if (rcGetCon(s, dir) != RC_NOT_CONNECTED)
					{
						int ax = x + rcGetDirOffsetX(dir);
						int ay = y + rcGetDirOffsetY(dir);
						int ai = (int)chf.cells[ax+ay*w].index + rcGetCon(s, dir);
						r = chf.spans[ai].reg;
					}
					if (r == chf.spans[i].reg)
						res |= (byte)(1 << dir);
				}
				flags[i] = (byte)(res ^ 0xf); // Inverse, mark non connected edges.
			}
		}
	}
	
	ctx.stopTimer(rcTimerLabel.RC_TIMER_BUILD_CONTOURS_TRACE);
	
	//List<int> verts(256);
    List<int> verts = new List<int>();
    verts.Capacity = 256;
	//List<int> simplified(64);
    List<int> simplified = new List<int>();
    simplified.Capacity = 64;
	
	for (int y = 0; y < h; ++y)
	{
		for (int x = 0; x < w; ++x)
		{
			rcCompactCell c = chf.cells[x+y*w];
			for (int i = (int)c.index, ni = (int)(c.index+c.count); i < ni; ++i)
			{
				if (flags[i] == 0 || flags[i] == 0xf)
				{
					flags[i] = 0;
					continue;
				}
				ushort reg = chf.spans[i].reg;
                if (reg == 0 || (reg & RC_BORDER_REG) != 0) {
                    continue;
                }
				byte area = chf.areas[i];
				
				//verts.resize(0);
				//simplified.resize(0);
                verts.Clear();
                simplified.Clear();

				ctx.startTimer(rcTimerLabel.RC_TIMER_BUILD_CONTOURS_TRACE);
				walkContour(x, y, i, chf, flags, verts);
				ctx.stopTimer(rcTimerLabel.RC_TIMER_BUILD_CONTOURS_TRACE);

				ctx.startTimer(rcTimerLabel.RC_TIMER_BUILD_CONTOURS_SIMPLIFY);
				simplifyContour(verts, simplified, maxError, maxEdgeLen, buildFlags);
				removeDegenerateSegments(simplified);
				ctx.stopTimer(rcTimerLabel.RC_TIMER_BUILD_CONTOURS_SIMPLIFY);
				

				// Store region.contour remap info.
				// Create contour.
				if (simplified.Count/4 >= 3)
				{
					if (cset.nconts >= maxContours)
					{
						// Allocate more contours.
						// This can happen when there are tiny holes in the heightfield.
						int oldMax = maxContours;
						maxContours *= 2;
                        rcContour[] newConts = new rcContour[maxContours];// (rcContour*)rcAlloc(sizeof(rcContour) * maxContours, RC_ALLOC_PERM);
						for (int j = 0; j < cset.nconts; ++j)
						{
							newConts[j] = cset.conts[j];
							// Reset source pointers to prevent data deletion.
							cset.conts[j].verts = null;
							cset.conts[j].rverts = null;
						}
						//rcFree(cset.conts);
						cset.conts = newConts;
					
						ctx.log(rcLogCategory.RC_LOG_WARNING, "rcBuildContours: Expanding max contours from " +  oldMax + " to "+ maxContours);
					}
					
					int contId = cset.nconts;
						cset.nconts++;
					rcContour cont = cset.conts[contId];
                    
					cont.nverts = simplified.Count/4;
                    cont.verts = new int[cont.nverts * 4]; //(int*)rcAlloc(sizeof(int)*cont.nverts*4, RC_ALLOC_PERM);
					if (cont.verts == null)
					{
						ctx.log(rcLogCategory.RC_LOG_ERROR, "rcBuildContours: Out of memory 'verts' " + cont.nverts);
						return false;
					}
					//memcpy(cont.verts, &simplified[0], sizeof(int)*cont.nverts*4);
                    for (int j = 0; j < cont.nverts * 4; ++j) {
                        cont.verts[j] = simplified[j];
                    }
					if (borderSize > 0)
					{
						// If the heightfield was build with bordersize, remove the offset.
						for (int j = 0; j < cont.nverts; ++j)
						{
							//int* v = &cont.verts[j*4];
                            cont.verts[j * 4] -= borderSize;
                            cont.verts[j*4  + 2] -= borderSize;
							//v[0] -= borderSize;
							//v[2] -= borderSize;
						}
					}
					
					cont.nrverts = verts.Count/4;
                    cont.rverts = new int[cont.nrverts * 4];//(int*)rcAlloc(sizeof(int)*cont.nrverts*4, RC_ALLOC_PERM);
					if (cont.rverts == null)
					{
						ctx.log(rcLogCategory.RC_LOG_ERROR, "rcBuildContours: Out of memory 'rverts' " + cont.nrverts);
						return false;
					}
					//memcpy(cont.rverts, &verts[0], sizeof(int)*cont.nrverts*4);
                    for (int j = 0; j < cont.nrverts * 4; ++j) {
                        cont.rverts[j] = verts[j];
                    }
					if (borderSize > 0)
					{
						// If the heightfield was build with bordersize, remove the offset.
						for (int j = 0; j < cont.nrverts; ++j)
						{
							//int* v = &cont.rverts[j*4];
                            cont.rverts[j * 4] -= borderSize;
                            cont.rverts[j * 4 + 2] -= borderSize;
						}
					}
					
/*					cont.cx = cont.cy = cont.cz = 0;
					for (int i = 0; i < cont.nverts; ++i)
					{
						cont.cx += cont.verts[i*4+0];
						cont.cy += cont.verts[i*4+1];
						cont.cz += cont.verts[i*4+2];
					}
					cont.cx /= cont.nverts;
					cont.cy /= cont.nverts;
					cont.cz /= cont.nverts;*/
					
					cont.reg = reg;
					cont.area = area;

					cset.conts[contId] = cont;
				}
			}
		}
	}
	
	// Check and merge droppings.
	// Sometimes the previous algorithms can fail and create several contours
	// per area. This pass will try to merge the holes into the main region.
	for (int i = 0; i < cset.nconts; ++i)
	{
		rcContour cont = cset.conts[i];
		// Check if the contour is would backwards.
		if (calcAreaOfPolygon2D(cont.verts, cont.nverts) < 0)
		{
			// Find another contour which has the same region ID.
			int mergeIdx = -1;
			for (int j = 0; j < cset.nconts; ++j)
			{
				if (i == j) continue;
				if (cset.conts[j].nverts != 0 && cset.conts[j].reg == cont.reg)
				{
					// Make sure the polygon is correctly oriented.
					if (calcAreaOfPolygon2D(cset.conts[j].verts, cset.conts[j].nverts) != 0)
					{
						mergeIdx = j;
						break;
					}
				}
			}
			if (mergeIdx == -1)
			{
				ctx.log(rcLogCategory.RC_LOG_WARNING, "rcBuildContours: Could not find merge target for bad contour " + i);
			}
			else
			{
				rcContour mcont = cset.conts[mergeIdx];
				// Merge by closest points.
				int ia = 0, ib = 0;
				getClosestIndices(mcont.verts, mcont.nverts, cont.verts, cont.nverts, ref ia, ref ib);
				if (ia == -1 || ib == -1)
				{
					ctx.log(rcLogCategory.RC_LOG_WARNING, "rcBuildContours: Failed to find merge points for " +  i + " and " + mergeIdx);
					continue;
				}
				if (!mergeContours(ref mcont,ref cont, ia, ib))
				{
					ctx.log(rcLogCategory.RC_LOG_WARNING, "rcBuildContours: Failed to merge contours " + i + " and " + mergeIdx);
					continue;
				}
                cset.conts[mergeIdx] = mcont;
                cset.conts[i] = cont;
			}
		}
	}
	
	ctx.stopTimer(rcTimerLabel.RC_TIMER_BUILD_CONTOURS);
	
	return true;
}
}