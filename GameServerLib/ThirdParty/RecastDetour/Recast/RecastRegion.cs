using System;
using System.Diagnostics;
using System.Collections.Generic;

public static partial class Recast{
    static void calculateDistanceField( rcContext ctx, rcCompactHeightfield chf, ushort[] src, ref ushort maxDist)
    {
	    int w = chf.width;
	    int h = chf.height;
        
	    // Init distance and points.
	    for (int i = 0; i < chf.spanCount; ++i)
		    src[i] = 0xffff;

	    // Mark boundary cells.
	    for (int y = 0; y < h; ++y)
	    {
		    for (int x = 0; x < w; ++x)
		    {
			    rcCompactCell c = chf.cells[x+y*w];
			    for (int i = (int)c.index, ni = (int)(c.index+c.count); i < ni; ++i)
			    {
				    rcCompactSpan s = chf.spans[i];
				    byte area = chf.areas[i];
				
				    int nc = 0;
				    for (int dir = 0; dir < 4; ++dir)
				    {
					    if (rcGetCon(s, dir) != RC_NOT_CONNECTED)
					    {
						    int ax = x + rcGetDirOffsetX(dir);
						    int ay = y + rcGetDirOffsetY(dir);
						    int ai = (int)chf.cells[ax+ay*w].index + rcGetCon(s, dir);
						    if (area == chf.areas[ai])
							    nc++;
					    }
				    }
				    if (nc != 4)
					    src[i] = 0;
			    }
		    }
	    }
	    // Pass 1
	    for (int y = 0; y < h; ++y)
	    {
		    for (int x = 0; x < w; ++x)
		    {
			    rcCompactCell c = chf.cells[x+y*w];
			    for (int i = (int)c.index, ni = (int)(c.index+c.count); i < ni; ++i)
			    {
				    rcCompactSpan s = chf.spans[i];
				
				    if (rcGetCon(s, 0) != RC_NOT_CONNECTED)
				    {
					    // (-1,0)
					    int ax = x + rcGetDirOffsetX(0);
					    int ay = y + rcGetDirOffsetY(0);
					    int ai = (int)chf.cells[ax+ay*w].index + rcGetCon(s, 0);
					    rcCompactSpan aSpan = chf.spans[ai];
					    if (src[ai]+2 < src[i]){
						    src[i] = (ushort)(src[ai]+2);
                        }
					
					    // (-1,-1)
					    if (rcGetCon(aSpan, 3) != RC_NOT_CONNECTED)
					    {
						    int aax = ax + rcGetDirOffsetX(3);
						    int aay = ay + rcGetDirOffsetY(3);
						    int aai = (int)chf.cells[aax+aay*w].index + rcGetCon(aSpan, 3);
						    if (src[aai]+3 < src[i]){
							    src[i] = (ushort)(src[aai]+3);
                            }
					    }
				    }
				    if (rcGetCon(s, 3) != RC_NOT_CONNECTED)
				    {
					    // (0,-1)
					    int ax = x + rcGetDirOffsetX(3);
					    int ay = y + rcGetDirOffsetY(3);
					    int ai = (int)chf.cells[ax+ay*w].index + rcGetCon(s, 3);
					    rcCompactSpan aSpan = chf.spans[ai];
					    if (src[ai]+2 < src[i]){
						    src[i] = (ushort)(src[ai]+2);
                        }

					    // (1,-1)
					    if (rcGetCon(aSpan, 2) != RC_NOT_CONNECTED)
					    {
						    int aax = ax + rcGetDirOffsetX(2);
						    int aay = ay + rcGetDirOffsetY(2);
						    int aai = (int)chf.cells[aax+aay*w].index + rcGetCon(aSpan, 2);
						    if (src[aai]+3 < src[i]){
							    src[i] = (ushort)(src[aai]+3);
                            }
					    }
				    }
			    }
		    }
	    }
	    // Pass 2
	    for (int y = h-1; y >= 0; --y)
	    {
		    for (int x = w-1; x >= 0; --x)
		    {
			    rcCompactCell c = chf.cells[x+y*w];
			    for (int i = (int)c.index, ni = (int)(c.index+c.count); i < ni; ++i)
			    {
				    rcCompactSpan s = chf.spans[i];
				
				    if (rcGetCon(s, 2) != RC_NOT_CONNECTED)
				    {
					    // (1,0)
					    int ax = x + rcGetDirOffsetX(2);
					    int ay = y + rcGetDirOffsetY(2);
					    int ai = (int)chf.cells[ax+ay*w].index + rcGetCon(s, 2);
					    rcCompactSpan aSpan = chf.spans[ai];
					    if (src[ai]+2 < src[i]){
						    src[i] = (ushort)(src[ai]+2);
                        }
					
					    // (1,1)
					    if (rcGetCon(aSpan, 1) != RC_NOT_CONNECTED)
					    {
						    int aax = ax + rcGetDirOffsetX(1);
						    int aay = ay + rcGetDirOffsetY(1);
						    int aai = (int)chf.cells[aax+aay*w].index + rcGetCon(aSpan, 1);
						    if (src[aai]+3 < src[i]){
							    src[i] = (ushort)(src[aai]+3);
                            }
					    }
				    }
				    if (rcGetCon(s, 1) != RC_NOT_CONNECTED)
				    {
					    // (0,1)
					    int ax = x + rcGetDirOffsetX(1);
					    int ay = y + rcGetDirOffsetY(1);
					    int ai = (int)chf.cells[ax+ay*w].index + rcGetCon(s, 1);
					    rcCompactSpan aSpan = chf.spans[ai];
					    if (src[ai]+2 < src[i]){
						    src[i] = (ushort)(src[ai]+2);
                        }
					
					    // (-1,1)
					    if (rcGetCon(aSpan, 0) != RC_NOT_CONNECTED)
					    {
						    int aax = ax + rcGetDirOffsetX(0);
						    int aay = ay + rcGetDirOffsetY(0);
						    int aai = (int)chf.cells[aax+aay*w].index + rcGetCon(aSpan, 0);
						    if (src[aai]+3 < src[i]){
							    src[i] = (ushort)(src[aai]+3);
                            }
					    }
				    }
			    }
		    }
	    }
	    maxDist = 0;
	    for (int i = 0; i < chf.spanCount; ++i){
		    maxDist = Math.Max(src[i], maxDist);
        }
	
    }

    static ushort[] boxBlur(rcCompactHeightfield chf, int thr,
							       ushort[] src, ushort[] dst)
    {
	    int w = chf.width;
	    int h = chf.height;
	
	    thr *= 2;
	
	    for (int y = 0; y < h; ++y)
	    {
		    for (int x = 0; x < w; ++x)
		    {
			    rcCompactCell c = chf.cells[x+y*w];
			    for (int i = (int)c.index, ni = (int)(c.index+c.count); i < ni; ++i)
			    {
				    rcCompactSpan s = chf.spans[i];
				    ushort cd = src[i];
				    if (cd <= thr)
				    {
					    dst[i] = cd;
					    continue;
				    }

				    int d = (int)cd;
				    for (int dir = 0; dir < 4; ++dir)
				    {
					    if (rcGetCon(s, dir) != RC_NOT_CONNECTED)
					    {
						    int ax = x + rcGetDirOffsetX(dir);
						    int ay = y + rcGetDirOffsetY(dir);
						    int ai = (int)chf.cells[ax+ay*w].index + rcGetCon(s, dir);
						    d += (int)src[ai];
						
						    rcCompactSpan aSpan = chf.spans[ai];
						    int dir2 = (dir+1) & 0x3;
						    if (rcGetCon(aSpan, dir2) != RC_NOT_CONNECTED)
						    {
							    int ax2 = ax + rcGetDirOffsetX(dir2);
							    int ay2 = ay + rcGetDirOffsetY(dir2);
							    int ai2 = (int)chf.cells[ax2+ay2*w].index + rcGetCon(aSpan, dir2);
							    d += (int)src[ai2];
						    }
						    else
						    {
							    d += cd;
						    }
					    }
					    else
					    {
						    d += cd*2;
					    }
				    }
				    dst[i] = (ushort)((d+5)/9);
			    }
		    }
	    }
	    return dst;
    }


    static bool floodRegion(int x, int y, int i,
						    ushort level, ushort r,
						    rcCompactHeightfield chf,
						    ushort[] srcReg, ushort[] srcDist,
						    List<int> stack)
    {
	    int w = chf.width;
	
	    byte area = chf.areas[i];
	
	    // Flood fill mark region.
	    //stack.resize(0);
        stack.Clear();
	    stack.Add((int)x);
	    stack.Add((int)y);
	    stack.Add((int)i);
	    srcReg[i] = r;
	    srcDist[i] = 0;
	
	    ushort lev = (ushort)(level >= 2 ? level-2 : 0);
	    int count = 0;
	
	    while (stack.Count > 0)
	    {
		    int ci = rccsPop(stack);
		    int cy = rccsPop(stack);
		    int cx = rccsPop(stack);
		
		    rcCompactSpan cs = chf.spans[ci];
		
		    // Check if any of the neighbours already have a valid region set.
		    ushort ar = 0;
		    for (int dir = 0; dir < 4; ++dir)
		    {
			    // 8 connected
			    if (rcGetCon(cs, dir) != RC_NOT_CONNECTED)
			    {
				    int ax = cx + rcGetDirOffsetX(dir);
				    int ay = cy + rcGetDirOffsetY(dir);
				    int ai = (int)chf.cells[ax+ay*w].index + rcGetCon(cs, dir);
				    if (chf.areas[ai] != area)
					    continue;
				    ushort nr = srcReg[ai];
				    if ((nr & RC_BORDER_REG) != 0) // Do not take borders into account.
					    continue;
				    if (nr != 0 && nr != r)
				    {
					    ar = nr;
					    break;
				    }
				
				    rcCompactSpan aSpan = chf.spans[ai];
				
				    int dir2 = (dir+1) & 0x3;
				    if (rcGetCon(aSpan, dir2) != RC_NOT_CONNECTED)
				    {
					    int ax2 = ax + rcGetDirOffsetX(dir2);
					    int ay2 = ay + rcGetDirOffsetY(dir2);
					    int ai2 = (int)chf.cells[ax2+ay2*w].index + rcGetCon(aSpan, dir2);
					    if (chf.areas[ai2] != area)
						    continue;
					    ushort nr2 = srcReg[ai2];
					    if (nr2 != 0 && nr2 != r)
					    {
						    ar = nr2;
						    break;
					    }
				    }				
			    }
		    }
		    if (ar != 0)
		    {
			    srcReg[ci] = 0;
			    continue;
		    }
		    count++;
		
		    // Expand neighbours.
		    for (int dir = 0; dir < 4; ++dir)
		    {
			    if (rcGetCon(cs, dir) != RC_NOT_CONNECTED)
			    {
				    int ax = cx + rcGetDirOffsetX(dir);
				    int ay = cy + rcGetDirOffsetY(dir);
				    int ai = (int)chf.cells[ax+ay*w].index + rcGetCon(cs, dir);
				    if (chf.areas[ai] != area)
					    continue;
				    if (chf.dist[ai] >= lev && srcReg[ai] == 0)
				    {
					    srcReg[ai] = r;
					    srcDist[ai] = 0;
					    stack.Add(ax);
					    stack.Add(ay);
					    stack.Add(ai);
				    }
			    }
		    }
	    }
	
	    return count > 0;
    }

    static ushort[] expandRegions(int maxIter, ushort level,
									     rcCompactHeightfield chf,
									     ushort[] srcReg, ushort[] srcDist,
									     ushort[] dstReg, ushort[] dstDist, 
									     List<int> stack,
									     bool fillStack)
    {
	    int w = chf.width;
	    int h = chf.height;

	    if (fillStack)
	    {
		    // Find cells revealed by the raised level.
		    stack.Clear();
		    for (int y = 0; y < h; ++y)
		    {
			    for (int x = 0; x < w; ++x)
			    {
				    rcCompactCell c = chf.cells[x+y*w];
				    for (int i = (int)c.index, ni = (int)(c.index+c.count); i < ni; ++i)
				    {
					    if (chf.dist[i] >= level && srcReg[i] == 0 && chf.areas[i] != RC_NULL_AREA)
					    {
						    stack.Add(x);
						    stack.Add(y);
						    stack.Add(i);
					    }
				    }
			    }
		    }
	    }
	    else // use cells in the input stack
	    {
		    // mark all cells which already have a region
		    for (int j=0; j<stack.Count; j+=3)
		    {
			    int i = stack[j+2];
			    if (srcReg[i] != 0)
				    stack[j+2] = -1;
		    }
	    }

	    int iter = 0;
	    while (stack.Count > 0)
	    {
		    int failed = 0;
		
		    //memcpy(dstReg, srcReg, sizeof(ushort)*chf.spanCount);
            for (int i=0;i<chf.spanCount;++i){
                dstReg[i] = srcReg[i];
            }
		    //memcpy(dstDist, srcDist, sizeof(ushort)*chf.spanCount);
		    for (int i=0;i<chf.spanCount;++i){
                dstDist[i] = srcDist[i];
            }
		    for (int j = 0; j < stack.Count; j += 3)
		    {
			    int x = stack[j+0];
			    int y = stack[j+1];
			    int i = stack[j+2];
			    if (i < 0)
			    {
				    failed++;
				    continue;
			    }
			
			    ushort r = srcReg[i];
			    ushort d2 = 0xffff;
			    byte area = chf.areas[i];
			    rcCompactSpan s = chf.spans[i];
			    for (int dir = 0; dir < 4; ++dir)
			    {
				    if (rcGetCon(s, dir) == RC_NOT_CONNECTED) 
                        continue;
				    int ax = x + rcGetDirOffsetX(dir);
				    int ay = y + rcGetDirOffsetY(dir);
				    int ai = (int)chf.cells[ax+ay*w].index + rcGetCon(s, dir);
				    if (chf.areas[ai] != area) continue;
				    if (srcReg[ai] > 0 && (srcReg[ai] & RC_BORDER_REG) == 0)
				    {
					    if ((int)srcDist[ai]+2 < (int)d2)
					    {
						    r = srcReg[ai];
						    d2 = (ushort)(srcDist[ai]+2);
					    }
				    }
			    }
			    if (r != 0)
			    {
				    stack[j+2] = -1; // mark as used
				    dstReg[i] = r;
				    dstDist[i] = d2;
			    }
			    else
			    {
				    failed++;
			    }
		    }
		
		    // rcSwap source and dest.
		    rcSwap(ref srcReg, ref dstReg);
		    rcSwap(ref srcDist, ref dstDist);
		
		    if (failed*3 == stack.Count)
			    break;
		
		    if (level > 0)
		    {
			    ++iter;
			    if (iter >= maxIter)
				    break;
		    }
	    }
	
	    return srcReg;
    }



    static void sortCellsByLevel(ushort startLevel,
							      rcCompactHeightfield chf,
							      ushort[] srcReg,
							      uint nbStacks, List<int>[] stacks,
							      ushort loglevelsPerStack) // the levels per stack (2 in our case) as a bit shift
    {
	    int w = chf.width;
	    int h = chf.height;
	    startLevel = (ushort)(startLevel >> loglevelsPerStack);

	    for (uint j=0; j<nbStacks; ++j)
		    stacks[j].Clear();

	    // put all cells in the level range into the appropriate stacks
	    for (int y = 0; y < h; ++y)
	    {
		    for (int x = 0; x < w; ++x)
		    {
			    rcCompactCell c = chf.cells[x+y*w];
			    for (int i = (int)c.index, ni = (int)(c.index+c.count); i < ni; ++i)
			    {
				    if (chf.areas[i] == RC_NULL_AREA || srcReg[i] != 0)
					    continue;

				    int level = chf.dist[i] >> loglevelsPerStack;
				    int sId = startLevel - level;
				    if (sId >= (int)nbStacks)
					    continue;
				    if (sId < 0)
					    sId = 0;

				    stacks[sId].Add(x);
				    stacks[sId].Add(y);
				    stacks[sId].Add(i);
			    }
		    }
	    }
    }


    static void appendStacks(List<int> srcStack, List<int> dstStack,
						     ushort[] srcReg)
    {
	    for (int j=0; j<srcStack.Count; j+=3)
	    {
		    int i = srcStack[j+2];
		    if ((i < 0) || (srcReg[i] != 0))
			    continue;
		    dstStack.Add(srcStack[j]);
		    dstStack.Add(srcStack[j+1]);
		    dstStack.Add(srcStack[j+2]);
	    }
    }

    public class rcRegion
    {
	    public rcRegion(ushort i) {
            id = i;
        }
		    
	    public int spanCount = 0;			// Number of spans belonging to this region
	    public ushort id = 0;				// ID of the region
	    public byte areaType = 0;			// Area type.
	    public bool remap = false;
	    public bool visited = false;
	    public List<int> connections = new List<int>();
	    public List<int> floors = new List<int>();
    };

    static void removeAdjacentNeighbours(rcRegion reg)
    {
	    // Remove adjacent duplicates.
	    for (int i = 0; i < reg.connections.Count && reg.connections.Count > 1; )
	    {
		    int ni = (i+1) % reg.connections.Count;
		    if (reg.connections[i] == reg.connections[ni])
		    {
			    // Remove duplicate
			    for (int j = i; j < reg.connections.Count-1; ++j){
				    reg.connections[j] = reg.connections[j+1];
                }
			    rccsPop(reg.connections);
		    }
		    else
			    ++i;
	    }
    }

    static void replaceNeighbour(rcRegion reg, ushort oldId, ushort newId)
    {
	    bool neiChanged = false;
	    for (int i = 0; i < reg.connections.Count; ++i)
	    {
		    if (reg.connections[i] == oldId)
		    {
			    reg.connections[i] = newId;
			    neiChanged = true;
		    }
	    }
	    for (int i = 0; i < reg.floors.Count; ++i)
	    {
		    if (reg.floors[i] == oldId)
			    reg.floors[i] = newId;
	    }
	    if (neiChanged)
		    removeAdjacentNeighbours(reg);
    }

    static bool canMergeWithRegion(rcRegion rega, rcRegion regb)
    {
	    if (rega.areaType != regb.areaType)
		    return false;
	    int n = 0;
	    for (int i = 0; i < rega.connections.Count; ++i)
	    {
		    if (rega.connections[i] == regb.id)
			    n++;
	    }
	    if (n > 1)
		    return false;
	    for (int i = 0; i < rega.floors.Count; ++i)
	    {
		    if (rega.floors[i] == regb.id)
			    return false;
	    }
	    return true;
    }

    static void addUniqueFloorRegion(rcRegion reg, int n)
    {
	    for (int i = 0; i < reg.floors.Count; ++i)
		    if (reg.floors[i] == n)
			    return;
	    reg.floors.Add(n);
    }

    static bool mergeRegions(rcRegion rega, rcRegion regb)
    {
	    ushort aid = rega.id;
	    ushort bid = regb.id;
	
	    // Duplicate current neighbourhood.
	    List<int> acon = new List<int>();
	    
	    for (int i = 0; i < rega.connections.Count; ++i)
		    acon.Add( rega.connections[i] );
	    List<int> bcon = regb.connections;
	
	    // Find insertion point on A.
	    int insa = -1;
	    for (int i = 0; i < acon.Count; ++i)
	    {
		    if (acon[i] == bid)
		    {
			    insa = i;
			    break;
		    }
	    }
	    if (insa == -1)
		    return false;
	
	    // Find insertion point on B.
	    int insb = -1;
	    for (int i = 0; i < bcon.Count; ++i)
	    {
		    if (bcon[i] == aid)
		    {
			    insb = i;
			    break;
		    }
	    }
	    if (insb == -1)
		    return false;
	
	    // Merge neighbours.
	    rega.connections.Clear();
	    for (int i = 0, ni = acon.Count; i < ni-1; ++i)
		    rega.connections.Add(acon[(insa+1+i) % ni]);
		
	    for (int i = 0, ni = bcon.Count; i < ni-1; ++i)
		    rega.connections.Add(bcon[(insb+1+i) % ni]);
	
	    removeAdjacentNeighbours(rega);
	
	    for (int j = 0; j < regb.floors.Count; ++j)
		    addUniqueFloorRegion(rega, regb.floors[j]);
	    rega.spanCount += regb.spanCount;
	    regb.spanCount = 0;
	    regb.connections.Clear();

	    return true;
    }

    static bool isRegionConnectedToBorder(rcRegion reg)
    {
	    // Region is connected to border if
	    // one of the neighbours is null id.
	    for (int i = 0; i < reg.connections.Count; ++i)
	    {
		    if (reg.connections[i] == 0)
			    return true;
	    }
	    return false;
    }

    static bool isSolidEdge(rcCompactHeightfield chf, ushort[] srcReg,
						    int x, int y, int i, int dir)
    {
	    rcCompactSpan s = chf.spans[i];
	    ushort r = 0;
	    if (rcGetCon(s, dir) != RC_NOT_CONNECTED)
	    {
		    int ax = x + rcGetDirOffsetX(dir);
		    int ay = y + rcGetDirOffsetY(dir);
		    int ai = (int)chf.cells[ax+ay*chf.width].index + rcGetCon(s, dir);
		    r = srcReg[ai];
	    }
	    if (r == srcReg[i])
		    return false;
	    return true;
    }

    static void walkContour(int x, int y, int i, int dir,
						    rcCompactHeightfield chf,
						    ushort[] srcReg,
						    List<int> cont)
    {
	    int startDir = dir;
	    int starti = i;

	    rcCompactSpan ss = chf.spans[i];
	    ushort curReg = 0;
	    if (rcGetCon(ss, dir) != RC_NOT_CONNECTED)
	    {
		    int ax = x + rcGetDirOffsetX(dir);
		    int ay = y + rcGetDirOffsetY(dir);
		    int ai = (int)chf.cells[ax+ay*chf.width].index + rcGetCon(ss, dir);
		    curReg = srcReg[ai];
	    }
	    cont.Add(curReg);
			
	    int iter = 0;
	    while (++iter < 40000)
	    {
		    rcCompactSpan s = chf.spans[i];
		
		    if (isSolidEdge(chf, srcReg, x, y, i, dir))
		    {
			    // Choose the edge corner
			    ushort r = 0;
			    if (rcGetCon(s, dir) != RC_NOT_CONNECTED)
			    {
				    int ax = x + rcGetDirOffsetX(dir);
				    int ay = y + rcGetDirOffsetY(dir);
				    int ai = (int)chf.cells[ax+ay*chf.width].index + rcGetCon(s, dir);
				    r = srcReg[ai];
			    }
			    if (r != curReg)
			    {
				    curReg = r;
				    cont.Add(curReg);
			    }
			
			    dir = (dir+1) & 0x3;  // Rotate CW
		    }
		    else
		    {
			    int ni = -1;
			    int nx = x + rcGetDirOffsetX(dir);
			    int ny = y + rcGetDirOffsetY(dir);
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
			    dir = (dir+3) & 0x3;	// Rotate CCW
		    }
		
		    if (starti == i && startDir == dir)
		    {
			    break;
		    }
	    }

	    // Remove adjacent duplicates.
	    if (cont.Count > 1)
	    {
		    for (int j = 0; j < cont.Count; )
		    {
			    int nj = (j+1) % cont.Count;
			    if (cont[j] == cont[nj])
			    {
				    for (int k = j; k < cont.Count-1; ++k)
					    cont[k] = cont[k+1];
				    rccsPop(cont); 
			    }
			    else
				    ++j;
		    }
	    }
    }

    static bool filterSmallRegions(rcContext ctx, int minRegionArea, int mergeRegionSize,
							       ref ushort maxRegionId,
							       rcCompactHeightfield chf,
							       ushort[] srcReg)
    {
		int w = chf.width;
	    int h = chf.height;
	
	    int nreg = maxRegionId+1;

        rcRegion[] regions = new rcRegion[nreg];
	    if (regions == null)
	    {
		    ctx.log(rcLogCategory.RC_LOG_ERROR, "filterSmallRegions: Out of memory 'regions' (" +nreg+ ").");
		    return false;
	    }

	    // Construct regions
	    for (int i = 0; i < nreg; ++i){
            regions[i] = new rcRegion((ushort) i);
        }
	
	    // Find edge of a region and find connections around the contour.
	    for (int y = 0; y < h; ++y)
	    {
		    for (int x = 0; x < w; ++x)
		    {
			    rcCompactCell c = chf.cells[x+y*w];
			    for (int i = (int)c.index, ni = (int)(c.index+c.count); i < ni; ++i)
			    {
				    ushort r = srcReg[i];
				    if (r == 0 || r >= nreg)
					    continue;
				
				    rcRegion reg = regions[r];
				    reg.spanCount++;
				
				    // Update floors.
				    for (int j = (int)c.index; j < ni; ++j)
				    {
					    if (i == j) continue;
					    ushort floorId = srcReg[j];
					    if (floorId == 0 || floorId >= nreg)
						    continue;
					    addUniqueFloorRegion(reg, floorId);
				    }
				
				    // Have found contour
				    if (reg.connections.Count > 0)
					    continue;
				
				    reg.areaType = chf.areas[i];
				
				    // Check if this cell is next to a border.
				    int ndir = -1;
				    for (int dir = 0; dir < 4; ++dir)
				    {
					    if (isSolidEdge(chf, srcReg, x, y, i, dir))
					    {
						    ndir = dir;
						    break;
					    }
				    }
				
				    if (ndir != -1)
				    {
					    // The cell is at border.
					    // Walk around the contour to find all the neighbours.
					    walkContour(x, y, i, ndir, chf, srcReg, reg.connections);
				    }
			    }
		    }
	    }
		
	    // Remove too small regions.
	    List<int> stack = new List<int>();//(32);
	    List<int> trace= new List<int>();//(32);
		stack.Capacity = 32;
		trace.Capacity = 32;
	    for (int i = 0; i < nreg; ++i)
	    {
		    rcRegion reg = regions[i];
		    if (reg.id == 0 || (reg.id & RC_BORDER_REG) != 0)
			    continue;                       
		    if (reg.spanCount == 0)
			    continue;
		    if (reg.visited)
			    continue;
		
		    // Count the total size of all the connected regions.
		    // Also keep track of the regions connects to a tile border.
		    bool connectsToBorder = false;
		    int spanCount = 0;
		    stack.Clear();
		    trace.Clear();

		    reg.visited = true;
		    stack.Add(i);
		
		    while (stack.Count != 0)
		    {
			    // Pop
			    int ri = rccsPop(stack);
			
			    rcRegion creg = regions[ri];

			    spanCount += creg.spanCount;
			    trace.Add(ri);

			    for (int j = 0; j < creg.connections.Count; ++j)
			    {
				    if ((creg.connections[j] & RC_BORDER_REG) != 0)
				    {
					    connectsToBorder = true;
					    continue;
				    }
				    rcRegion neireg = regions[creg.connections[j]];
				    if (neireg.visited)
					    continue;
				    if (neireg.id == 0 || (neireg.id & RC_BORDER_REG) != 0)
					    continue;
				    // Visit
				    stack.Add(neireg.id);
				    neireg.visited = true;
			    }
		    }
		
		    // If the accumulated regions size is too small, remove it.
		    // Do not remove areas which connect to tile borders
		    // as their size cannot be estimated correctly and removing them
		    // can potentially remove necessary areas.
		    if (spanCount < minRegionArea && !connectsToBorder)
		    {
			    // Kill all visited regions.
			    for (int j = 0; j < trace.Count; ++j)
			    {
				    regions[trace[j]].spanCount = 0;
				    regions[trace[j]].id = 0;
			    }
		    }
	    }

	    // Merge too small regions to neighbour regions.
	    int mergeCount = 0 ;
	    do
	    {
		    mergeCount = 0;
		    for (int i = 0; i < nreg; ++i)
		    {
			    rcRegion reg = regions[i];
			    if (reg.id == 0 || (reg.id & RC_BORDER_REG) != 0)
				    continue;                       
			    if (reg.spanCount == 0)
				    continue;
			
			    // Check to see if the region should be merged.
			    if (reg.spanCount > mergeRegionSize && isRegionConnectedToBorder(reg))
				    continue;
			
			    // Small region with more than 1 connection.
			    // Or region which is not connected to a border at all.
			    // Find smallest neighbour region that connects to this one.
			    int smallest = 0xfffffff;
			    ushort mergeId = reg.id;
			    for (int j = 0; j < reg.connections.Count; ++j)
			    {
				    if ((reg.connections[j] & RC_BORDER_REG) != 0) 
                        continue;
				    rcRegion mreg = regions[reg.connections[j]];
				    if (mreg.id == 0 || (mreg.id & RC_BORDER_REG) != 0) 
                        continue;
				    if (mreg.spanCount < smallest &&
					    canMergeWithRegion(reg, mreg) &&
					    canMergeWithRegion(mreg, reg))
				    {
					    smallest = mreg.spanCount;
					    mergeId = mreg.id;
				    }
			    }
			    // Found new id.
			    if (mergeId != reg.id)
			    {
				    ushort oldId = reg.id;
				    rcRegion target = regions[mergeId];

				    // Merge neighbours.
					if (   mergeRegions(target, reg))
				    {
					    // Fixup regions pointing to current region.
					    for (int j = 0; j < nreg; ++j)
					    {


						    if (regions[j].id == 0 || (regions[j].id & RC_BORDER_REG) != 0) 
                                continue;
						    // If another region was already merged into current region
						    // change the nid of the previous region too.
						    if (regions[j].id == oldId)
							    regions[j].id = mergeId;
						    // Replace the current region with the new one if the
						    // current regions is neighbour.
						    replaceNeighbour(regions[j], oldId, mergeId);
					    }
					    mergeCount++;
				    }
				    
			    }
		    }
	    }
	    while (mergeCount > 0);
	
	    // Compress region Ids.
	    for (int i = 0; i < nreg; ++i)
	    {
		    regions[i].remap = false;
		    if (regions[i].id == 0) 
                continue;       // Skip nil regions.
		    if ((regions[i].id & RC_BORDER_REG) != 0) 
                continue;    // Skip external regions.
		    regions[i].remap = true;
	    }
	
	    ushort regIdGen = 0;
	    for (int i = 0; i < nreg; ++i)
	    {
		    if (!regions[i].remap)
			    continue;
		    ushort oldId = regions[i].id;
		    ushort newId = ++regIdGen;
		    for (int j = i; j < nreg; ++j)
		    {
			    if (regions[j].id == oldId)
			    {
				    regions[j].id = newId;
				    regions[j].remap = false;
			    }
		    }
	    }
	    maxRegionId = regIdGen;
	
	    // Remap regions.
	    for (int i = 0; i < chf.spanCount; ++i)
	    {
		    if ((srcReg[i] & RC_BORDER_REG) == 0)
			    srcReg[i] = regions[srcReg[i]].id;
	    }
	
	    return true;
    }

    /// @par
    /// 
    /// This is usually the second to the last step in creating a fully built
    /// compact heightfield.  This step is required before regions are built
    /// using #rcBuildRegions or #rcBuildRegionsMonotone.
    /// 
    /// After this step, the distance data is available via the rcCompactHeightfield::maxDistance
    /// and rcCompactHeightfield::dist fields.
    ///
    /// @see rcCompactHeightfield, rcBuildRegions, rcBuildRegionsMonotone
    public static bool rcBuildDistanceField(rcContext ctx, rcCompactHeightfield chf)
    {
	    Debug.Assert(ctx != null, "rcContext is null");
	
	    ctx.startTimer(rcTimerLabel.RC_TIMER_BUILD_DISTANCEFIELD);

	    chf.dist = null;

	    //ushort* src = (ushort*)rcAlloc(sizeof(ushort)*chf.spanCount, RC_ALLOC_TEMP);
        ushort[] src = new ushort[chf.spanCount];
	    if (src == null)
	    {
		    ctx.log(rcLogCategory.RC_LOG_ERROR, "rcBuildDistanceField: Out of memory 'src' ("+chf.spanCount+").");
		    return false;
	    }
	    //ushort* dst = (ushort*)rcAlloc(sizeof(ushort)*chf.spanCount, RC_ALLOC_TEMP);
        ushort[] dst = new ushort[chf.spanCount];
	    if (dst == null)
	    {
		    ctx.log(rcLogCategory.RC_LOG_ERROR, "rcBuildDistanceField: Out of memory 'dst' ("+chf.spanCount+").");
		    //rcFree(src);
		    return false;
	    }
	
	    ushort maxDist = 0;
        
	    ctx.startTimer(rcTimerLabel.RC_TIMER_BUILD_DISTANCEFIELD_DIST);
	
	    calculateDistanceField(ctx, chf, src, ref maxDist);
	    chf.maxDistance = maxDist;
	
	    ctx.stopTimer(rcTimerLabel.RC_TIMER_BUILD_DISTANCEFIELD_DIST);
	
	    ctx.startTimer(rcTimerLabel.RC_TIMER_BUILD_DISTANCEFIELD_BLUR);
	
	    // Blur
	    if (boxBlur(chf, 1, src, dst) != src){
		    rcSwap(ref src,ref dst);
        }
	
	    // Store distance.
	    chf.dist = src;
	
	    ctx.stopTimer(rcTimerLabel.RC_TIMER_BUILD_DISTANCEFIELD_BLUR);

	    ctx.stopTimer(rcTimerLabel.RC_TIMER_BUILD_DISTANCEFIELD);
	
	    //rcFree(dst);
        dst = null;
	
	    return true;
    }

    public static void paintRectRegion(int minx, int maxx, int miny, int maxy, ushort regId,
							    rcCompactHeightfield chf, ushort[] srcReg)
    {
	    int w = chf.width;	
	    for (int y = miny; y < maxy; ++y)
	    {
		    for (int x = minx; x < maxx; ++x)
		    {
			    rcCompactCell c = chf.cells[x+y*w];
			    for (int i = (int)c.index, ni = (int)(c.index+c.count); i < ni; ++i)
			    {
				    if (chf.areas[i] != RC_NULL_AREA)
					    srcReg[i] = regId;
			    }
		    }
	    }
    }


    const ushort RC_NULL_NEI = 0xffff;

    public class rcSweepSpan
    {
	    public ushort rid = 0;	// row id
	    public ushort id = 0;	// region id
	    public ushort ns = 0;	// number samples
	    public ushort nei = 0;	// neighbour id
    };

    /// @par
    /// 
    /// Non-null regions will consist of connected, non-overlapping walkable spans that form a single contour.
    /// Contours will form simple polygons.
    /// 
    /// If multiple regions form an area that is smaller than @p minRegionArea, then all spans will be
    /// re-assigned to the zero (null) region.
    /// 
    /// Partitioning can result in smaller than necessary regions. @p mergeRegionArea helps 
    /// reduce unecessarily small regions.
    /// 
    /// See the #rcConfig documentation for more information on the configuration parameters.
    /// 
    /// The region data will be available via the rcCompactHeightfield::maxRegions
    /// and rcCompactSpan::reg fields.
    /// 
    /// @warning The distance field must be created using #rcBuildDistanceField before attempting to build regions.
    /// 
    /// @see rcCompactHeightfield, rcCompactSpan, rcBuildDistanceField, rcBuildRegionsMonotone, rcConfig
    public static bool rcBuildRegionsMonotone(rcContext ctx, rcCompactHeightfield chf,
							    int borderSize, int minRegionArea, int mergeRegionArea)
    {
	    Debug.Assert(ctx != null, "rcContext is null");
	
	    ctx.startTimer(rcTimerLabel.RC_TIMER_BUILD_REGIONS);
	
	    int w = chf.width;
	    int h = chf.height;
	    ushort id = 1;
	
        ushort[] srcReg = new ushort[chf.spanCount];
	    if (srcReg == null)
	    {
		    ctx.log(rcLogCategory.RC_LOG_ERROR, "rcBuildRegionsMonotone: Out of memory 'src' ("+chf.spanCount+").");
		    return false;
	    }
	    
	    int nsweeps = Math.Max(chf.width,chf.height);
        rcSweepSpan[] sweeps = new rcSweepSpan[nsweeps];
		rccsArrayItemsCreate(sweeps);
	    if (sweeps == null)
	    {
		    ctx.log(rcLogCategory.RC_LOG_ERROR, "rcBuildRegionsMonotone: Out of memory 'sweeps' ("+nsweeps+").");
		    return false;
	    }
	
	    // Mark border regions.
	    if (borderSize > 0)
	    {
		    // Make sure border will not overflow.
		    int bw = Math.Min(w, borderSize);
		    int bh = Math.Min(h, borderSize);
		    // Paint regions
		    paintRectRegion(0, bw, 0, h, (ushort)(id|RC_BORDER_REG), chf, srcReg); id++;
		    paintRectRegion(w-bw, w, 0, h, (ushort)(id|RC_BORDER_REG), chf, srcReg); id++;
		    paintRectRegion(0, w, 0, bh, (ushort)(id|RC_BORDER_REG), chf, srcReg); id++;
		    paintRectRegion(0, w, h-bh, h, (ushort)(id|RC_BORDER_REG), chf, srcReg); id++;
		
		    chf.borderSize = borderSize;
	    }
	
	    List<int> prev = new List<int>();//256
		prev.Capacity = 256;
	    // Sweep one line at a time.
	    for (int y = borderSize; y < h-borderSize; ++y)
	    {
		    // Collect spans from this row.
            rccsResizeList(prev, id+1);
            for (int i=0;i<id;++i){
                prev[i] = 0;
            }
		    ushort rid = 1;
		
		    for (int x = borderSize; x < w-borderSize; ++x)
		    {
			    rcCompactCell c = chf.cells[x+y*w];
			
			    for (int i = (int)c.index, ni = (int)(c.index+c.count); i < ni; ++i)
			    {
				    rcCompactSpan s = chf.spans[i];
				    if (chf.areas[i] == RC_NULL_AREA) continue;
				
				    // -x
				    ushort previd = 0;
				    if (rcGetCon(s, 0) != RC_NOT_CONNECTED)
				    {
					    int ax = x + rcGetDirOffsetX(0);
					    int ay = y + rcGetDirOffsetY(0);
					    int ai = (int)chf.cells[ax+ay*w].index + rcGetCon(s, 0);
					    if ((srcReg[ai] & RC_BORDER_REG) == 0 && chf.areas[i] == chf.areas[ai])
						    previd = srcReg[ai];
				    }
				
				    if (previd == 0)
				    {
					    previd = rid++;
					    sweeps[previd].rid = previd;
					    sweeps[previd].ns = 0;
					    sweeps[previd].nei = 0;
				    }

				    // -y
				    if (rcGetCon(s,3) != RC_NOT_CONNECTED)
				    {
					    int ax = x + rcGetDirOffsetX(3);
					    int ay = y + rcGetDirOffsetY(3);
					    int ai = (int)chf.cells[ax+ay*w].index + rcGetCon(s, 3);
					    if (srcReg[ai] != 0 && (srcReg[ai] & RC_BORDER_REG) == 0 && chf.areas[i] == chf.areas[ai])
					    {
						    ushort nr = srcReg[ai];
						    if (sweeps[previd].nei == 0 || sweeps[previd].nei == nr)
						    {
							    sweeps[previd].nei = nr;
							    sweeps[previd].ns++;
							    prev[nr]++;
						    }
						    else
						    {
							    sweeps[previd].nei = RC_NULL_NEI;
						    }
					    }
				    }

				    srcReg[i] = previd;
			    }
		    }
		
		    // Create unique ID.
		    for (int i = 1; i < rid; ++i)
		    {
			    if (sweeps[i].nei != RC_NULL_NEI && sweeps[i].nei != 0 &&
				    prev[sweeps[i].nei] == (int)sweeps[i].ns)
			    {
				    sweeps[i].id = sweeps[i].nei;
			    }
			    else
			    {
				    sweeps[i].id = id++;
			    }
		    }
		
		    // Remap IDs
		    for (int x = borderSize; x < w-borderSize; ++x)
		    {
			    rcCompactCell c = chf.cells[x+y*w];
			
			    for (int i = (int)c.index, ni = (int)(c.index+c.count); i < ni; ++i)
			    {
				    if (srcReg[i] > 0 && srcReg[i] < rid)
					    srcReg[i] = sweeps[srcReg[i]].id;
			    }
		    }
	    }

	    ctx.startTimer(rcTimerLabel.RC_TIMER_BUILD_REGIONS_FILTER);

	    // Filter out small regions.
	    chf.maxRegions = id;
	    if (!filterSmallRegions(ctx, minRegionArea, mergeRegionArea, ref chf.maxRegions, chf, srcReg))
		    return false;

	    ctx.stopTimer(rcTimerLabel.RC_TIMER_BUILD_REGIONS_FILTER);
	
	    // Store the result out.
	    for (int i = 0; i < chf.spanCount; ++i)
		    chf.spans[i].reg = srcReg[i];
	
	    ctx.stopTimer(rcTimerLabel.RC_TIMER_BUILD_REGIONS);

	    return true;
    }

    /// @par
    /// 
    /// Non-null regions will consist of connected, non-overlapping walkable spans that form a single contour.
    /// Contours will form simple polygons.
    /// 
    /// If multiple regions form an area that is smaller than @p minRegionArea, then all spans will be
    /// re-assigned to the zero (null) region.
    /// 
    /// Watershed partitioning can result in smaller than necessary regions, especially in diagonal corridors. 
    /// @p mergeRegionArea helps reduce unecessarily small regions.
    /// 
    /// See the #rcConfig documentation for more information on the configuration parameters.
    /// 
    /// The region data will be available via the rcCompactHeightfield::maxRegions
    /// and rcCompactSpan::reg fields.
    /// 
    /// @warning The distance field must be created using #rcBuildDistanceField before attempting to build regions.
    /// 
    /// @see rcCompactHeightfield, rcCompactSpan, rcBuildDistanceField, rcBuildRegionsMonotone, rcConfig
    public static bool rcBuildRegions(rcContext ctx, rcCompactHeightfield chf,
					    int borderSize, int minRegionArea, int mergeRegionArea)
    {
	    Debug.Assert(ctx != null, "rcContext is null");
	
	    ctx.startTimer(rcTimerLabel.RC_TIMER_BUILD_REGIONS);
	
	    int w = chf.width;
	    int h = chf.height;
	
	    //rcScopedDelete<ushort> buf = (ushort*)rcAlloc(sizeof(ushort)*chf.spanCount*4, RC_ALLOC_TEMP);
        /*
        ushort[] buf = new ushort[chf.spanCount*4];
	    if (buf == null)
	    {
		    ctx.log(rcLogCategory.RC_LOG_ERROR, "rcBuildRegions: Out of memory 'tmp' ("+chf.spanCount*4+").");
		    return false;
	    }
	*/
	    ctx.startTimer(rcTimerLabel.RC_TIMER_BUILD_REGIONS_WATERSHED);

	    const int LOG_NB_STACKS = 3;
	    const int NB_STACKS = 1 << LOG_NB_STACKS;
	    List<int>[] lvlStacks = new List<int>[NB_STACKS];
        for (int i = 0; i < NB_STACKS; ++i) {
            lvlStacks[i] = new List<int>();
            //rccsResizeList(lvlStacks[i], 1024);
			lvlStacks[i].Capacity = 1024;
        }

	    List<int> stack = new List<int>();//(1024);
        List<int> visited = new List<int>();//(1024);
		stack.Capacity = 1024;
		visited.Capacity = 1024;
        //rccResizeList(stack, 1024);
        //rccResizeList(visited, 1024);
	
	    ushort[] srcReg = new ushort[chf.spanCount];
	    ushort[] srcDist = new ushort[chf.spanCount];//buf+chf.spanCount;
	    ushort[] dstReg = new ushort[chf.spanCount];// buf+chf.spanCount*2;
        ushort[] dstDist = new ushort[chf.spanCount];//buf+chf.spanCount*3;
	
	    //memset(srcReg, 0, sizeof(ushort)*chf.spanCount);
	    //memset(srcDist, 0, sizeof(ushort)*chf.spanCount);
        
	    ushort regionId = 1;
	    ushort level = (ushort)((chf.maxDistance+1) & ~1);

	    // TODO: Figure better formula, expandIters defines how much the 
	    // watershed "overflows" and simplifies the regions. Tying it to
	    // agent radius was usually good indication how greedy it could be.
    //	const int expandIters = 4 + walkableRadius * 2;
	    const int expandIters = 8;

	    if (borderSize > 0)
	    {
		    // Make sure border will not overflow.
		    int bw = Math.Min(w, borderSize);
		    int bh = Math.Min(h, borderSize);
		    // Paint regions
		    paintRectRegion(0, bw, 0, h,(ushort)( regionId|RC_BORDER_REG ), chf, srcReg); regionId++;
            paintRectRegion(w - bw, w, 0, h, (ushort)(regionId | RC_BORDER_REG), chf, srcReg); regionId++;
            paintRectRegion(0, w, 0, bh, (ushort)(regionId | RC_BORDER_REG), chf, srcReg); regionId++;
            paintRectRegion(0, w, h - bh, h, (ushort)(regionId | RC_BORDER_REG), chf, srcReg); regionId++;

		    chf.borderSize = borderSize;
	    }
	
	    int sId = -1;
	    while (level > 0)
	    {
		    level = (ushort)(level >= 2 ? level-2 : 0);
		    sId = (sId+1) & (NB_STACKS-1);

    //		ctx.startTimer(rcTimerLabel.RC_TIMER_DIVIDE_TO_LEVELS);

		    if (sId == 0)
			    sortCellsByLevel(level, chf, srcReg, NB_STACKS, lvlStacks, 1);
		    else 
			    appendStacks(lvlStacks[sId-1], lvlStacks[sId], srcReg); // copy left overs from last level

    //		ctx.stopTimer(rcTimerLabel.RC_TIMER_DIVIDE_TO_LEVELS);

		    ctx.startTimer(rcTimerLabel.RC_TIMER_BUILD_REGIONS_EXPAND);
		
		    // Expand current regions until no empty connected cells found.
		    if (expandRegions(expandIters, level, chf, srcReg, srcDist, dstReg, dstDist, lvlStacks[sId], false) != srcReg)
		    {
			    rcSwap(ref srcReg,ref dstReg);
			    rcSwap(ref srcDist,ref dstDist);
		    }
		
		    ctx.stopTimer(rcTimerLabel.RC_TIMER_BUILD_REGIONS_EXPAND);
		
		    ctx.startTimer(rcTimerLabel.RC_TIMER_BUILD_REGIONS_FLOOD);
		
		    // Mark new regions with IDs.
		    for (int j=0; j<lvlStacks[sId].Count; j+=3)
		    {
			    int x = lvlStacks[sId][j];
			    int y = lvlStacks[sId][j+1];
			    int i = lvlStacks[sId][j+2];
			    if (i >= 0 && srcReg[i] == 0)
			    {
				    if (floodRegion(x, y, i, level, regionId, chf, srcReg, srcDist, stack))
					    regionId++;
			    }
		    }
		
		    ctx.stopTimer(rcTimerLabel.RC_TIMER_BUILD_REGIONS_FLOOD);
	    }
	
	    // Expand current regions until no empty connected cells found.
	    if (expandRegions(expandIters*8, 0, chf, srcReg, srcDist, dstReg, dstDist, stack, true) != srcReg)
	    {
		    rcSwap(ref srcReg,ref dstReg);
		    rcSwap(ref srcDist,ref dstDist);
	    }
	
	    ctx.stopTimer(rcTimerLabel.RC_TIMER_BUILD_REGIONS_WATERSHED);
	
	    ctx.startTimer(rcTimerLabel.RC_TIMER_BUILD_REGIONS_FILTER);
	
	    // Filter out small regions.
	    chf.maxRegions = regionId;
	    if (!filterSmallRegions(ctx, minRegionArea, mergeRegionArea, ref chf.maxRegions, chf, srcReg))
		    return false;
	
	    ctx.stopTimer(rcTimerLabel.RC_TIMER_BUILD_REGIONS_FILTER);
		
	    // Write the result out.
	    for (int i = 0; i < chf.spanCount; ++i)
		    chf.spans[i].reg = srcReg[i];
	
	    ctx.stopTimer(rcTimerLabel.RC_TIMER_BUILD_REGIONS);
	
	    return true;
    }
}