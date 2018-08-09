using System;
using System.Diagnostics;

public static partial class Recast{

    const int RC_MAX_LAYERS = RC_NOT_CONNECTED;
    const int RC_MAX_NEIS = 16;

    public class rcLayerRegion
    {
	    public byte[] layers = new byte[RC_MAX_LAYERS];
        public byte[] neis = new byte[RC_MAX_NEIS];
        public ushort ymin;
        public ushort ymax;
        public byte layerId;		// Layer ID
        public byte nlayers;		// Layer count
        public byte nneis;		// Neighbour count
        public byte baseFlag;			// Flag indicating if the region is hte base of merged regions.
    };


    public static void addUnique(byte[] a,ref byte an, byte v)
    {
	    int n = (int)an;
	    for (int i = 0; i < n; ++i){
		    if (a[i] == v){
			    return;
            }
        }
	    a[an] = v;
	    an++;
    }

    public static bool contains(byte[] a, byte an, byte v)
    {
	    int n = (int)an;
	    for (int i = 0; i < n; ++i){
		    if (a[i] == v){
			    return true;
            }
        }
	    return false;
    }

    public static bool overlapRange( ushort amin, ushort amax,
						     ushort bmin, ushort bmax)
    {
	    return (amin > bmax || amax < bmin) ? false : true;
    }



    public class rcLayerSweepSpan
    {
	    public ushort ns;	// number samples
	    public byte id;	// region id
	    public byte nei;	// neighbour id
    };

    /// @par
    /// 
    /// See the #rcConfig documentation for more information on the configuration parameters.
    /// 
    /// @see rcAllocHeightfieldLayerSet, rcCompactHeightfield, rcHeightfieldLayerSet, rcConfig
    public static bool rcBuildHeightfieldLayers(rcContext ctx, rcCompactHeightfield chf,
							      int borderSize, int walkableHeight,
							      rcHeightfieldLayerSet lset)
    {
	    Debug.Assert(ctx != null, "rcContext is null");
	
	    ctx.startTimer(rcTimerLabel.RC_TIMER_BUILD_LAYERS);
	
	    int w = chf.width;
	    int h = chf.height;
	
	    //rcScopedDelete<byte> srcReg = (byte*)rcAlloc(sizeof(byte)*chf.spanCount, RC_ALLOC_TEMP);
        byte[] srcReg = new byte[chf.spanCount];
	    if (srcReg == null)
	    {
		    ctx.log(rcLogCategory.RC_LOG_ERROR, "rcBuildHeightfieldLayers: Out of memory 'srcReg' " + chf.spanCount);
		    return false;
	    }
	    //memset(srcReg,0xff,sizeof(byte)*chf.spanCount);
        for (int i=0;i<chf.spanCount;++i){
            srcReg[i] = 0xff;
        }
	
	    int nsweeps = chf.width;
	    //rcScopedDelete<rcLayerSweepSpan> sweeps = (rcLayerSweepSpan*)rcAlloc(sizeof(rcLayerSweepSpan)*nsweeps, RC_ALLOC_TEMP);
        rcLayerSweepSpan[] sweeps = new rcLayerSweepSpan[nsweeps];
	    if (sweeps == null)
	    {
		    ctx.log(rcLogCategory.RC_LOG_ERROR, "rcBuildHeightfieldLayers: Out of memory 'sweeps' " + nsweeps);
		    return false;
	    }
	
	
	    // Partition walkable area into monotone regions.
	    int[] prevCount = new int[256];
	    byte regId = 0;

	    for (int y = borderSize; y < h-borderSize; ++y)
	    {
            //memset to 0 is done by C# alloc
		    //memset(prevCount,0,sizeof(int)*regId);
            
		    byte sweepId = 0;
		
		    for (int x = borderSize; x < w-borderSize; ++x)
		    {
			    rcCompactCell c = chf.cells[x+y*w];
			
			    for (int i = (int)c.index, ni = (int)(c.index+c.count); i < ni; ++i)
			    {
				    rcCompactSpan s = chf.spans[i];
				    if (chf.areas[i] == RC_NULL_AREA) continue;

				    byte sid = 0xff;

				    // -x
				    if (rcGetCon(s, 0) != RC_NOT_CONNECTED)
				    {
					    int ax = x + rcGetDirOffsetX(0);
					    int ay = y + rcGetDirOffsetY(0);
					    int ai = (int)chf.cells[ax+ay*w].index + rcGetCon(s, 0);
					    if (chf.areas[ai] != RC_NULL_AREA && srcReg[ai] != 0xff)
						    sid = srcReg[ai];
				    }
				
				    if (sid == 0xff)
				    {
					    sid = sweepId++;
					    sweeps[sid].nei = (byte)0xff;
					    sweeps[sid].ns = 0;
				    }
				
				    // -y
				    if (rcGetCon(s,3) != RC_NOT_CONNECTED)
				    {
					    int ax = x + rcGetDirOffsetX(3);
					    int ay = y + rcGetDirOffsetY(3);
					    int ai = (int)chf.cells[ax+ay*w].index + rcGetCon(s, 3);
					    byte nr = srcReg[ai];
					    if (nr != 0xff)
					    {
						    // Set neighbour when first valid neighbour is encoutered.
						    if (sweeps[sid].ns == 0)
							    sweeps[sid].nei = nr;
						
						    if (sweeps[sid].nei == nr)
						    {
							    // Update existing neighbour
							    sweeps[sid].ns++;
							    prevCount[nr]++;
						    }
						    else
						    {
							    // This is hit if there is nore than one neighbour.
							    // Invalidate the neighbour.
							    sweeps[sid].nei = 0xff;
						    }
					    }
				    }
				
				    srcReg[i] = sid;
			    }
		    }
		
		    // Create unique ID.
		    for (int i = 0; i < sweepId; ++i)
		    {
			    // If the neighbour is set and there is only one continuous connection to it,
			    // the sweep will be merged with the previous one, else new region is created.
			    if (sweeps[i].nei != 0xff && prevCount[sweeps[i].nei] == (int)sweeps[i].ns)
			    {
				    sweeps[i].id = sweeps[i].nei;
			    }
			    else
			    {
				    if (regId == 255)
				    {
					    ctx.log(rcLogCategory.RC_LOG_ERROR, "rcBuildHeightfieldLayers: Region ID overflow.");
					    return false;
				    }
				    sweeps[i].id = regId++;
			    }
		    }
		
		    // Remap local sweep ids to region ids.
		    for (int x = borderSize; x < w-borderSize; ++x)
		    {
			    rcCompactCell c = chf.cells[x+y*w];
			    for (int i = (int)c.index, ni = (int)(c.index+c.count); i < ni; ++i)
			    {
				    if (srcReg[i] != 0xff)
					    srcReg[i] = sweeps[srcReg[i]].id;
			    }
		    }
	    }

	    // Allocate and init layer regions.
	    int nregs = (int)regId;
	    //rcScopedDelete<rcLayerRegion> regs = (rcLayerRegion*)rcAlloc(sizeof(rcLayerRegion)*nregs, RC_ALLOC_TEMP);
        rcLayerRegion[] regs = new rcLayerRegion[nregs];
	    if (regs == null)
	    {
		    ctx.log(rcLogCategory.RC_LOG_ERROR, "rcBuildHeightfieldLayers: Out of memory 'regs' " + nregs);
		    return false;
	    }
	    //memset(regs, 0, sizeof(rcLayerRegion)*nregs);
	    for (int i = 0; i < nregs; ++i)
	    {
		    regs[i].layerId = 0xff;
		    regs[i].ymin = 0xffff;
		    regs[i].ymax = 0;
	    }
	
	    // Find region neighbours and overlapping regions.
	    for (int y = 0; y < h; ++y)
	    {
		    for (int x = 0; x < w; ++x)
		    {
			    rcCompactCell c = chf.cells[x+y*w];
			
			    byte[] lregs = new byte[RC_MAX_LAYERS];
			    int nlregs = 0;
			
			    for (int i = (int)c.index, ni = (int)(c.index+c.count); i < ni; ++i)
			    {
				    rcCompactSpan s = chf.spans[i];
				    byte ri = srcReg[i];
				    if (ri == 0xff){
                        continue;
                    }
				
				    regs[ri].ymin = Math.Min(regs[ri].ymin, s.y);
				    regs[ri].ymax = Math.Max(regs[ri].ymax, s.y);
				
				    // Collect all region layers.
				    if (nlregs < RC_MAX_LAYERS)
					    lregs[nlregs++] = ri;
				
				    // Update neighbours
				    for (int dir = 0; dir < 4; ++dir)
				    {
					    if (rcGetCon(s, dir) != RC_NOT_CONNECTED)
					    {
						    int ax = x + rcGetDirOffsetX(dir);
						    int ay = y + rcGetDirOffsetY(dir);
						    int ai = (int)chf.cells[ax+ay*w].index + rcGetCon(s, dir);
						    byte rai = srcReg[ai];
						    if (rai != 0xff && rai != ri){
							    addUnique(regs[ri].neis,ref regs[ri].nneis, rai);
                            }
					    }
				    }
				
			    }
			
			    // Update overlapping regions.
			    for (int i = 0; i < nlregs-1; ++i)
			    {
				    for (int j = i+1; j < nlregs; ++j)
				    {
					    if (lregs[i] != lregs[j])
					    {
						    rcLayerRegion ri = regs[lregs[i]];
						    rcLayerRegion rj = regs[lregs[j]];
						    addUnique(ri.layers,ref ri.nlayers, lregs[j]);
						    addUnique(rj.layers,ref rj.nlayers, lregs[i]);
					    }
				    }
			    }
			
		    }
	    }
	
	    // Create 2D layers from regions.
	    byte layerId = 0;
	
	    const int MAX_STACK = 64;
	    byte[] stack = new byte[MAX_STACK];
	    int nstack = 0;
	
	    for (int i = 0; i < nregs; ++i)
	    {
		    rcLayerRegion root = regs[i];
		    // Skip alreadu visited.
		    if (root.layerId != 0xff){
			    continue;
            }

		    // Start search.
		    root.layerId = layerId;
		    root.baseFlag = 1;
		
		    nstack = 0;
		    stack[nstack++] = (byte)i;
		
		    while (nstack != 0)
		    {
			    // Pop front
			    rcLayerRegion reg = regs[stack[0]];
			    nstack--;
			    for (int j = 0; j < nstack; ++j){
				    stack[j] = stack[j+1];
                }
			
			    int nneis = (int)reg.nneis;
			    for (int j = 0; j < nneis; ++j)
			    {
				    byte nei = reg.neis[j];
				    rcLayerRegion regn = regs[nei];
				    // Skip already visited.
				    if (regn.layerId != 0xff){
					    continue;
                    }
				    // Skip if the neighbour is overlapping root region.
				    if (contains(root.layers, root.nlayers, nei)){
					    continue;
                    }
				    // Skip if the height range would become too large.
				    int ymin = Math.Min(root.ymin, regn.ymin);
				    int ymax = Math.Max(root.ymax, regn.ymax);
				    if ((ymax - ymin) >= 255){
					     continue;
                    }

				    if (nstack < MAX_STACK)
				    {
					    // Deepen
					    stack[nstack++] = (byte)nei;
					
					    // Mark layer id
					    regn.layerId = layerId;
					    // Merge current layers to root.
					    for (int k = 0; k < regn.nlayers; ++k){
						    addUnique(root.layers,ref root.nlayers, regn.layers[k]);
                        }
					    root.ymin = Math.Min(root.ymin, regn.ymin);
					    root.ymax = Math.Max(root.ymax, regn.ymax);
				    }
			    }
		    }
		
		    layerId++;
	    }
	
	    // Merge non-overlapping regions that are close in height.
	    ushort mergeHeight = (ushort)(walkableHeight * 4);
	
	    for (int i = 0; i < nregs; ++i)
	    {
		    rcLayerRegion ri = regs[i];
		    if (ri.baseFlag == 0){
                continue;
            }
		
		    byte newId = ri.layerId;
		
		    for (;;)
		    {
			    byte oldId = 0xff;
			
			    for (int j = 0; j < nregs; ++j)
			    {
				    if (i == j){
                        continue;
                    }
				    rcLayerRegion rj = regs[j];
				    if (rj.baseFlag == 0){
                        continue;
                    }
				
				    // Skip if teh regions are not close to each other.
				    if (!overlapRange(ri.ymin,
                        (ushort)(ri.ymax + mergeHeight),
                        rj.ymin,
                        (ushort)(rj.ymax + mergeHeight))){
					    continue;
                    }
				    // Skip if the height range would become too large.
				    int ymin = Math.Min(ri.ymin, rj.ymin);
				    int ymax = Math.Max(ri.ymax, rj.ymax);
				    if ((ymax - ymin) >= 255){
				      continue;
                    }
						  
				    // Make sure that there is no overlap when mergin 'ri' and 'rj'.
				    bool overlap = false;
				    // Iterate over all regions which have the same layerId as 'rj'
				    for (int k = 0; k < nregs; ++k)
				    {
					    if (regs[k].layerId != rj.layerId)
						    continue;
					    // Check if region 'k' is overlapping region 'ri'
					    // Index to 'regs' is the same as region id.
					    if (contains(ri.layers,ri.nlayers, (byte)k))
					    {
						    overlap = true;
						    break;
					    }
				    }
				    // Cannot merge of regions overlap.
				    if (overlap)
					    continue;
				
				    // Can merge i and j.
				    oldId = rj.layerId;
				    break;
			    }
			
			    // Could not find anything to merge with, stop.
			    if (oldId == 0xff)
				    break;
			
			    // Merge
			    for (int j = 0; j < nregs; ++j)
			    {
				    rcLayerRegion rj = regs[j];
				    if (rj.layerId == oldId)
				    {
					    rj.baseFlag = 0;
					    // Remap layerIds.
					    rj.layerId = newId;
					    // Add overlaid layers from 'rj' to 'ri'.
					    for (int k = 0; k < rj.nlayers; ++k){
						    addUnique(ri.layers,ref ri.nlayers, rj.layers[k]);
                        }
					    // Update heigh bounds.
					    ri.ymin = Math.Min(ri.ymin, rj.ymin);
					    ri.ymax = Math.Max(ri.ymax, rj.ymax);
				    }
			    }
		    }
	    }
	
	    // Compact layerIds
	    byte[] remap = new byte[256];
	    //memset(remap, 0, 256);

	    // Find number of unique layers.
	    layerId = 0;
	    for (int i = 0; i < nregs; ++i){
		    remap[regs[i].layerId] = 1;
        }
	    for (int i = 0; i < 256; ++i)
	    {
		    if (remap[i] != 0){
			    remap[i] = layerId++;
		    }
            else{
			    remap[i] = 0xff;
            }
	    }
	    // Remap ids.
	    for (int i = 0; i < nregs; ++i){
		    regs[i].layerId = remap[regs[i].layerId];
        }
	
	    // No layers, return empty.
	    if (layerId == 0)
	    {
		    ctx.stopTimer(rcTimerLabel.RC_TIMER_BUILD_LAYERS);
		    return true;
	    }
	
	    // Create layers.
	    Debug.Assert(lset.layers == null,"Assert lset.layers == 0");
	
	    int lw = w - borderSize*2;
	    int lh = h - borderSize*2;

	    // Build contracted bbox for layers.
	    float[] bmin = new float[3];
        float[] bmax = new float[3];
	    rcVcopy(bmin, chf.bmin);
	    rcVcopy(bmax, chf.bmax);
	    bmin[0] += borderSize*chf.cs;
	    bmin[2] += borderSize*chf.cs;
	    bmax[0] -= borderSize*chf.cs;
	    bmax[2] -= borderSize*chf.cs;
	
	    lset.nlayers = (int)layerId;
	
	    //lset.layers = (rcHeightfieldLayer*)rcAlloc(sizeof(rcHeightfieldLayer)*lset.nlayers, RC_ALLOC_PERM);
        lset.layers = new rcHeightfieldLayer[lset.nlayers];
	    if (lset.layers == null)
	    {
		    ctx.log(rcLogCategory.RC_LOG_ERROR, "rcBuildHeightfieldLayers: Out of memory 'layers' " + lset.nlayers);
		    return false;
	    }
	    //memset(lset.layers, 0, sizeof(rcHeightfieldLayer)*lset.nlayers);

	
	    // Store layers.
	    for (int i = 0; i < lset.nlayers; ++i)
	    {
		    byte curId = (byte)i;
		
		    // Allocate memory for the current layer.
		    rcHeightfieldLayer layer = lset.layers[i];
		    //memset(layer, 0, sizeof(rcHeightfieldLayer));

		    int gridSize = sizeof(byte)*lw*lh;

		    layer.heights = new byte[gridSize];//(byte*)rcAlloc(gridSize, RC_ALLOC_PERM);
		    if (layer.heights == null)
		    {
			    ctx.log(rcLogCategory.RC_LOG_ERROR, "rcBuildHeightfieldLayers: Out of memory 'heights' " + gridSize);
			    return false;
		    }
		    //memset(layer.heights, 0xff, gridSize);
            for (int j=0;j<gridSize;++j){
                layer.heights[j] = 0xFF;
            }

		    layer.areas = new byte[gridSize];// (byte*)rcAlloc(gridSize, RC_ALLOC_PERM);
		    if (layer.areas == null)
		    {
			    ctx.log(rcLogCategory.RC_LOG_ERROR, "rcBuildHeightfieldLayers: Out of memory 'areas' " + gridSize);
			    return false;
		    }
		    //memset(layer.areas, 0, gridSize);

		    layer.cons =new byte[gridSize];//  (byte*)rcAlloc(gridSize, RC_ALLOC_PERM);
		    if (layer.cons == null)
		    {
			    ctx.log(rcLogCategory.RC_LOG_ERROR, "rcBuildHeightfieldLayers: Out of memory 'cons' " + gridSize);
			    return false;
		    }
		    //memset(layer.cons, 0, gridSize);
		
		    // Find layer height bounds.
		    int hmin = 0, hmax = 0;
		    for (int j = 0; j < nregs; ++j)
		    {
			    if (regs[j].baseFlag != 0 && regs[j].layerId == curId)
			    {
				    hmin = (int)regs[j].ymin;
				    hmax = (int)regs[j].ymax;
			    }
		    }

		    layer.width = lw;
		    layer.height = lh;
		    layer.cs = chf.cs;
		    layer.ch = chf.ch;
		
		    // Adjust the bbox to fit the heighfield.
		    rcVcopy(layer.bmin, bmin);
		    rcVcopy(layer.bmax, bmax);
		    layer.bmin[1] = bmin[1] + hmin*chf.ch;
		    layer.bmax[1] = bmin[1] + hmax*chf.ch;
		    layer.hmin = hmin;
		    layer.hmax = hmax;

		    // Update usable data region.
		    layer.minx = layer.width;
		    layer.maxx = 0;
		    layer.miny = layer.height;
		    layer.maxy = 0;
		
		    // Copy height and area from compact heighfield. 
		    for (int y = 0; y < lh; ++y)
		    {
			    for (int x = 0; x < lw; ++x)
			    {
				    int cx = borderSize+x;
				    int cy = borderSize+y;
				    rcCompactCell c = chf.cells[cx+cy*w];
				    for (int j = (int)c.index, nj = (int)(c.index+c.count); j < nj; ++j)
				    {
					    rcCompactSpan s = chf.spans[j];
					    // Skip unassigned regions.
					    if (srcReg[j] == 0xff){
						    continue;
                        }
					    // Skip of does nto belong to current layer.
					    byte lid = regs[srcReg[j]].layerId;
					    if (lid != curId)
						    continue;
					
					    // Update data bounds.
					    layer.minx = Math.Min(layer.minx, x);
					    layer.maxx = Math.Max(layer.maxx, x);
					    layer.miny = Math.Min(layer.miny, y);
					    layer.maxy = Math.Max(layer.maxy, y);
					
					    // Store height and area type.
					    int idx = x+y*lw;
					    layer.heights[idx] = (byte)(s.y - hmin);
					    layer.areas[idx] = chf.areas[j];
					
					    // Check connection.
					    byte portal = 0;
					    byte con = 0;
					    for (int dir = 0; dir < 4; ++dir)
					    {
						    if (rcGetCon(s, dir) != RC_NOT_CONNECTED)
						    {
							    int ax = cx + rcGetDirOffsetX(dir);
							    int ay = cy + rcGetDirOffsetY(dir);
							    int ai = (int)chf.cells[ax+ay*w].index + rcGetCon(s, dir);
							    byte alid = srcReg[ai] != (byte)0xff ? regs[srcReg[ai]].layerId : (byte)0xff;
							    // Portal mask
							    if (chf.areas[ai] != RC_NULL_AREA && lid != alid)
							    {
								    portal |= (byte)(1<<dir);
								    // Update height so that it matches on both sides of the portal.
								    rcCompactSpan aSpan = chf.spans[ai];
								    if (aSpan.y > hmin)
                                        layer.heights[idx] = Math.Max(layer.heights[idx], (byte)(aSpan.y - hmin));
							    }
							    // Valid connection mask
							    if (chf.areas[ai] != RC_NULL_AREA && lid == alid)
							    {
								    int nx = ax - borderSize;
								    int ny = ay - borderSize;
								    if (nx >= 0 && ny >= 0 && nx < lw && ny < lh)
									    con |= (byte)(1<<dir);
							    }
						    }
					    }
					
					    layer.cons[idx] = (byte)( (portal << 4) | con );
				    }
			    }
		    }
		
		    if (layer.minx > layer.maxx)
			    layer.minx = layer.maxx = 0;
		    if (layer.miny > layer.maxy)
			    layer.miny = layer.maxy = 0;
	    }
	
	    ctx.stopTimer(rcTimerLabel.RC_TIMER_BUILD_LAYERS);
	
	    return true;
    }
}