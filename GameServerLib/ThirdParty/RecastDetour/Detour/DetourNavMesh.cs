using System;
using System.Diagnostics;

using dtStatus = System.UInt32;

/**
    @typedef dtPolyRef
    @par

    Polygon references are subject to the same invalidate/preserve/restore 
    rules that apply to #dtTileRef's.  If the #dtTileRef for the polygon's
    tile changes, the polygon reference becomes invalid.

    Changing a polygon's flags, area id, etc. does not impact its polygon
    reference.

    @typedef dtTileRef
    @par

    The following changes will invalidate a tile reference:

    - The referenced tile has been removed from the navigation mesh.
    - The navigation mesh has been initialized using a different set
      of #dtNavMeshParams.

    A tile reference is preserved/restored if the tile is added to a navigation 
    mesh initialized with the original #dtNavMeshParams and is added at the
    original reference location. (E.g. The lastRef parameter is used with
    dtNavMesh::addTile.)

    Basically, if the storage structure of a tile changes, its associated
    tile reference changes.
  */

#if DT_POLYREF64
using dtPolyRef = System.UInt64;
using dtTileRef = System.UInt64;
#else
using dtPolyRef = System.UInt32;
using dtTileRef = System.UInt32;
#endif

#if DT_POLYREF64
public static partial class Detour{
    static const uint DT_SALT_BITS = 16;
    static const uint DT_TILE_BITS = 28;
    static const uint DT_POLY_BITS = 20;
}
#endif

public static partial class Detour{
    public static bool overlapSlabs(float[] amin, float[] amax,
						        float[] bmin,float[] bmax,
						        float px, float py)
    {
	    // Check for horizontal overlap.
	    // The segment is shrunken a little so that slabs which touch
	    // at end points are not connected.
	    float minx = (float)Math.Max(amin[0]+px,bmin[0]+px);
	    float maxx = (float)Math.Min(amax[0]-px,bmax[0]-px);
	    if (minx > maxx)
		    return false;
	
	    // Check vertical overlap.
	    float ad = (amax[1]-amin[1]) / (amax[0]-amin[0]);
	    float ak = amin[1] - ad*amin[0];
	    float bd = (bmax[1]-bmin[1]) / (bmax[0]-bmin[0]);
	    float bk = bmin[1] - bd*bmin[0];
	    float aminy = ad*minx + ak;
	    float amaxy = ad*maxx + ak;
	    float bminy = bd*minx + bk;
	    float bmaxy = bd*maxx + bk;
	    float dmin = bminy - aminy;
	    float dmax = bmaxy - amaxy;
		
	    // Crossing segments always overlap.
	    if (dmin*dmax < 0)
		    return true;
		
	    // Check for overlap at endpoints.
	    float thr = dtSqr(py*2);
	    if (dmin*dmin <= thr || dmax*dmax <= thr)
		    return true;
		
	    return false;
    }

    public static float getSlabCoord(float[] va, int side)
    {
	    if (side == 0 || side == 4)
		    return va[0];
	    else if (side == 2 || side == 6)
		    return va[2];
	    return 0;
    }
    public static float getSlabCoord(float[] va, int vaStart, int side)
    {
	    if (side == 0 || side == 4)
		    return va[vaStart+0];
	    else if (side == 2 || side == 6)
		    return va[vaStart+2];
	    return 0;
    }


    public static void calcSlabEndPoints(float[] va, int vaStart, float[] vb, int vbStart, float[] bmin, float[] bmax, int side)
    {
	    if (side == 0 || side == 4)
	    {
		    if (va[vaStart + 2] < vb[vbStart + 2])
		    {
			    bmin[0] = va[vaStart + 2];
			    bmin[1] = va[vaStart + 1];
			    bmax[0] = vb[vbStart + 2];
			    bmax[1] = vb[vbStart + 1];
		    }
		    else
		    {
			    bmin[0] = vb[vbStart + 2];
			    bmin[1] = vb[vbStart + 1];
			    bmax[0] = va[vaStart + 2];
			    bmax[1] = va[vaStart + 1];
		    }
	    }
	    else if (side == 2 || side == 6)
	    {
		    if (va[vaStart + 0] < vb[0])
		    {
			    bmin[0] = va[vaStart + 0];
			    bmin[1] = va[vaStart + 1];
			    bmax[0] = vb[vbStart + 0];
			    bmax[1] = vb[vbStart + 1];
		    }
		    else
		    {
			    bmin[0] = vb[vbStart + 0];
			    bmin[1] = vb[vbStart + 1];
			    bmax[0] = va[vaStart + 0];
			    bmax[1] = va[vaStart + 1];
		    }
	    }
    }

    public static int computeTileHash(int x, int y, int mask)
    {
	    const uint h1 = 0x8da6b343; // Large multiplicative constants;
	    const uint h2 = 0xd8163841; // here arbitrarily chosen primes
	    uint n = (uint)(h1 * x + h2 * y);
	    return (int)(n & mask);
    }

    public static uint allocLink(dtMeshTile tile)
    {
	    if (tile.linksFreeList == Detour.DT_NULL_LINK)
		    return DT_NULL_LINK;
	    uint link = tile.linksFreeList;
	    tile.linksFreeList = tile.links[link].next;
	    return link;
    }

    public static void freeLink(dtMeshTile tile, uint link)
    {
	    tile.links[link].next = tile.linksFreeList;
	    tile.linksFreeList = link;
    }

    /*
    dtNavMesh* dtAllocNavMesh()
    {
	    void* mem = dtAlloc(sizeof(dtNavMesh), DT_ALLOC_PERM);
	    if (!mem) return 0;
	    return new(mem) dtNavMesh;
    }
    */
    /// @par
    ///
    /// This function will only free the memory for tiles with the #DT_TILE_FREE_DATA
    /// flag set.
    /*
    void dtFreeNavMesh(dtNavMesh* navmesh)
    {
	    if (!navmesh) return;
	    navmesh.~dtNavMesh();
	    dtFree(navmesh);
    }
    */
    //////////////////////////////////////////////////////////////////////////////////////////

    /**
    @class dtNavMesh

    The navigation mesh consists of one or more tiles defining three primary types of structural data:

    A polygon mesh which defines most of the navigation graph. (See rcPolyMesh for its structure.)
    A detail mesh used for determining surface height on the polygon mesh. (See rcPolyMeshDetail for its structure.)
    Off-mesh connections, which define custom point-to-point edges within the navigation graph.

    The general build process is as follows:

    -# Create rcPolyMesh and rcPolyMeshDetail data using the Recast build pipeline.
    -# Optionally, create off-mesh connection data.
    -# Combine the source data into a dtNavMeshCreateParams structure.
    -# Create a tile data array using dtCreateNavMeshData().
    -# Allocate at dtNavMesh object and initialize it. (For single tile navigation meshes,
        the tile data is loaded during this step.)
    -# For multi-tile navigation meshes, load the tile data using dtNavMesh::addTile().

    Notes:

    - This class is usually used in conjunction with the dtNavMeshQuery class for pathfinding.
    - Technically, all navigation meshes are tiled. A 'solo' mesh is simply a navigation mesh initialized 
        to have only a single tile.
    - This class does not implement any asynchronous methods. So the ::dtStatus result of all methods will 
        always contain either a success or failure flag.

    @see dtNavMeshQuery, dtCreateNavMeshData, dtNavMeshCreateParams, #dtAllocNavMesh, #dtFreeNavMesh
    */
    /// A navigation mesh based on tiles of convex polygons.
    /// @ingroup detour
    public class dtNavMesh{
        
        private dtNavMeshParams m_params;			//< Current initialization params. TODO: do not store this info twice.
        private float[] m_orig = new float[3];					//< Origin of the tile (0,0)
        private float m_tileWidth, m_tileHeight;	//< Dimensions of each tile.
        private int m_maxTiles;						//< Max number of tiles.
        private int m_tileLutSize;					//< Tile hash lookup size (must be pot).
        private int m_tileLutMask;					//< Tile hash lookup mask.

        //dtMeshTile**
        private dtMeshTile[] m_posLookup;			//< Tile hash lookup.
        private dtMeshTile m_nextFree;				//< Freelist of tiles.
        private dtMeshTile[] m_tiles;				//< List of tiles.

#if DT_POLYREF64
#else
        uint m_saltBits;			//< Number of salt bits in the tile ID.
        uint m_tileBits;			//< Number of tile bits in the tile ID.
        uint m_polyBits;			//< Number of poly bits in the tile ID.
#endif

        public dtNavMesh()
        {
        #if DT_POLYREF64
	        m_saltBits = 0;
	        m_tileBits = 0;
	        m_polyBits = 0;
        #endif
            m_params = new dtNavMeshParams();
	        m_orig[0] = 0;
	        m_orig[1] = 0;
	        m_orig[2] = 0;
        }

        ~dtNavMesh()
        {
            //C#: all this auto
            /*
	        for (int i = 0; i < m_maxTiles; ++i)
	        {
		        if (m_tiles[i].flags & Detour.DT_TILE_FREE_DATA)
		        {
			        //dtFree(m_tiles[i].data);
			        m_tiles[i].data = null;
			        m_tiles[i].dataSize = 0;
		        }
	        }
	        //dtFree(m_posLookup);
	        //dtFree(m_tiles);
            m_posLookup = null;
            m_tiles = null;
                * */
        }
        /// Derives a standard polygon reference.
        ///  @note This function is generally meant for internal use only.
        ///  @param[in]	salt	The tile's salt value.
        ///  @param[in]	it		The index of the tile.
        ///  @param[in]	ip		The index of the polygon within the tile.
        public dtPolyRef encodePolyId(uint salt, uint it, uint ip) {
#if DT_POLYREF64
		return ((dtPolyRef)salt << (DT_POLY_BITS+DT_TILE_BITS)) | ((dtPolyRef)it << DT_POLY_BITS) | (dtPolyRef)ip;
#else
            return ((dtPolyRef)salt << (int)(m_polyBits + m_tileBits)) | ((dtPolyRef)it << (int)m_polyBits) | (dtPolyRef)ip;
#endif
        }

        /// Decodes a standard polygon reference.
        ///  @note This function is generally meant for internal use only.
        ///  @param[in]	ref   The polygon reference to decode.
        ///  @param[out]	salt	The tile's salt value.
        ///  @param[out]	it		The index of the tile.
        ///  @param[out]	ip		The index of the polygon within the tile.
        ///  @see #encodePolyId
        public void decodePolyId(dtPolyRef polyRef, ref uint salt, ref uint it, ref uint ip) {
#if DT_POLYREF64
		const dtPolyRef saltMask = ((dtPolyRef)1<<DT_SALT_BITS)-1;
		const dtPolyRef tileMask = ((dtPolyRef)1<<DT_TILE_BITS)-1;
		const dtPolyRef polyMask = ((dtPolyRef)1<<DT_POLY_BITS)-1;
		salt = (uint)((ref >> (DT_POLY_BITS+DT_TILE_BITS)) & saltMask);
		it = (uint)((ref >> DT_POLY_BITS) & tileMask);
		ip = (uint)(ref & polyMask);
#else
            dtPolyRef saltMask = (dtPolyRef)(1 << (int)m_saltBits) - 1;
            dtPolyRef tileMask = (dtPolyRef)((1 << (int)m_tileBits) - 1);
            dtPolyRef polyMask = (dtPolyRef)((1 << (int)m_polyBits) - 1);
            salt = (uint)((polyRef >> (int)(m_polyBits + m_tileBits)) & saltMask);
            it = (uint)((polyRef >> (int)m_polyBits) & tileMask);
            ip = (uint)(polyRef & polyMask);
#endif
        }

        /// Extracts a tile's salt value from the specified polygon reference.
        ///  @note This function is generally meant for internal use only.
        ///  @param[in]	ref		The polygon reference.
        ///  @see #encodePolyId
        public uint decodePolyIdSalt(dtPolyRef polyRef) {
#if DT_POLYREF64
		const dtPolyRef saltMask = ((dtPolyRef)1<<DT_SALT_BITS)-1;
		return (uint)((ref >> (DT_POLY_BITS+DT_TILE_BITS)) & saltMask);
#else
            dtPolyRef saltMask = (dtPolyRef)((1 << (int)m_saltBits) - 1);
            return (uint)((polyRef >> (int)(m_polyBits + m_tileBits)) & saltMask);
#endif
        }

        /// Extracts the tile's index from the specified polygon reference.
        ///  @note This function is generally meant for internal use only.
        ///  @param[in]	ref		The polygon reference.
        ///  @see #encodePolyId
        public uint decodePolyIdTile(dtPolyRef polyRef) {
#if DT_POLYREF64
		const dtPolyRef tileMask = ((dtPolyRef)1<<DT_TILE_BITS)-1;
		return (uint)((ref >> DT_POLY_BITS) & tileMask);
#else
            dtPolyRef tileMask = (dtPolyRef)((1 << (int)m_tileBits) - 1);
            return (uint)((polyRef >> (int)m_polyBits) & tileMask);
#endif
        }

        /// Extracts the polygon's index (within its tile) from the specified polygon reference.
        ///  @note This function is generally meant for internal use only.
        ///  @param[in]	ref		The polygon reference.
        ///  @see #encodePolyId
        public uint decodePolyIdPoly(dtPolyRef polyRef) {
#if DT_POLYREF64
		const dtPolyRef polyMask = ((dtPolyRef)1<<DT_POLY_BITS)-1;
		return (uint)(ref & polyMask);
#else
            dtPolyRef polyMask = (dtPolyRef)((1 << (int)m_polyBits) - 1);
            return (uint)(polyRef & polyMask);
#endif
        }

        /// @{
        /// @name Initialization and Tile Management

        /// Initializes the navigation mesh for tiled use.
        ///  @param[in]	params		Initialization parameters.
        /// @return The status flags for the operation.
        public dtStatus init(dtNavMeshParams navMeshParams)
        {
	        //memcpy(&m_params, params, sizeof(dtNavMeshParams));
            m_params = navMeshParams.Clone();
	        dtVcopy(m_orig, navMeshParams.orig);
	        m_tileWidth = navMeshParams.tileWidth;
	        m_tileHeight = navMeshParams.tileHeight;
	
	        // Init tiles
	        m_maxTiles = navMeshParams.maxTiles;
	        m_tileLutSize = (int)dtNextPow2((uint)(navMeshParams.maxTiles/4));
	        if (m_tileLutSize == 0)
                m_tileLutSize = 1;
	        m_tileLutMask = m_tileLutSize-1; 
	
	        //m_tiles = (dtMeshTile*)dtAlloc(sizeof(dtMeshTile)*m_maxTiles, DT_ALLOC_PERM);
            m_tiles = new dtMeshTile[m_maxTiles];
            dtcsArrayItemsCreate(m_tiles);
        
	        if (m_tiles == null)
		        return (DT_FAILURE | DT_OUT_OF_MEMORY);
	        //m_posLookup = (dtMeshTile**)dtAlloc(sizeof(dtMeshTile*)*m_tileLutSize, DT_ALLOC_PERM);
            m_posLookup = new dtMeshTile[m_tileLutSize];
            dtcsArrayItemsCreate(m_posLookup);
	        if (m_posLookup == null)
		        return DT_FAILURE | DT_OUT_OF_MEMORY;
	        //memset(m_tiles, 0, sizeof(dtMeshTile)*m_maxTiles);
	        //memset(m_posLookup, 0, sizeof(dtMeshTile*)*m_tileLutSize);
	        m_nextFree = null;
	        for (int i = m_maxTiles-1; i >= 0; --i)
	        {
		        m_tiles[i].salt = 1;
		        m_tiles[i].next = m_nextFree;
		        m_nextFree = m_tiles[i];
	        }
	
	        // Init ID generator values.
        #if DT_POLYREF64
        #else
	        m_tileBits = (dtStatus)dtIlog2(dtNextPow2((uint)navMeshParams.maxTiles));
	        m_polyBits = (dtStatus)dtIlog2(dtNextPow2((uint)navMeshParams.maxPolys));
	        // Only allow 31 salt bits, since the salt mask is calculated using 32bit uint and it will overflow.
	        m_saltBits = Math.Min((uint)31, 32 - m_tileBits - m_polyBits);

	        if (m_saltBits < 10)
		        return DT_FAILURE | DT_INVALID_PARAM;
        #endif
	
	        return DT_SUCCESS;
        }

        /// Initializes the navigation mesh for single tile use.
        ///  @param[in]	data		Data of the new tile. (See: #dtCreateNavMeshData)
        ///  @param[in]	dataSize	The data size of the new tile.
        ///  @param[in]	flags		The tile flags. (See: #dtTileFlags)
        /// @return The status flags for the operation.
        ///  @see dtCreateNavMeshData
        public dtStatus init(dtRawTileData rawTile, int flags)
        {
            //C#: Using an intermediate class dtRawTileData because Cpp uses a binary buffer.

	        // Make sure the data is in right format.
	        //dtMeshHeader header = (dtMeshHeader*)data;
            dtMeshHeader header = rawTile.header;
	        if (header.magic != DT_NAVMESH_MAGIC)
		        return DT_FAILURE | DT_WRONG_MAGIC;
	        if (header.version != DT_NAVMESH_VERSION)
		        return DT_FAILURE | DT_WRONG_VERSION;

	        dtNavMeshParams navMeshParams = new dtNavMeshParams();
	        dtVcopy(navMeshParams.orig, header.bmin);
	        navMeshParams.tileWidth = header.bmax[0] - header.bmin[0];
	        navMeshParams.tileHeight = header.bmax[2] - header.bmin[2];
	        navMeshParams.maxTiles = 1;
	        navMeshParams.maxPolys = header.polyCount;
	
	        dtStatus status = init(navMeshParams);
	        if (dtStatusFailed(status))
		        return status;

	        //return addTile(data, dataSize, flags, 0, 0);
            dtTileRef dummyResult = 0;
            return addTile(rawTile, flags, 0,ref dummyResult);
        }

        /// The navigation mesh initialization params.
        /// @par
        ///
        /// @note The parameters are created automatically when the single tile
        /// initialization is performed.
        public dtNavMeshParams getParams() 
        {
	        return m_params;
        }

        //////////////////////////////////////////////////////////////////////////////////////////
        /// Returns all polygons in neighbour tile based on portal defined by the segment.
        public int findConnectingPolys(float[] va, int vaStart, float[] vb, int vbStart,
								           dtMeshTile tile, int side,
								           dtPolyRef[] con, float[] conarea, int maxcon)
        {
	        if (tile == null) 
                return 0;
	
	        float[] amin = new float[2];
            float[] amax = new float[2];
	        calcSlabEndPoints(va, vaStart,vb, vbStart, amin,amax, side);
	        float apos = getSlabCoord(va, vaStart, side);

	        // Remove links pointing to 'side' and compact the links array. 
	        float[] bmin = new float[2];
            float[] bmax = new float[2];
	        ushort m =(ushort)( DT_EXT_LINK | (ushort)side );
	        int n = 0;
	
	        dtPolyRef polyRefBase = getPolyRefBase(tile);
	
	        for (int i = 0; i < tile.header.polyCount; ++i)
	        {
		        dtPoly poly = tile.polys[i];
		        int nv = (int)poly.vertCount;
		        for (int j = 0; j < nv; ++j)
		        {
			        // Skip edges which do not point to the right side.
			        if (poly.neis[j] != m) 
                        continue;
			
			        //const float* vc = &tile.verts[poly.verts[j]*3];
			        //const float* vd = &tile.verts[poly.verts[(j+1) % nv]*3];
                    int vcStart = poly.verts[j]*3;
                    int vdStart = poly.verts[(j+1) % nv]*3;
			        float bpos = getSlabCoord(tile.verts, vcStart, side);
			
			        // Segments are not close enough.
			        if (Math.Abs(apos-bpos) > 0.01f)
				        continue;
			
			        // Check if the segments touch.
			        calcSlabEndPoints(tile.verts,vcStart,tile.verts,vdStart, bmin,bmax, side);
			
			        if (!overlapSlabs(amin,amax, bmin,bmax, 0.01f, tile.header.walkableClimb)) 
                        continue;
			
			        // Add return value.
			        if (n < maxcon)
			        {
				        conarea[n*2+0] = Math.Max(amin[0], bmin[0]);
				        conarea[n*2+1] = Math.Min(amax[0], bmax[0]);
				        con[n] = polyRefBase | (dtPolyRef)i;
				        n++;
			        }
			        break;
		        }
	        }
	        return n;
        }

        /// Removes external links at specified side.
        public void unconnectExtLinks(dtMeshTile tile, dtMeshTile target)
        {
	        if (tile == null || target == null) 
                return;

	        uint targetNum = decodePolyIdTile(getTileRef(target));

	        for (int i = 0; i < tile.header.polyCount; ++i)
	        {
		        dtPoly poly = tile.polys[i];
		        uint j = poly.firstLink;
		        uint pj = DT_NULL_LINK;
		        while (j != DT_NULL_LINK)
		        {
			        if (tile.links[j].side != 0xff &&
				        decodePolyIdTile(tile.links[j].polyRef) == targetNum)
			        {
				        // Revove link.
				        uint nj = tile.links[j].next;
				        if (pj == DT_NULL_LINK)
					        poly.firstLink = nj;
				        else
					        tile.links[pj].next = nj;
				        freeLink(tile, j);
				        j = nj;
			        }
			        else
			        {
				        // Advance
				        pj = j;
				        j = tile.links[j].next;
			        }
		        }
	        }
        }

        /// Builds external polygon links for a tile.
        public void connectExtLinks(dtMeshTile tile, dtMeshTile target, int side)
        {
	        if (tile == null) 
                return;
	
	        // Connect border links.
	        for (int i = 0; i < tile.header.polyCount; ++i)
	        {
		        dtPoly poly = tile.polys[i];

		        // Create new links.
        //		ushort m = DT_EXT_LINK | (ushort)side;
		
		        int nv = (int)poly.vertCount;
		        for (int j = 0; j < nv; ++j)
		        {
			        // Skip non-portal edges.
			        if ((poly.neis[j] & DT_EXT_LINK) == 0)
				        continue;
			
			        int dir = (int)(poly.neis[j] & 0xff);
			        if (side != -1 && dir != side)
				        continue;
			
			        // Create new links
			        //const float* va = &tile.verts[poly.verts[j]*3];
			        //const float* vb = &tile.verts[poly.verts[(j+1) % nv]*3];
                    int vaStart = poly.verts[j]*3;
                    int vbStart = poly.verts[(j+1) % nv]*3;

			        dtPolyRef[] nei = new dtPolyRef[4];
			        float[] neia = new float[4*2];
			        int nnei = findConnectingPolys(tile.verts,vaStart,tile.verts,vbStart, target, dtOppositeTile(dir), nei,neia,4);
			        for (int k = 0; k < nnei; ++k)
			        {
				        uint idx = allocLink(tile);
				        if (idx != DT_NULL_LINK)
				        {
					        dtLink link = tile.links[idx];
					        link.polyRef = nei[k];
					        link.edge = (byte)j;
					        link.side = (byte)dir;
					
					        link.next = poly.firstLink;
					        poly.firstLink = idx;

					        // Compress portal limits to a byte value.
					        if (dir == 0 || dir == 4)
					        {
						        float tmin = (neia[k*2+0]-tile.verts[vaStart + 2]) / (tile.verts[vbStart + 2]-tile.verts[vaStart + 2]);
						        float tmax = (neia[k*2+1]-tile.verts[vaStart + 2]) / (tile.verts[vbStart +2]-tile.verts[vaStart + 2]);
						        if (tmin > tmax)
							        dtSwap(ref tmin,ref tmax);
						        link.bmin = (byte)(dtClamp(tmin, 0.0f, 1.0f)*255.0f);
						        link.bmax = (byte)(dtClamp(tmax, 0.0f, 1.0f)*255.0f);
					        }
					        else if (dir == 2 || dir == 6)
					        {
						        float tmin = (neia[k*2+0]-tile.verts[vaStart + 0]) / (tile.verts[vbStart +0]-tile.verts[vaStart + 0]);
						        float tmax = (neia[k*2+1]-tile.verts[vaStart + 0]) / (tile.verts[vbStart +0]-tile.verts[vaStart + 0]);
						        if (tmin > tmax)
							        dtSwap(ref tmin,ref tmax);
						        link.bmin = (byte)(dtClamp(tmin, 0.0f, 1.0f)*255.0f);
						        link.bmax = (byte)(dtClamp(tmax, 0.0f, 1.0f)*255.0f);
					        }
				        }
			        }
		        }
	        }
        }

        /// Builds external polygon links for a tile.
        public void connectExtOffMeshLinks(dtMeshTile tile, dtMeshTile target, int side)
        {
	        if (tile == null) 
                return;
	
	        // Connect off-mesh links.
	        // We are interested on links which land from target tile to this tile.
	        byte oppositeSide = (side == -1) ? (byte)0xff : (byte)dtOppositeTile(side);
	
	        for (int i = 0; i < target.header.offMeshConCount; ++i)
	        {
		        dtOffMeshConnection targetCon = target.offMeshCons[i];
		        if (targetCon.side != oppositeSide)
			        continue;

		        dtPoly targetPoly = target.polys[targetCon.poly];
		        // Skip off-mesh connections which start location could not be connected at all.
		        if (targetPoly.firstLink == DT_NULL_LINK)
			        continue;
		
		        float[] ext = new float[] { targetCon.rad, target.header.walkableClimb, targetCon.rad };
		
		        // Find polygon to connect to.
		        //const float* p = &targetCon.pos[3];
                int pIndex = 3;
		        float[] nearestPt = new float[3];
		        dtPolyRef polyRef = findNearestPolyInTile(tile, targetCon.pos, pIndex, ext, nearestPt);
		        if (polyRef == 0)
			        continue;
		        // findNearestPoly may return too optimistic results, further check to make sure. 
		        if (dtSqr(nearestPt[0]-targetCon.pos[pIndex])+dtSqr(nearestPt[2]-targetCon.pos[pIndex + 2]) > dtSqr(targetCon.rad))
			        continue;
		        // Make sure the location is on current mesh.
		        //float* v = &target.verts[targetPoly.verts[1]*3];
                int vIndex = targetPoly.verts[1]*3;
		        dtVcopy(target.verts, vIndex, nearestPt, 0);
				
		        // Link off-mesh connection to target poly.
		        uint idx = allocLink(target);
		        if (idx != DT_NULL_LINK)
		        {
			        dtLink link = target.links[idx];
			        link.polyRef = polyRef;
			        link.edge = (byte)1;
			        link.side = oppositeSide;
			        link.bmin = link.bmax = 0;
			        // Add to linked list.
			        link.next = targetPoly.firstLink;
			        targetPoly.firstLink = idx;
		        }
		
		        // Link target poly to off-mesh connection.
		        if ((targetCon.flags & DT_OFFMESH_CON_BIDIR )!= 0)
		        {
			        uint tidx = allocLink(tile);
			        if (tidx != DT_NULL_LINK)
			        {
				        ushort landPolyIdx = (ushort)decodePolyIdPoly(polyRef);
				        dtPoly landPoly = tile.polys[landPolyIdx];
				        dtLink link = tile.links[tidx];
				        link.polyRef = getPolyRefBase(target) | (dtPolyRef)(targetCon.poly);
				        link.edge = 0xff;
				        link.side = (byte)(side == -1 ? 0xff : side);
				        link.bmin = link.bmax = 0;
				        // Add to linked list.
				        link.next = landPoly.firstLink;
				        landPoly.firstLink = tidx;
			        }
		        }
	        }

        }
        /// Builds internal polygons links for a tile.
        public void connectIntLinks(dtMeshTile tile)
        {
	        if (tile == null) 
                return;

	        dtPolyRef polyRefBase = getPolyRefBase(tile);

	        for (int i = 0; i < tile.header.polyCount; ++i)
	        {
		        dtPoly poly = tile.polys[i];
		        poly.firstLink = DT_NULL_LINK;

		        if (poly.getType() == (byte) dtPolyTypes.DT_POLYTYPE_OFFMESH_CONNECTION)
			        continue;
			
		        // Build edge links backwards so that the links will be
		        // in the linked list from lowest index to highest.
		        for (int j = poly.vertCount-1; j >= 0; --j)
		        {
			        // Skip hard and non-internal edges.
			        if (poly.neis[j] == 0 || (poly.neis[j] & DT_EXT_LINK) != 0) 
                        continue;

			        uint idx = allocLink(tile);
			        if (idx != DT_NULL_LINK)
			        {
				        dtLink link = tile.links[idx];
				        link.polyRef = polyRefBase | (dtPolyRef)(poly.neis[j]-1);
				        link.edge = (byte)j;
				        link.side = 0xff;
				        link.bmin = link.bmax = 0;
				        // Add to linked list.
				        link.next = poly.firstLink;
				        poly.firstLink = idx;
			        }
		        }			
	        }
        }

        /// Builds internal polygons links for a tile.
        public void baseOffMeshLinks(dtMeshTile tile)
        {
	        if (tile == null) 
                return;
	
	        dtPolyRef polyRefBase = getPolyRefBase(tile);
	
	        // Base off-mesh connection start points.
	        for (int i = 0; i < tile.header.offMeshConCount; ++i)
	        {
		        dtOffMeshConnection con = tile.offMeshCons[i];
		        dtPoly poly = tile.polys[con.poly];
	
		        float[] ext = new float[] { con.rad, tile.header.walkableClimb, con.rad };
		
		        // Find polygon to connect to.
		        //const float* p = &con.pos[0]; // First vertex

		        float[] nearestPt = new float[3];
		        dtPolyRef polyRef = findNearestPolyInTile(tile, con.pos, 0, ext, nearestPt);
		        if (polyRef == 0) 
                    continue;
		        // findNearestPoly may return too optimistic results, further check to make sure. 
		        if (dtSqr(nearestPt[0]-con.pos[0])+dtSqr(nearestPt[2]-con.pos[2]) > dtSqr(con.rad))
			        continue;
		        // Make sure the location is on current mesh.
		        //float* v = &tile.verts[poly.verts[0]*3];
                int vIndex = poly.verts[0]*3;
		        dtVcopy(tile.verts, vIndex, nearestPt, 0);

		        // Link off-mesh connection to target poly.
		        uint idx = allocLink(tile);
		        if (idx != DT_NULL_LINK)
		        {
			        dtLink link = tile.links[idx];
			        link.polyRef = polyRef;
			        link.edge = (byte)0;
			        link.side = 0xff;
			        link.bmin = link.bmax = 0;
			        // Add to linked list.
			        link.next = poly.firstLink;
			        poly.firstLink = idx;
		        }

		        // Start end-point is always connect back to off-mesh connection. 
		        uint tidx = allocLink(tile);
		        if (tidx != DT_NULL_LINK)
		        {
			        ushort landPolyIdx = (ushort)decodePolyIdPoly(polyRef);
			        dtPoly landPoly = tile.polys[landPolyIdx];
			        dtLink link = tile.links[tidx];
			        link.polyRef = polyRefBase | (dtPolyRef)(con.poly);
			        link.edge = 0xff;
			        link.side = 0xff;
			        link.bmin = link.bmax = 0;
			        // Add to linked list.
			        link.next = landPoly.firstLink;
			        landPoly.firstLink = tidx;
		        }
	        }
        }

        public void closestPointOnPoly(dtPolyRef polyRef, float[] pos, int posStart, float[] closest, ref bool posOverPoly)
        {
	        dtMeshTile tile = null;
	        dtPoly poly = null;
            uint ip = 0;
	        getTileAndPolyByRefUnsafe(polyRef, ref tile, ref poly, ref ip);
	
	        // Off-mesh connections don't have detail polygons.
	        if (poly.getType() == (byte)dtPolyTypes.DT_POLYTYPE_OFFMESH_CONNECTION)
	        {
		        //const float* v0 = &tile.verts[poly.verts[0]*3];
		        //const float* v1 = &tile.verts[poly.verts[1]*3];
                int v0Start = poly.verts[0]*3;
                int v1Start = poly.verts[1]*3;
		        float d0 = dtVdist(pos, posStart, tile.verts, v0Start);
		        float d1 = dtVdist(pos, posStart, tile.verts, v1Start);
		        float u = d0 / (d0+d1);
		        dtVlerp(closest, 0, tile.verts,v0Start,tile.verts, v1Start, u);
		
		        posOverPoly = false;
		        return;
	        }
	
	        //uint ip = (uint)(poly - tile.polys);
	        dtPolyDetail pd = tile.detailMeshes[ip];
	
	        // Clamp point to be inside the polygon.
	        float[] verts = new float[DT_VERTS_PER_POLYGON*3];	
	        float[] edged = new float[DT_VERTS_PER_POLYGON];
	        float[] edget = new float[DT_VERTS_PER_POLYGON];
	        int nv = poly.vertCount;
	        for (int i = 0; i < nv; ++i){
		        dtVcopy(verts,i*3, tile.verts,poly.verts[i]*3);
            }
	
	        dtVcopy(closest, 0, pos, posStart);
	        if (!dtDistancePtPolyEdgesSqr(pos, posStart, verts, nv, edged, edget))
	        {
		        // Point is outside the polygon, dtClamp to nearest edge.
		        float dmin = float.MaxValue;
		        int imin = -1;
		        for (int i = 0; i < nv; ++i)
		        {
			        if (edged[i] < dmin)
			        {
				        dmin = edged[i];
				        imin = i;
			        }
		        }
		        //float[] va = &verts[imin*3];
		        //const float* vb = &verts[((imin+1)%nv)*3];
                int vaStart = imin*3;
                int vbStart = ((imin+1)%nv)*3;
		        dtVlerp(closest,0, verts,vaStart,verts, vbStart, edget[imin]);
		
		        posOverPoly = false;
	        }
	        else
	        {
		        posOverPoly = true;
	        }
	
	        // Find height at the location.
	        for (int j = 0; j < pd.triCount; ++j)
	        {
		        //const byte* t = &tile.detailTris[(pd.triBase+j)*4];
                int tIndex = (int)(pd.triBase+j)*4;
		        //float* v[3];
                int[] vIndices = new int[3];
                float[][] vArrays = new float[3][];
		        for (int k = 0; k < 3; ++k)
		        {
			        if (tile.detailTris[tIndex + k] < poly.vertCount){
				        //v[k] = &tile.verts[poly.verts[t[k]]*3];
                        vIndices[k] = poly.verts[tile.detailTris[tIndex + k]]*3;
                        vArrays[k] = tile.verts;
			        }else{
				        //v[k] = &tile.detailVerts[(pd.vertBase+(tile.detailTris[tIndex + k]-poly.vertCount))*3];
                        vIndices[k] = (int)(pd.vertBase+(tile.detailTris[tIndex + k]-poly.vertCount))*3;
                        vArrays[k] = tile.detailVerts;
                    }
		        }
                float h = .0f; ;
		        if (dtClosestHeightPointTriangle(pos, posStart, vArrays[0], vIndices[0], vArrays[1], vIndices[1], vArrays[2], vIndices[2],ref h))
		        {
			        closest[1] = h;
			        break;
		        }
	        }
        }

        public dtPolyRef findNearestPolyInTile(dtMeshTile tile,
								        float[] center, int centerStart, 
                                        float[] extents,
								        float[] nearestPt) 
        {
	        float[] bmin = new float[3];//, bmax[3];
            float[] bmax = new float[3];
	        dtVsub(bmin, 0, center, centerStart, extents, 0);
	        dtVadd(bmax, 0, center, centerStart, extents, 0);
	
	        // Get nearby polygons from proximity grid.
	        dtPolyRef[] polys = new dtPolyRef[128];
	        int polyCount = queryPolygonsInTile(tile, bmin, bmax, polys, 128);
	
	        // Find nearest polygon amongst the nearby polygons.
	        dtPolyRef nearest = 0;
	        float nearestDistanceSqr = float.MaxValue;
	        for (int i = 0; i < polyCount; ++i)
	        {
		        dtPolyRef polyRef = polys[i];
		        float[] closestPtPoly = new float[3];
		        float[] diff = new float[3];
		        bool posOverPoly = false;
		        float d = 0;
		        closestPointOnPoly(polyRef, center, centerStart, closestPtPoly, ref posOverPoly);

		        // If a point is directly over a polygon and closer than
		        // climb height, favor that instead of straight line nearest point.
		        dtVsub(diff, 0, center, centerStart, closestPtPoly, 0);
		        if (posOverPoly)
		        {
			        d = Math.Abs(diff[1]) - tile.header.walkableClimb;
			        d = d > 0 ? d*d : 0;			
		        }
		        else
		        {
			        d = dtVlenSqr(diff);
		        }
		
		        if (d < nearestDistanceSqr)
		        {
			        dtVcopy(nearestPt, closestPtPoly);
			        nearestDistanceSqr = d;
			        nearest = polyRef;
		        }
	        }
	
	        return nearest;
        }

        public int queryPolygonsInTile(dtMeshTile tile, float[] qmin, float[] qmax,
                                dtPolyRef[] polys, int maxPolys)
        {
	        if (tile.bvTree != null)
	        {
		        // tile.bvTree[0];
	            //dtBVNode end = tile.bvTree[tile.header.bvNodeCount];
                int endNodeIndex = tile.header.bvNodeCount;
		        float[] tbmin = tile.header.bmin;
		        float[] tbmax = tile.header.bmax;
		        float qfac = tile.header.bvQuantFactor;
		
		        // Calculate quantized box
		        ushort[] bmin = new ushort[3];//, bmax[3];
                ushort[] bmax = new ushort[3];
		        // dtClamp query box to world box.
		        float minx = dtClamp(qmin[0], tbmin[0], tbmax[0]) - tbmin[0];
		        float miny = dtClamp(qmin[1], tbmin[1], tbmax[1]) - tbmin[1];
		        float minz = dtClamp(qmin[2], tbmin[2], tbmax[2]) - tbmin[2];
		        float maxx = dtClamp(qmax[0], tbmin[0], tbmax[0]) - tbmin[0];
		        float maxy = dtClamp(qmax[1], tbmin[1], tbmax[1]) - tbmin[1];
		        float maxz = dtClamp(qmax[2], tbmin[2], tbmax[2]) - tbmin[2];
		        // Quantize
		        bmin[0] = (ushort)((ushort)(qfac * minx) & 0xfffe);
		        bmin[1] = (ushort)((ushort)(qfac * miny) & 0xfffe);
		        bmin[2] = (ushort)((ushort)(qfac * minz) & 0xfffe);
		        bmax[0] = (ushort)((ushort)(qfac * maxx + 1) | 1);
		        bmax[1] = (ushort)((ushort)(qfac * maxy + 1) | 1);
		        bmax[2] = (ushort)((ushort)(qfac * maxz + 1) | 1);
		
		        // Traverse tree
		        dtPolyRef polyRefBase = getPolyRefBase(tile);
		        int n = 0;
                int curNode = 0;
                dtBVNode node = null;
		        while (curNode < endNodeIndex)
		        {
                    node = tile.bvTree[curNode];

			        bool overlap = dtOverlapQuantBounds(bmin, bmax, node.bmin, node.bmax);
			        bool isLeafNode = node.i >= 0;
			
			        if (isLeafNode && overlap)
			        {
				        if (n < maxPolys){
					        polys[n++] = polyRefBase | (dtPolyRef)node.i;
                        }
			        }
			
			        if (overlap || isLeafNode){
				        //node++;
                        ++curNode;
                    }
			        else
			        {
				        int escapeIndex = - node.i;
				        curNode += escapeIndex;
			        }
		        }
		
		        return n;
	        }
	        else
	        {
		        float[] bmin = new float[3];//, bmax[3];
                float[] bmax = new float[3];
		        int n = 0;
		        dtPolyRef polyRefBase = getPolyRefBase(tile);
		        for (int i = 0; i < tile.header.polyCount; ++i)
		        {
			        dtPoly p = tile.polys[i];
            
			        // Do not return off-mesh connection polygons.
			        if (p.getType() == (byte) dtPolyTypes.DT_POLYTYPE_OFFMESH_CONNECTION)
				        continue;
			        // Calc polygon bounds.
			        //float[] v = tile.verts[p.verts[0]*3];
                    int vIndex = p.verts[0]*3;
			        dtVcopy(bmin,0 ,tile.verts, vIndex);
			        dtVcopy(bmax,0 ,tile.verts, vIndex);
			        for (int j = 1; j < p.vertCount; ++j)
			        {
				        //v = &tile.verts[p.verts[j]*3];
                        vIndex = p.verts[j]*3;
				        dtVmin(bmin, 0 ,tile.verts, vIndex);
				        dtVmax(bmax, 0 ,tile.verts, vIndex);
			        }
			        if (dtOverlapBounds(qmin,qmax, bmin,bmax))
			        {
				        if (n < maxPolys)
					        polys[n++] = polyRefBase | (dtPolyRef)i;
			        }
		        }
		        return n;
	        }
        }

        /// @par
        ///
        /// The add operation will fail if the data is in the wrong format, the allocated tile
        /// space is full, or there is a tile already at the specified reference.
        ///
        /// The lastRef parameter is used to restore a tile with the same tile
        /// reference it had previously used.  In this case the #dtPolyRef's for the
        /// tile will be restored to the same values they were before the tile was 
        /// removed.
        ///
        /// @see dtCreateNavMeshData, #removeTile
        /// Adds a tile to the navigation mesh.
        ///  @param[in]		data		Data for the new tile mesh. (See: #dtCreateNavMeshData)
        ///  @param[in]		dataSize	Data size of the new tile mesh.
        ///  @param[in]		flags		Tile flags. (See: #dtTileFlags)
        ///  @param[in]		lastRef		The desired reference for the tile. (When reloading a tile.) [opt] [Default: 0]
        ///  @param[out]	result		The tile reference. (If the tile was succesfully added.) [opt]
        /// @return The status flags for the operation.
        public dtStatus addTile(dtRawTileData rawTileData, int flags, dtTileRef lastRef, ref dtTileRef result)
        {
            //C#: Using an intermediate class dtRawTileData because Cpp uses a binary buffer.

	        // Make sure the data is in right format.
	        dtMeshHeader header = rawTileData.header;
	        if (header.magic != DT_NAVMESH_MAGIC)
		        return DT_FAILURE | DT_WRONG_MAGIC;
	        if (header.version != DT_NAVMESH_VERSION)
		        return DT_FAILURE | DT_WRONG_VERSION;
		
	        // Make sure the location is free.
	        if (getTileAt(header.x, header.y, header.layer) != null)
		        return DT_FAILURE;
		
	        // Allocate a tile.
	        dtMeshTile tile = null;
	        if (lastRef == 0)
	        {
		        if (m_nextFree != null)
		        {
			        tile = m_nextFree;
			        m_nextFree = tile.next;
			        tile.next = null;
		        }
	        }
	        else
	        {
		        // Try to relocate the tile to specific index with same salt.
		        int tileIndex = (int)decodePolyIdTile((dtPolyRef)lastRef);
		        if (tileIndex >= m_maxTiles)
			        return DT_FAILURE | DT_OUT_OF_MEMORY;
		        // Try to find the specific tile id from the free list.
		        dtMeshTile target = m_tiles[tileIndex];
		        dtMeshTile prev = null;
		        tile = m_nextFree;
		        while (tile != null && tile != target)
		        {
			        prev = tile;
			        tile = tile.next;
		        }
		        // Could not find the correct location.
		        if (tile != target)
			        return DT_FAILURE | DT_OUT_OF_MEMORY;
		        // Remove from freelist
		        if (prev == null)
			        m_nextFree = tile.next;
		        else
			        prev.next = tile.next;

		        // Restore salt.
		        tile.salt = decodePolyIdSalt((dtPolyRef)lastRef);
	        }

	        // Make sure we could allocate a tile.
	        if (tile == null)
		        return DT_FAILURE | DT_OUT_OF_MEMORY;
	
	        // Insert tile into the position lut.
	        int h = computeTileHash(header.x, header.y, m_tileLutMask);
	        tile.next = m_posLookup[h];
	        m_posLookup[h] = tile;
	
	        // Patch header pointers.
    
	        //int vertsCount = 3*header.vertCount;
	        //int polysSize = header.polyCount;
	        //int linksSize = header.maxLinkCount;
	        //int detailMeshesSize = header.detailMeshCount;
	        //int detailVertsSize = 3*header.detailVertCount;
	        //int detailTrisSize = 4*header.detailTriCount;
	        //int bvtreeSize = header.bvNodeCount;
	        //int offMeshLinksSize = header.offMeshConCount;

	        //byte* d = data + headerSize;
	        tile.verts = rawTileData.verts;
	        tile.polys = rawTileData.polys;
	        tile.links = rawTileData.links;
	        tile.detailMeshes = rawTileData.detailMeshes;
	        tile.detailVerts = rawTileData.detailVerts;
	        tile.detailTris = rawTileData.detailTris;
	        tile.bvTree = rawTileData.bvTree;
	        tile.offMeshCons = rawTileData.offMeshCons;

	        // If there are no items in the bvtree, reset the tree pointer.
            //c#: unnecessary, Cpp is afraid to point to whatever data ends up here
	        //if (bvtreeSize == 0)
	        //	tile.bvTree = null;

	        // Build links freelist
	        tile.linksFreeList = 0;
	        tile.links[header.maxLinkCount-1].next = DT_NULL_LINK;
	        for (int i = 0; i < header.maxLinkCount-1; ++i){
		        tile.links[i].next = (dtTileRef) (i+1);
            }

	        // Init tile.
	        tile.header = header;
	        tile.data = rawTileData;
	        //tile.dataSize = dataSize;
	        tile.flags = flags;

	        connectIntLinks(tile);
	        baseOffMeshLinks(tile);

	        // Create connections with neighbour tiles.
	        const int MAX_NEIS = 32;
	        dtMeshTile[] neis = new dtMeshTile[MAX_NEIS];
	        int nneis;
	
	        // Connect with layers in current tile.
	        nneis = getTilesAt(header.x, header.y, neis, MAX_NEIS);
	        for (int j = 0; j < nneis; ++j)
	        {
		        if (neis[j] != tile)
		        {
			        connectExtLinks(tile, neis[j], -1);
			        connectExtLinks(neis[j], tile, -1);
		        }
		        connectExtOffMeshLinks(tile, neis[j], -1);
		        connectExtOffMeshLinks(neis[j], tile, -1);
	        }
	
	        // Connect with neighbour tiles.
	        for (int i = 0; i < 8; ++i)
	        {
		        nneis = getNeighbourTilesAt(header.x, header.y, i, neis, MAX_NEIS);
		        for (int j = 0; j < nneis; ++j)
		        {
			        connectExtLinks(tile, neis[j], i);
			        connectExtLinks(neis[j], tile, dtOppositeTile(i));
			        connectExtOffMeshLinks(tile, neis[j], i);
			        connectExtOffMeshLinks(neis[j], tile, dtOppositeTile(i));
		        }
	        }
	
	
	        result = getTileRef(tile);
	
	        return DT_SUCCESS;
        }

        /// Gets the tile at the specified grid location.
        ///  @param[in]	x		The tile's x-location. (x, y, layer)
        ///  @param[in]	y		The tile's y-location. (x, y, layer)
        ///  @param[in]	layer	The tile's layer. (x, y, layer)
        /// @return The tile, or null if the tile does not exist.
        public dtMeshTile getTileAt(int x, int y, int layer) 
        {
	        // Find tile based on hash.
	        int h = computeTileHash(x,y,m_tileLutMask);
	        dtMeshTile tile = m_posLookup[h];
	        while (tile != null)
	        {
		        if (tile.header != null &&
			        tile.header.x == x &&
			        tile.header.y == y &&
			        tile.header.layer == layer)
		        {
			        return tile;
		        }
		        tile = tile.next;
	        }
	        return null;
        }

        public int getNeighbourTilesAt(int x, int y, int side, dtMeshTile[] tiles, int maxTiles)
        {
	        int nx = x, ny = y;
	        switch (side)
	        {
		        case 0: nx++; break;
		        case 1: nx++; ny++; break;
		        case 2: ny++; break;
		        case 3: nx--; ny++; break;
		        case 4: nx--; break;
		        case 5: nx--; ny--; break;
		        case 6: ny--; break;
		        case 7: nx++; ny--; break;
	        };

	        return getTilesAt(nx, ny, tiles, maxTiles);
        }

    
        /// @par
        ///
        /// This function will not fail if the tiles array is too small to hold the
        /// entire result set.  It will simply fill the array to capacity.
        /// Gets all tiles at the specified grid location. (All layers.)
        ///  @param[in]		x			The tile's x-location. (x, y)
        ///  @param[in]		y			The tile's y-location. (x, y)
        ///  @param[out]	tiles		A pointer to an array of tiles that will hold the result.
        ///  @param[in]		maxTiles	The maximum tiles the tiles parameter can hold.
        /// @return The number of tiles returned in the tiles array.
        public int getTilesAt(int x, int y, dtMeshTile[] tiles, int maxTiles)
        {
	        int n = 0;
	
	        // Find tile based on hash.
	        int h = computeTileHash(x,y,m_tileLutMask);
	        dtMeshTile tile = m_posLookup[h];
	        while (tile != null)
	        {
		        if (tile.header != null &&
			        tile.header.x == x &&
			        tile.header.y == y)
		        {
			        if (n < maxTiles)
				        tiles[n++] = tile;
		        }
		        tile = tile.next;
	        }
	
	        return n;
        }

        /// Gets the tile reference for the tile at specified grid location.
        ///  @param[in]	x		The tile's x-location. (x, y, layer)
        ///  @param[in]	y		The tile's y-location. (x, y, layer)
        ///  @param[in]	layer	The tile's layer. (x, y, layer)
        /// @return The tile reference of the tile, or 0 if there is none.
        public dtTileRef getTileRefAt(int x, int y, int layer) 
        {
	        // Find tile based on hash.
	        int h = computeTileHash(x,y,m_tileLutMask);
	        dtMeshTile tile = m_posLookup[h];
	        while (tile != null)
	        {
		        if (tile.header != null &&
			        tile.header.x == x &&
			        tile.header.y == y &&
			        tile.header.layer == layer)
		        {
			        return getTileRef(tile);
		        }
		        tile = tile.next;
	        }
	        return 0;
        }
        /// Gets the tile for the specified tile reference.
        ///  @param[in]	ref		The tile reference of the tile to retrieve.
        /// @return The tile for the specified reference, or null if the 
        ///		reference is invalid.
        public dtMeshTile getTileByRef(dtTileRef tileRef) 
        {
	        if (tileRef != 0)
		        return null;
	        uint tileIndex = decodePolyIdTile((dtPolyRef)tileRef);
	        uint tileSalt = decodePolyIdSalt((dtPolyRef)tileRef);
	        if ((int)tileIndex >= m_maxTiles)
		        return null;
	        dtMeshTile tile = m_tiles[tileIndex];
	        if (tile.salt != tileSalt)
		        return null;
	        return tile;
        }

        /// The maximum number of tiles supported by the navigation mesh.
        /// @return The maximum number of tiles supported by the navigation mesh.
        public int getMaxTiles()
        {
	        return m_maxTiles;
        }

        /// Gets the tile at the specified index.
        ///  @param[in]	i		The tile index. [Limit: 0 >= index < #getMaxTiles()]
        /// @return The tile at the specified index.
        public dtMeshTile getTile(int i)
        {
	        return m_tiles[i];
        }

        /// Calculates the tile grid location for the specified world position.
        ///  @param[in]	pos  The world position for the query. [(x, y, z)]
        ///  @param[out]	tx		The tile's x-location. (x, y)
        ///  @param[out]	ty		The tile's y-location. (x, y)
        public void calcTileLoc(float[] pos, ref int tx, ref int ty)
        {
	        tx = (int)Math.Floor((pos[0]-m_orig[0]) / m_tileWidth);
	        ty = (int)Math.Floor((pos[2]-m_orig[2]) / m_tileHeight);
        }

        /// Gets the tile and polygon for the specified polygon reference.
        ///  @param[in]		ref		The reference for the a polygon.
        ///  @param[out]	tile	The tile containing the polygon.
        ///  @param[out]	poly	The polygon.
        /// @return The status flags for the operation.
        public dtStatus getTileAndPolyByRef(dtPolyRef polyRef, ref dtMeshTile tile, ref dtPoly poly) 
        {
	        if (polyRef == 0) 
                return DT_FAILURE;
	        uint salt = 0, it = 0, ip = 0;
	        decodePolyId(polyRef,ref salt,ref it,ref ip);
	        if (it >= (uint)m_maxTiles) 
                return DT_FAILURE | DT_INVALID_PARAM;
	        if (m_tiles[it].salt != salt || m_tiles[it].header == null)    
                return DT_FAILURE | DT_INVALID_PARAM;
	        if (ip >= (uint)m_tiles[it].header.polyCount) 
                return DT_FAILURE | DT_INVALID_PARAM;
	        tile = m_tiles[it];
	        poly = m_tiles[it].polys[ip];
	        return DT_SUCCESS;
        }

        //C# port : also return ip because the code used to do pointer arithmetics on the
        // array's addresses, which is a no in C# because managed array may not be contiguous in memory
        public dtStatus getTileAndPolyByRef(dtPolyRef polyRef, ref dtMeshTile tile, ref dtPoly poly, ref uint ip) {
            if (polyRef == 0)
                return DT_FAILURE;
            uint salt = 0, it = 0;
            decodePolyId(polyRef, ref salt, ref it, ref ip);
            if (it >= (uint)m_maxTiles)
                return DT_FAILURE | DT_INVALID_PARAM;
            if (m_tiles[it].salt != salt || m_tiles[it].header == null)
                return DT_FAILURE | DT_INVALID_PARAM;
            if (ip >= (uint)m_tiles[it].header.polyCount)
                return DT_FAILURE | DT_INVALID_PARAM;
            tile = m_tiles[it];
            poly = m_tiles[it].polys[ip];
            return DT_SUCCESS;
        }

        /// @par
        ///
        /// @warning Only use this function if it is known that the provided polygon
        /// reference is valid. This function is faster than #getTileAndPolyByRef, but
        /// it does not validate the reference.
        /// Returns the tile and polygon for the specified polygon reference.
        ///  @param[in]		ref		A known valid reference for a polygon.
        ///  @param[out]	tile	The tile containing the polygon.
        ///  @param[out]	poly	The polygon.
        public void getTileAndPolyByRefUnsafe(dtPolyRef polyRef, ref dtMeshTile tile, ref dtPoly poly)
        {
	        uint salt = 0, it = 0, ip = 0;
	        decodePolyId(polyRef,ref salt,ref it,ref ip);
	        tile = m_tiles[it];
	        poly = m_tiles[it].polys[ip];
        }

        public void getTileAndPolyByRefUnsafe(dtPolyRef polyRef, ref dtMeshTile tile, ref dtPoly poly, ref uint ip)
        {
	        uint salt = 0, it = 0;
	        decodePolyId(polyRef,ref salt,ref it,ref ip);
	        tile = m_tiles[it];
	        poly = m_tiles[it].polys[ip];
        }

        /// Checks the validity of a polygon reference.
        ///  @param[in]	ref		The polygon reference to check.
        /// @return True if polygon reference is valid for the navigation mesh.
        public bool isValidPolyRef(dtPolyRef polyRef)
        {
	        if (polyRef == 0) 
                return false;
	        uint salt = 0, it = 0, ip = 0;
	        decodePolyId(polyRef,ref salt,ref it,ref ip);
	        if (it >= (uint)m_maxTiles) 
                return false;
	        if (m_tiles[it].salt != salt || m_tiles[it].header == null) 
                return false;
	        if (ip >= (uint)m_tiles[it].header.polyCount) 
                return false;
	        return true;
        }

        /// @par
        ///
        /// This function returns the data for the tile so that, if desired,
        /// it can be added back to the navigation mesh at a later point.
        ///
        /// @see #addTile
        /// Removes the specified tile from the navigation mesh.
        ///  @param[in]		ref			The reference of the tile to remove.
        ///  @param[out]	data		Data associated with deleted tile.
        ///  @param[out]	dataSize	Size of the data associated with deleted tile.
        /// @return The status flags for the operation.
        public dtStatus removeTile(dtTileRef tileRef, out dtRawTileData rawTileData)
        {
            rawTileData = null;
         
	        if (tileRef == 0)
		        return DT_FAILURE | DT_INVALID_PARAM;
	        uint tileIndex = decodePolyIdTile((dtPolyRef)tileRef);
	        uint tileSalt = decodePolyIdSalt((dtPolyRef)tileRef);
	        if ((int)tileIndex >= m_maxTiles)
		        return DT_FAILURE | DT_INVALID_PARAM;
	        dtMeshTile tile = m_tiles[tileIndex];
	        if (tile.salt != tileSalt)
		        return DT_FAILURE | DT_INVALID_PARAM;
	
	        // Remove tile from hash lookup.
	        int h = computeTileHash(tile.header.x,tile.header.y,m_tileLutMask);
	        dtMeshTile prev = null;
	        dtMeshTile cur = m_posLookup[h];
	        while (cur != null)
	        {
		        if (cur == tile)
		        {
			        if (prev != null)
				        prev.next = cur.next;
			        else
				        m_posLookup[h] = cur.next;
			        break;
		        }
		        prev = cur;
		        cur = cur.next;
	        }
	
	        // Remove connections to neighbour tiles.
	        // Create connections with neighbour tiles.
	        const int MAX_NEIS = 32;
	        dtMeshTile[] neis = new dtMeshTile[MAX_NEIS];
	        int nneis;
	
	        // Connect with layers in current tile.
	        nneis = getTilesAt(tile.header.x, tile.header.y, neis, MAX_NEIS);
	        for (int j = 0; j < nneis; ++j)
	        {
		        if (neis[j] == tile) continue;
		        unconnectExtLinks(neis[j], tile);
	        }
	
	        // Connect with neighbour tiles.
	        for (int i = 0; i < 8; ++i)
	        {
		        nneis = getNeighbourTilesAt(tile.header.x, tile.header.y, i, neis, MAX_NEIS);
		        for (int j = 0; j < nneis; ++j)
			        unconnectExtLinks(neis[j], tile);
	        }
		
	        // Reset tile.
	        if ((tile.flags & (int)dtTileFlags.DT_TILE_FREE_DATA) != 0)
	        {
		        // Owns data
		        //dtFree(tile.data);
		        tile.data = null;
		        //tile.dataSize = 0;
		        //if (data) *data = 0;
		        //if (dataSize) *dataSize = 0;
                rawTileData = null;
        
	        }
	        else
	        {
		        //if (data) *data = tile.data;
		        //if (dataSize) *dataSize = tile.dataSize;
                rawTileData = tile.data;
	        }

	        tile.header = null;
	        tile.flags = 0;
	        tile.linksFreeList = 0;
	        tile.polys = null;
	        tile.verts = null;
	        tile.links = null;
	        tile.detailMeshes = null;
	        tile.detailVerts = null;
	        tile.detailTris = null;
	        tile.bvTree = null;
	        tile.offMeshCons = null;

	        // Update salt, salt should never be zero.
        #if DT_POLYREF64
	        tile.salt = (tile.salt+1) & ((1<<DT_SALT_BITS)-1);
        #else
	        tile.salt = (dtTileRef)( (tile.salt+1) & ((1<<(int)m_saltBits)-1) );
        #endif
	        if (tile.salt == 0)
		        tile.salt++;

	        // Add to free list.
	        tile.next = m_nextFree;
	        m_nextFree = tile;

	        return DT_SUCCESS;
        }

        /// Gets the tile reference for the specified tile.
        ///  @param[in]	tile	The tile.
        /// @return The tile reference of the tile.
        public dtTileRef getTileRef(dtMeshTile tile)
        {
	        if (tile == null) return 0;
	        uint it = (uint) Array.IndexOf(m_tiles, tile); //(uint)(tile - m_tiles);
	        return encodePolyId(tile.salt, it, 0);
        }

        /// @par
        ///
        /// Example use case:
        /// @code
        ///
        /// const dtPolyRef base = navmesh.getPolyRefBase(tile);
        /// for (int i = 0; i < tile.header.polyCount; ++i)
        /// {
        ///     const dtPoly* p = &tile.polys[i];
        ///     const dtPolyRef ref = base | (dtPolyRef)i;
        ///     
        ///     // Use the reference to access the polygon data.
        /// }
        /// @endcode
        /// Gets the polygon reference for the tile's base polygon.
        ///  @param[in]	tile		The tile.
        /// @return The polygon reference for the base polygon in the specified tile.
        public dtPolyRef getPolyRefBase(dtMeshTile tile)
        {
	        if (tile == null) return 0;
	        uint it = (uint) Array.IndexOf(m_tiles, tile);
	        return encodePolyId(tile.salt, it, 0);
        }

        public struct dtTileState
        {
	        int magic;								// Magic number, used to identify the data.
	        int version;							// Data version number.
	        dtTileRef tileRef;							// Tile ref at the time of storing the data.
        };

        public struct dtPolyState
        {
	        ushort flags;						// Flags (see dtPolyFlags).
	        byte area;							// Area ID of the polygon.
        };


        //C#:Following code is never called in the sample and i'm not sure what it's for
        /*
        ///  @see #storeTileState
        /// Gets the size of the buffer required by #storeTileState to store the specified tile's state.
        ///  @param[in]	tile	The tile.
        /// @return The size of the buffer required to store the state.
        int getTileStateSize(dtMeshTile tile)
        {
            if (tile == null) return 0;
            int headerSize = dtAlign4(sizeof(dtTileState));
            int polyStateSize = dtAlign4(sizeof(dtPolyState) * tile.header.polyCount);
            return headerSize + polyStateSize;
        }

        /// @par
        ///
        /// Tile state includes non-structural data such as polygon flags, area ids, etc.
        /// @note The state data is only valid until the tile reference changes.
        /// @see #getTileStateSize, #restoreTileState
        /// Stores the non-structural state of the tile in the specified buffer. (Flags, area ids, etc.)
        ///  @param[in]		tile			The tile.
        ///  @param[out]	data			The buffer to store the tile's state in.
        ///  @param[in]		maxDataSize		The size of the data buffer. [Limit: >= #getTileStateSize]
        /// @return The status flags for the operation.
        dtStatus dtNavMesh::storeTileState(const dtMeshTile* tile, byte* data, const int maxDataSize) const
        {
            // Make sure there is enough space to store the state.
            const int sizeReq = getTileStateSize(tile);
            if (maxDataSize < sizeReq)
                return DT_FAILURE | DT_BUFFER_TOO_SMALL;
		
            dtTileState* tileState = (dtTileState*)data; data += dtAlign4(sizeof(dtTileState));
            dtPolyState* polyStates = (dtPolyState*)data; data += dtAlign4(sizeof(dtPolyState) * tile.header.polyCount);
	
            // Store tile state.
            tileState.magic = DT_NAVMESH_STATE_MAGIC;
            tileState.version = DT_NAVMESH_STATE_VERSION;
            tileState.ref = getTileRef(tile);
	
            // Store per poly state.
            for (int i = 0; i < tile.header.polyCount; ++i)
            {
                const dtPoly* p = &tile.polys[i];
                dtPolyState* s = &polyStates[i];
                s.flags = p.flags;
                s.area = p.getArea();
            }
	
            return DT_SUCCESS;
        }

        /// @par
        ///
        /// Tile state includes non-structural data such as polygon flags, area ids, etc.
        /// @note This function does not impact the tile's #dtTileRef and #dtPolyRef's.
        /// @see #storeTileState
        /// Restores the state of the tile.
        ///  @param[in]	tile			The tile.
        ///  @param[in]	data			The new state. (Obtained from #storeTileState.)
        ///  @param[in]	maxDataSize		The size of the state within the data buffer.
        /// @return The status flags for the operation.
        dtStatus dtNavMesh::restoreTileState(dtMeshTile* tile, const byte* data, const int maxDataSize)
        {
            // Make sure there is enough space to store the state.
            const int sizeReq = getTileStateSize(tile);
            if (maxDataSize < sizeReq)
                return DT_FAILURE | DT_INVALID_PARAM;
	
            const dtTileState* tileState = (const dtTileState*)data; data += dtAlign4(sizeof(dtTileState));
            const dtPolyState* polyStates = (const dtPolyState*)data; data += dtAlign4(sizeof(dtPolyState) * tile.header.polyCount);
	
            // Check that the restore is possible.
            if (tileState.magic != DT_NAVMESH_STATE_MAGIC)
                return DT_FAILURE | DT_WRONG_MAGIC;
            if (tileState.version != DT_NAVMESH_STATE_VERSION)
                return DT_FAILURE | DT_WRONG_VERSION;
            if (tileState.ref != getTileRef(tile))
                return DT_FAILURE | DT_INVALID_PARAM;
	
            // Restore per poly state.
            for (int i = 0; i < tile.header.polyCount; ++i)
            {
                dtPoly* p = &tile.polys[i];
                const dtPolyState* s = &polyStates[i];
                p.flags = s.flags;
                p.setArea(s.area);
            }
	
            return DT_SUCCESS;
        }
            */
        /// @par
        ///
        /// Off-mesh connections are stored in the navigation mesh as special 2-vertex 
        /// polygons with a single edge. At least one of the vertices is expected to be 
        /// inside a normal polygon. So an off-mesh connection is "entered" from a 
        /// normal polygon at one of its endpoints. This is the polygon identified by 
        /// the prevRef parameter.
        /// Gets the endpoints for an off-mesh connection, ordered by "direction of travel".
        ///  @param[in]		prevRef		The reference of the polygon before the connection.
        ///  @param[in]		polyRef		The reference of the off-mesh connection polygon.
        ///  @param[out]	startPos	The start position of the off-mesh connection. [(x, y, z)]
        ///  @param[out]	endPos		The end position of the off-mesh connection. [(x, y, z)]
        /// @return The status flags for the operation.
        public dtStatus getOffMeshConnectionPolyEndPoints(dtPolyRef prevRef, dtPolyRef polyRef, float[] startPos, float[] endPos)
        {
	        uint salt = 0, it = 0, ip = 0;

	        if (polyRef == 0)
		        return DT_FAILURE;
	
	        // Get current polygon
	        decodePolyId(polyRef,ref salt,ref it,ref ip);
	        if (it >= (uint)m_maxTiles) return DT_FAILURE | DT_INVALID_PARAM;
	        if (m_tiles[it].salt != salt || m_tiles[it].header == null) return DT_FAILURE | DT_INVALID_PARAM;
	        dtMeshTile tile = m_tiles[it];
	        if (ip >= (uint)tile.header.polyCount) return DT_FAILURE | DT_INVALID_PARAM;
	        dtPoly poly = tile.polys[ip];

	        // Make sure that the current poly is indeed off-mesh link.
	        if (poly.getType() != (byte)dtPolyTypes.DT_POLYTYPE_OFFMESH_CONNECTION)
		        return DT_FAILURE;

	        // Figure out which way to hand out the vertices.
	        int idx0 = 0, idx1 = 1;
	
	        // Find link that points to first vertex.
	        for (uint i = poly.firstLink; i != DT_NULL_LINK; i = tile.links[i].next)
	        {
		        if (tile.links[i].edge == 0)
		        {
			        if (tile.links[i].polyRef != prevRef)
			        {
				        idx0 = 1;
				        idx1 = 0;
			        }
			        break;
		        }
	        }
	
	        dtVcopy(startPos, 0, tile.verts, poly.verts[idx0]*3);
	        dtVcopy(endPos, 0, tile.verts, poly.verts[idx1]*3);

	        return DT_SUCCESS;
        }

        /// Gets the specified off-mesh connection.
        ///  @param[in]	ref		The polygon reference of the off-mesh connection.
        /// @return The specified off-mesh connection, or null if the polygon reference is not valid.
        public dtOffMeshConnection getOffMeshConnectionByRef(dtPolyRef polyRef)
        {
	        uint salt = 0, it = 0, ip = 0;
	
	        if (polyRef == 0)
		        return null;
	
	        // Get current polygon
	        decodePolyId(polyRef, ref salt, ref it, ref ip);
	        if (it >= (uint)m_maxTiles) return null;
	        if (m_tiles[it].salt != salt || m_tiles[it].header == null) return null;
	        dtMeshTile tile = m_tiles[it];
	        if (ip >= (uint)tile.header.polyCount) return null;
	        dtPoly poly = tile.polys[ip];
	
	        // Make sure that the current poly is indeed off-mesh link.
	        if (poly.getType() != (byte) dtPolyTypes.DT_POLYTYPE_OFFMESH_CONNECTION)
		        return null;

	        uint idx = (uint)( ip - tile.header.offMeshBase );
	        Debug.Assert(idx < (uint)tile.header.offMeshConCount);
	        return tile.offMeshCons[idx];
        }

        /// @{
        /// @name State Management
        /// These functions do not effect #dtTileRef or #dtPolyRef's. 

        /// Sets the user defined flags for the specified polygon.
        ///  @param[in]	ref		The polygon reference.
        ///  @param[in]	flags	The new flags for the polygon.
        /// @return The status flags for the operation.
        public dtStatus setPolyFlags(dtPolyRef polyRef, ushort flags)
        {
	        if (polyRef == 0) return DT_FAILURE;
	        uint salt = 0, it = 0, ip = 0;
	        decodePolyId(polyRef,ref salt,ref it,ref ip);
	        if (it >= (uint)m_maxTiles) return DT_FAILURE | DT_INVALID_PARAM;
	        if (m_tiles[it].salt != salt || m_tiles[it].header == null) return DT_FAILURE | DT_INVALID_PARAM;
	        dtMeshTile tile = m_tiles[it];
	        if (ip >= (uint)tile.header.polyCount) return DT_FAILURE | DT_INVALID_PARAM;
	        dtPoly poly = tile.polys[ip];
	
	        // Change flags.
	        poly.flags = flags;
	
	        return DT_SUCCESS;
        }

        //dtStatus setPolyFlags(dtPolyRef ref, ushort flags);

        /// Gets the user defined flags for the specified polygon.
        ///  @param[in]		ref				The polygon reference.
        ///  @param[out]	resultFlags		The polygon flags.
        /// @return The status flags for the operation.
        public dtStatus getPolyFlags(dtPolyRef polyRef, ref ushort resultFlags)
        {
	        if (polyRef == 0) return DT_FAILURE;
	        uint salt = 0, it = 0, ip = 0;
	        decodePolyId(polyRef, ref salt,ref it,ref ip);
	        if (it >= (uint)m_maxTiles) return DT_FAILURE | DT_INVALID_PARAM;
	        if (m_tiles[it].salt != salt || m_tiles[it].header == null) return DT_FAILURE | DT_INVALID_PARAM;
	        dtMeshTile tile = m_tiles[it];
	        if (ip >= (uint)tile.header.polyCount) return DT_FAILURE | DT_INVALID_PARAM;
	        dtPoly poly = tile.polys[ip];

	        resultFlags = poly.flags;
	
	        return DT_SUCCESS;
        }

        /// Sets the user defined area for the specified polygon.
        ///  @param[in]	ref		The polygon reference.
        ///  @param[in]	area	The new area id for the polygon. [Limit: < #DT_MAX_AREAS]
        /// @return The status flags for the operation.
        public dtStatus setPolyArea(dtPolyRef polyRef, byte area)
        {
	        if (polyRef == 0) return DT_FAILURE;
	        uint salt = 0, it = 0, ip = 0;
	        decodePolyId(polyRef,ref salt,ref it,ref ip);
	        if (it >= (uint)m_maxTiles) return DT_FAILURE | DT_INVALID_PARAM;
	        if (m_tiles[it].salt != salt || m_tiles[it].header == null) return DT_FAILURE | DT_INVALID_PARAM;
	        dtMeshTile tile = m_tiles[it];
	        if (ip >= (uint)tile.header.polyCount) return DT_FAILURE | DT_INVALID_PARAM;
	        dtPoly poly = tile.polys[ip];
	
	        poly.setArea(area);
	
	        return DT_SUCCESS;
        }

        /// Gets the user defined area for the specified polygon.
        ///  @param[in]		ref			The polygon reference.
        ///  @param[out]	resultArea	The area id for the polygon.
        /// @return The status flags for the operation.
        public dtStatus getPolyArea(dtPolyRef polyRef, ref byte resultArea)
        {
	        if (polyRef == 0) return DT_FAILURE;
	        uint salt = 0, it = 0, ip = 0;
	        decodePolyId(polyRef,ref salt,ref it,ref ip);
	        if (it >= (uint)m_maxTiles) return DT_FAILURE | DT_INVALID_PARAM;
	        if (m_tiles[it].salt != salt || m_tiles[it].header == null) return DT_FAILURE | DT_INVALID_PARAM;
	        dtMeshTile tile = m_tiles[it];
	        if (ip >= (uint)tile.header.polyCount) return DT_FAILURE | DT_INVALID_PARAM;
	        dtPoly poly = tile.polys[ip];
	
	        resultArea = poly.getArea();
	
	        return DT_SUCCESS;
        }
    }
}

