using System;
using System.Collections.Generic;

#if DT_POLYREF64
using dtPolyRef = System.UInt64;
#else
using dtPolyRef = System.UInt32;
#endif

public static partial class Detour{

    /// The maximum number of vertices per navigation polygon.
    /// @ingroup detour
    public const int DT_VERTS_PER_POLYGON = 6;

    /// @{
    /// @name Tile Serialization Constants
    /// These constants are used to detect whether a navigation tile's data
    /// and state format is compatible with the current build.
    ///

    /// A magic number used to detect compatibility of navigation tile data.
    public const int DT_NAVMESH_MAGIC = 'D' << 24 | 'N' << 16 | 'A' << 8 | 'V';

    /// A version number used to detect compatibility of navigation tile data.
    public const int DT_NAVMESH_VERSION = 7;

    /// A magic number used to detect the compatibility of navigation tile states.
    public const int DT_NAVMESH_STATE_MAGIC = 'D' << 24 | 'N' << 16 | 'M' << 8 | 'S';

    /// A version number used to detect compatibility of navigation tile states.
    public const int DT_NAVMESH_STATE_VERSION = 1;

    /// @}

    /// A flag that indicates that an entity links to an external entity.
    /// (E.g. A polygon edge is a portal that links to another polygon.)
    public const ushort DT_EXT_LINK = 0x8000;

    /// A value that indicates the entity does not link to anything.
    public const uint DT_NULL_LINK = 0xffffffff;

    /// A flag that indicates that an off-mesh connection can be traversed in both directions. (Is bidirectional.)
    public const uint DT_OFFMESH_CON_BIDIR = 1;

    /// The maximum number of user defined area ids.
    /// @ingroup detour
    public const int DT_MAX_AREAS = 64;

    const ushort MESH_NULL_IDX = 0xffff;

    /// Tile flags used for various functions and fields.
    /// For an example, see dtNavMesh::addTile().
    public enum dtTileFlags {
        /// The navigation mesh owns the tile memory and is responsible for freeing it.
        DT_TILE_FREE_DATA = 0x01,
    };

    /// Vertex flags returned by dtNavMeshQuery::findStraightPath.
    public enum dtStraightPathFlags {
        DT_STRAIGHTPATH_START = 0x01,				//< The vertex is the start position in the path.
        DT_STRAIGHTPATH_END = 0x02,					//< The vertex is the end position in the path.
        DT_STRAIGHTPATH_OFFMESH_CONNECTION = 0x04,	//< The vertex is the start of an off-mesh connection.
    };

    /// Options for dtNavMeshQuery::findStraightPath.
    public enum dtStraightPathOptions {
        DT_STRAIGHTPATH_AREA_CROSSINGS = 0x01,	//< Add a vertex at every polygon edge crossing where area changes.
        DT_STRAIGHTPATH_ALL_CROSSINGS = 0x02,	//< Add a vertex at every polygon edge crossing.
    };

    /// Flags representing the type of a navigation mesh polygon.
    public enum dtPolyTypes {
        /// The polygon is a standard convex polygon that is part of the surface of the mesh.
        DT_POLYTYPE_GROUND = 0,
        /// The polygon is an off-mesh connection consisting of two vertices.
        DT_POLYTYPE_OFFMESH_CONNECTION = 1,
    };


    /// Defines a polyogn within a dtMeshTile object.
    /// @ingroup detour
    public class dtPoly {
        /// Index to first link in linked list. (Or #DT_NULL_LINK if there is no link.)
        public uint firstLink;

        /// The indices of the polygon's vertices.
        /// The actual vertices are located in dtMeshTile::verts.
        public ushort[] verts = new ushort[DT_VERTS_PER_POLYGON];

        /// Packed data representing neighbor polygons references and flags for each edge.
        /*
        @var ushort dtPoly::neis[DT_VERTS_PER_POLYGON]
        @par

        Each entry represents data for the edge starting at the vertex of the same index. 
        E.g. The entry at index n represents the edge data for vertex[n] to vertex[n+1].

        A value of zero indicates the edge has no polygon connection. (It makes up the 
        border of the navigation mesh.)

        The information can be extracted as follows: 
        @code 
        neighborRef = neis[n] & 0xff; // Get the neighbor polygon reference.

        if (neis[n] & #DT_EX_LINK)
        {
            // The edge is an external (portal) edge.
        }
        @endcode
         * */
        public ushort[] neis = new ushort[DT_VERTS_PER_POLYGON];

        /// The user defined polygon flags.
        public ushort flags;

        /// The number of vertices in the polygon.
        public byte vertCount;

        /// The bit packed area id and polygon type.
        /// @note Use the structure's set and get methods to acess this value.
        public byte areaAndtype;

        public int FromBytes(byte[] array, int start) {
            firstLink = BitConverter.ToUInt32(array, start); start += sizeof(uint);
            for (int i = 0; i < DT_VERTS_PER_POLYGON; ++i) {
                verts[i] = BitConverter.ToUInt16(array, start); start += sizeof(ushort);
            }
            for (int i = 0; i < DT_VERTS_PER_POLYGON; ++i) {
                neis[i] = BitConverter.ToUInt16(array, start); start += sizeof(ushort);
            }
            flags = BitConverter.ToUInt16(array, start); start += sizeof(ushort);
            vertCount = array[start]; start += sizeof(byte);
            areaAndtype = array[start]; start += sizeof(byte);
            return start;
        }

        public byte[] ToBytes() {
            List<byte> bytes = new List<byte>();

            bytes.AddRange(BitConverter.GetBytes(firstLink));
            for (int i = 0; i < DT_VERTS_PER_POLYGON; ++i) {
                bytes.AddRange(BitConverter.GetBytes(verts[i]));
            }
            for (int i = 0; i < DT_VERTS_PER_POLYGON; ++i) {
                bytes.AddRange(BitConverter.GetBytes(neis[i]));
            }
            bytes.AddRange(BitConverter.GetBytes(flags));
            bytes.Add(vertCount);
            bytes.Add(areaAndtype);

            return bytes.ToArray();
        }
        /// Sets the user defined area id. [Limit: < #DT_MAX_AREAS]
        public void setArea(byte a) {
            //Bitwise operators are done on ints in C#
            areaAndtype = (byte)(((int)areaAndtype & 0xc0) | ((int)a & 0x3f));
        }

        /// Sets the polygon type. (See: #dtPolyTypes.)
        public void setType(byte t) {
            areaAndtype = (byte)(((int)areaAndtype & 0x3f) | (t << 6));
        }

        /// Gets the user defined area id.
        public byte getArea() {
            return (byte)((int)areaAndtype & 0x3f);
        }

        /// Gets the polygon type. (See: #dtPolyTypes)
        public byte getType() {
            return (byte)((int)areaAndtype >> 6);
        }
    };

    /// Defines the location of detail sub-mesh data within a dtMeshTile.
    public class dtPolyDetail {
        public uint vertBase;			//< The offset of the vertices in the dtMeshTile::detailVerts array.
        public uint triBase;			//< The offset of the triangles in the dtMeshTile::detailTris array.
        public byte vertCount;		//< The number of vertices in the sub-mesh.
        public byte triCount;			//< The number of triangles in the sub-mesh.

        public int FromBytes(byte[] array, int start) {
            vertBase = BitConverter.ToUInt32(array, start); start += sizeof(uint);
            triBase = BitConverter.ToUInt32(array, start); start += sizeof(uint);
            vertCount = array[start]; start += sizeof(byte);
            triCount = array[start]; start += sizeof(byte);
            return start;
        }

        public byte[] ToBytes() {
            List<byte> bytes = new List<byte>();

            bytes.AddRange(BitConverter.GetBytes(vertBase));
            bytes.AddRange(BitConverter.GetBytes(triBase));
            bytes.Add(vertCount);
            bytes.Add(triCount);

            return bytes.ToArray();
        }
    };

    /// Defines a link between polygons.
    /// @note This structure is rarely if ever used by the end user.
    /// @see dtMeshTile
    public class dtLink {
        public dtPolyRef polyRef;					//< Neighbour reference. (The neighbor that is linked to.)
        public uint next;				//< Index of the next link.
        public byte edge;				//< Index of the polygon edge that owns this link.
        public byte side;				//< If a boundary link, defines on which side the link is.
        public byte bmin;				//< If a boundary link, defines the minimum sub-edge area.
        public byte bmax;				//< If a boundary link, defines the maximum sub-edge area.

        public int FromBytes(byte[] array, int start) {
            polyRef = BitConverter.ToUInt32(array, start); start += sizeof(uint);
            next = BitConverter.ToUInt32(array, start); start += sizeof(uint);
            edge = array[start]; start += sizeof(byte);
            side = array[start]; start += sizeof(byte);
            bmin = array[start]; start += sizeof(byte);
            bmax = array[start]; start += sizeof(byte);
            return start;
        }

        public byte[] ToBytes() {
            List<byte> bytes = new List<byte>();

            bytes.AddRange(BitConverter.GetBytes(polyRef));
            bytes.AddRange(BitConverter.GetBytes(next));
            bytes.Add(edge);
            bytes.Add(side);
            bytes.Add(bmin);
            bytes.Add(bmax);

            return bytes.ToArray();
        }
    };

    /// Bounding volume node.
    /// @note This structure is rarely if ever used by the end user.
    /// @see dtMeshTile
    public class dtBVNode {
        public ushort[] bmin = new ushort[3];			//< Minimum bounds of the node's AABB. [(x, y, z)]
        public ushort[] bmax = new ushort[3];			//< Maximum bounds of the node's AABB. [(x, y, z)]
        public int i;							//< The node's index. (Negative for escape sequence.)

        public int FromBytes(byte[] array, int start) {
            for (int j = 0; j < bmin.Length; ++j) {
                bmin[j] = BitConverter.ToUInt16(array, start); start += sizeof(ushort);
            }
            for (int j = 0; j < bmax.Length; ++j) {
                bmax[j] = BitConverter.ToUInt16(array, start); start += sizeof(ushort);
            }
            i = BitConverter.ToInt32(array, start); start += sizeof(int);
            return start;
        }

        public byte[] ToBytes() {
            List<byte> bytes = new List<byte>();
            for (int j = 0; j < bmin.Length; ++j) {
                bytes.AddRange(BitConverter.GetBytes(bmin[j]));
            }
            for (int j = 0; j < bmax.Length; ++j) {
                bytes.AddRange(BitConverter.GetBytes(bmax[j]));
            }
            bytes.AddRange(BitConverter.GetBytes(i));

            return bytes.ToArray();
        }
    };

    /// Defines an navigation mesh off-mesh connection within a dtMeshTile object.
    /// An off-mesh connection is a user defined traversable connection made up to two vertices.
    public class dtOffMeshConnection {
        /// The endpoints of the connection. [(ax, ay, az, bx, by, bz)]
        public float[] pos = new float[6];

        /// The radius of the endpoints. [Limit: >= 0]
        public float rad;

        /// The polygon reference of the connection within the tile.
        public ushort poly;

        /// Link flags. 
        /// @note These are not the connection's user defined flags. Those are assigned via the 
        /// connection's dtPoly definition. These are link flags used for internal purposes.
        public byte flags;

        /// End point side.
        public byte side;

        /// The id of the offmesh connection. (User assigned when the navigation mesh is built.)
        public uint userId;


        public int FromBytes(byte[] array, int start) {
            for (int i = 0; i < 6; ++i) {
                pos[i] = BitConverter.ToSingle(array, start); start += sizeof(float);
            }
            rad = BitConverter.ToSingle(array, start); start += sizeof(float);
            poly = BitConverter.ToUInt16(array, start); start += sizeof(ushort);
            flags = array[start]; ++start;
            side = array[start]; ++start;
            userId = BitConverter.ToUInt32(array, start); start += sizeof(uint);
            return start;
        }

        public byte[] ToBytes() {
            List<byte> bytes = new List<byte>();
            for (int i = 0; i < 6; ++i) {
                bytes.AddRange(BitConverter.GetBytes(pos[i]));
            }
            bytes.AddRange(BitConverter.GetBytes(rad));
            bytes.AddRange(BitConverter.GetBytes(poly));
            bytes.Add(flags);
            bytes.Add(side);
            bytes.AddRange(BitConverter.GetBytes(userId));

            return bytes.ToArray();
        }
    };



    /// Provides high level information related to a dtMeshTile object.
    /// @ingroup detour
    public class dtMeshHeader {
        public int magic;				//< Tile magic number. (Used to identify the data format.)
        public int version;			//< Tile data format version number.
        public int x;					//< The x-position of the tile within the dtNavMesh tile grid. (x, y, layer)
        public int y;					//< The y-position of the tile within the dtNavMesh tile grid. (x, y, layer)
        public int layer;				//< The layer of the tile within the dtNavMesh tile grid. (x, y, layer)
        public uint userId;	//< The user defined id of the tile.
        public int polyCount;			//< The number of polygons in the tile.
        public int vertCount;			//< The number of vertices in the tile.
        public int maxLinkCount;		//< The number of allocated links.
        public int detailMeshCount;	//< The number of sub-meshes in the detail mesh.

        /// The number of unique vertices in the detail mesh. (In addition to the polygon vertices.)
        public int detailVertCount;

        public int detailTriCount;			//< The number of triangles in the detail mesh.
        public int bvNodeCount;			//< The number of bounding volume nodes. (Zero if bounding volumes are disabled.)
        public int offMeshConCount;		//< The number of off-mesh connections.
        public int offMeshBase;			//< The index of the first polygon which is an off-mesh connection.
        public float walkableHeight;		//< The height of the agents using the tile.
        public float walkableRadius;		//< The radius of the agents using the tile.
        public float walkableClimb;		//< The maximum climb height of the agents using the tile.
        public float[] bmin = new float[3];				//< The minimum bounds of the tile's AABB. [(x, y, z)]
        public float[] bmax = new float[3];				//< The maximum bounds of the tile's AABB. [(x, y, z)]

        /// The bounding volume quantization factor. 
        /*
        @var float dtMeshHeader::bvQuantFactor
        @par

        This value is used for converting between world and bounding volume coordinates.
        For example:
        @code
        const float cs = 1.0f / tile.header.bvQuantFactor;
        const dtBVNode* n = &tile.bvTree[i];
        if (n.i >= 0)
        {
            // This is a leaf node.
            float worldMinX = tile.header.bmin[0] + n.bmin[0]*cs;
            float worldMinY = tile.header.bmin[0] + n.bmin[1]*cs;
            // Etc...
        }
        @endcode */
        public float bvQuantFactor;

        public int FromBytes(byte[] array, int start) {
            magic = BitConverter.ToInt32(array, start); start += sizeof(int);
            version = BitConverter.ToInt32(array, start); start += sizeof(int);
            x = BitConverter.ToInt32(array, start); start += sizeof(int);
            y = BitConverter.ToInt32(array, start); start += sizeof(int);
            layer = BitConverter.ToInt32(array, start); start += sizeof(int);
            userId = BitConverter.ToUInt32(array, start); start += sizeof(uint);
            polyCount = BitConverter.ToInt32(array, start); start += sizeof(int);
            vertCount = BitConverter.ToInt32(array, start); start += sizeof(int);
            maxLinkCount = BitConverter.ToInt32(array, start); start += sizeof(int);
            detailMeshCount = BitConverter.ToInt32(array, start); start += sizeof(int);

            detailVertCount = BitConverter.ToInt32(array, start); start += sizeof(int);

            detailTriCount = BitConverter.ToInt32(array, start); start += sizeof(int);
            bvNodeCount = BitConverter.ToInt32(array, start); start += sizeof(int);
            offMeshConCount = BitConverter.ToInt32(array, start); start += sizeof(int);
            offMeshBase = BitConverter.ToInt32(array, start); start += sizeof(int);
            walkableHeight = BitConverter.ToSingle(array, start); start += sizeof(float);
            walkableRadius = BitConverter.ToSingle(array, start); start += sizeof(float);
            walkableClimb = BitConverter.ToSingle(array, start); start += sizeof(float);

            for (int i = 0; i < 3; ++i) {
                bmin[i] = BitConverter.ToSingle(array, start); start += sizeof(float);
            }
            for (int i = 0; i < 3; ++i) {
                bmax[i] = BitConverter.ToSingle(array, start); start += sizeof(float);
            }

            bvQuantFactor = BitConverter.ToSingle(array, start); start += sizeof(float);
            return start;
        }

        public byte[] ToBytes() {
            List<byte> bytes = new List<byte>();

            bytes.AddRange(BitConverter.GetBytes(magic));
            bytes.AddRange(BitConverter.GetBytes(version));
            bytes.AddRange(BitConverter.GetBytes(x));
            bytes.AddRange(BitConverter.GetBytes(y));
            bytes.AddRange(BitConverter.GetBytes(layer));
            bytes.AddRange(BitConverter.GetBytes(userId));
            bytes.AddRange(BitConverter.GetBytes(polyCount));
            bytes.AddRange(BitConverter.GetBytes(vertCount));
            bytes.AddRange(BitConverter.GetBytes(maxLinkCount));
            bytes.AddRange(BitConverter.GetBytes(detailMeshCount));

            bytes.AddRange(BitConverter.GetBytes(detailVertCount));

            bytes.AddRange(BitConverter.GetBytes(detailTriCount));
            bytes.AddRange(BitConverter.GetBytes(bvNodeCount));
            bytes.AddRange(BitConverter.GetBytes(offMeshConCount));
            bytes.AddRange(BitConverter.GetBytes(offMeshBase));
            bytes.AddRange(BitConverter.GetBytes(walkableHeight));
            bytes.AddRange(BitConverter.GetBytes(walkableRadius));
            bytes.AddRange(BitConverter.GetBytes(walkableClimb));
            for (int i = 0; i < 3; ++i) {
                bytes.AddRange(BitConverter.GetBytes(bmin[i]));
            }
            for (int i = 0; i < 3; ++i) {
                bytes.AddRange(BitConverter.GetBytes(bmax[i]));
            }
            bytes.AddRange(BitConverter.GetBytes(bvQuantFactor));
            return bytes.ToArray();
        }
    };


    public class dtRawTileData {
        public dtMeshHeader header;				    //< The tile header.
        public dtPoly[] polys;						//< The tile polygons. [Size: dtMeshHeader::polyCount]
        public float[] verts;						//< The tile vertices. [Size: dtMeshHeader::vertCount]
        public dtLink[] links;						//< The tile links. [Size: dtMeshHeader::maxLinkCount]
        public dtPolyDetail[] detailMeshes;			//< The tile's detail sub-meshes. [Size: dtMeshHeader::detailMeshCount]

        /// The detail mesh's unique vertices. [(x, y, z) * dtMeshHeader::detailVertCount]
        public float[] detailVerts;

        /// The detail mesh's triangles. [(vertA, vertB, vertC) * dtMeshHeader::detailTriCount]
        public byte[] detailTris;

        /// The tile bounding volume nodes. [Size: dtMeshHeader::bvNodeCount]
        /// (Will be null if bounding volumes are disabled.)
        public dtBVNode[] bvTree;

        public dtOffMeshConnection[] offMeshCons;		//< The tile off-mesh connections. [Size: dtMeshHeader::offMeshConCount]

        public int flags;								//< Tile flags. (See: #dtTileFlags)

        public int FromBytes(byte[] array, int start) {
            header = new dtMeshHeader();
            start = header.FromBytes(array, start);

            int count = header.polyCount;
            polys = new dtPoly[count];
            for (int i = 0; i < count; ++i) {
                polys[i] = new dtPoly();
                start = polys[i].FromBytes(array, start);
            }
            count = header.vertCount * 3;
            verts = new float[count];
            for (int i = 0; i < count; ++i) {
                verts[i] = BitConverter.ToSingle(array, start);
                start += sizeof(float);
            }
            count = header.maxLinkCount;
            links = new dtLink[count];
            for (int i = 0; i < count; ++i) {
                links[i] = new dtLink();
                start = links[i].FromBytes(array, start);
            }
            count = header.detailMeshCount;
            detailMeshes = new dtPolyDetail[count];
            for (int i = 0; i < count; ++i) {
                detailMeshes[i] = new dtPolyDetail();
                start = detailMeshes[i].FromBytes(array, start);
            }
            count = header.detailVertCount * 3;
            detailVerts = new float[count];
            for (int i = 0; i < count; ++i) {
                detailVerts[i] = BitConverter.ToSingle(array, start);
                start += sizeof(float);
            }
            count = header.detailTriCount * 4;
            detailTris = new byte[count];
            for (int i = 0; i < count; ++i) {
                detailTris[i] = array[start + i];
            }
            start += count;
            count = header.bvNodeCount;
            bvTree = new dtBVNode[count];
            for (int i = 0; i < count; ++i) {
                bvTree[i] = new dtBVNode();
                start = bvTree[i].FromBytes(array, start);
            }
            count = header.offMeshConCount;
            offMeshCons = new dtOffMeshConnection[count];
            for (int i = 0; i < count; ++i) {
                offMeshCons[i] = new dtOffMeshConnection();
                start = offMeshCons[i].FromBytes(array, start);
            }
            flags = BitConverter.ToInt32(array, start);
            start += sizeof(int);
            return start;
        }

        public byte[] ToBytes() {
            List<byte> bytes = new List<byte>();

            bytes.AddRange(header.ToBytes());
            for (int i = 0; i < polys.Length; ++i) {
                bytes.AddRange(polys[i].ToBytes());
            }
            for (int i = 0; i < verts.Length; ++i) {
                bytes.AddRange(BitConverter.GetBytes(verts[i]));
            }
            for (int i = 0; i < links.Length; ++i) {
                bytes.AddRange(links[i].ToBytes());
            }
            for (int i = 0; i < detailMeshes.Length; ++i) {
                bytes.AddRange(detailMeshes[i].ToBytes());
            }
            for (int i = 0; i < detailVerts.Length; ++i) {
                bytes.AddRange(BitConverter.GetBytes(detailVerts[i]));
            }
            for (int i = 0; i < detailTris.Length; ++i) {
                bytes.Add(detailTris[i]);
            }
            for (int i = 0; i < bvTree.Length; ++i) {
                bytes.AddRange(bvTree[i].ToBytes());
            }
            for (int i = 0; i < offMeshCons.Length; ++i) {
                bytes.AddRange(offMeshCons[i].ToBytes());
            }
            bytes.AddRange(BitConverter.GetBytes(flags));

            return bytes.ToArray();
        }
    }
    /// Defines a navigation mesh tile.
    /// @ingroup detour
    /* 
    @struct dtMeshTile
    @par

    Tiles generally only exist within the context of a dtNavMesh object.

    Some tile content is optional.  For example, a tile may not contain any
    off-mesh connections.  In this case the associated pointer will be null.

    If a detail mesh exists it will share vertices with the base polygon mesh.  
    Only the vertices unique to the detail mesh will be stored in #detailVerts.

    @warning Tiles returned by a dtNavMesh object are not guarenteed to be populated.
    For example: The tile at a location might not have been loaded yet, or may have been removed.
    In this case, pointers will be null.  So if in doubt, check the polygon count in the 
    tile's header to determine if a tile has polygons defined.

    @var float dtOffMeshConnection::pos[6]
    @par

    For a properly built navigation mesh, vertex A will always be within the bounds of the mesh. 
    Vertex B is not required to be within the bounds of the mesh.

    */
    public class dtMeshTile {
        public uint salt;					//< Counter describing modifications to the tile.

        public uint linksFreeList;			//< Index to the next free link.
        public dtMeshHeader header;				//< The tile header.
        public dtPoly[] polys;						//< The tile polygons. [Size: dtMeshHeader::polyCount]
        public float[] verts;						//< The tile vertices. [Size: dtMeshHeader::vertCount]
        public dtLink[] links;						//< The tile links. [Size: dtMeshHeader::maxLinkCount]
        public dtPolyDetail[] detailMeshes;			//< The tile's detail sub-meshes. [Size: dtMeshHeader::detailMeshCount]

        /// The detail mesh's unique vertices. [(x, y, z) * dtMeshHeader::detailVertCount]
        public float[] detailVerts;

        /// The detail mesh's triangles. [(vertA, vertB, vertC) * dtMeshHeader::detailTriCount]
        public byte[] detailTris;

        /// The tile bounding volume nodes. [Size: dtMeshHeader::bvNodeCount]
        /// (Will be null if bounding volumes are disabled.)
        public dtBVNode[] bvTree;

        public dtOffMeshConnection[] offMeshCons;		//< The tile off-mesh connections. [Size: dtMeshHeader::offMeshConCount]

        public dtRawTileData data;					//< The tile data. (Not directly accessed under normal situations.)
        public int dataSize;							//< Size of the tile data.
        public int flags;								//< Tile flags. (See: #dtTileFlags)
        public dtMeshTile next;						//< The next free tile, or the next tile in the spatial grid.
    };

    /// Configuration parameters used to define multi-tile navigation meshes.
    /// The values are used to allocate space during the initialization of a navigation mesh.
    /// @see dtNavMesh::init()
    /// @ingroup detour
    public class dtNavMeshParams {
        public float[] orig = new float[3];					//< The world space origin of the navigation mesh's tile space. [(x, y, z)]
        public float tileWidth;				//< The width of each tile. (Along the x-axis.)
        public float tileHeight;				//< The height of each tile. (Along the z-axis.)
        public int maxTiles;					//< The maximum number of tiles the navigation mesh can contain.
        public int maxPolys;					//< The maximum number of polygons each tile can contain.
        ///
        public dtNavMeshParams Clone() {
            dtNavMeshParams copy = new dtNavMeshParams();
            for (int i = 0; i < orig.Length; ++i) {
                copy.orig[i] = orig[i];
            }
            copy.tileWidth = tileWidth;
            copy.tileHeight = tileHeight;
            copy.maxTiles = maxTiles;
            copy.maxPolys = maxPolys;
            return copy;
        }
    };



    /// Represents the source data used to build an navigation mesh tile.
    /// @ingroup detour
    /**
    @struct dtNavMeshCreateParams
    @par

    This structure is used to marshal data between the Recast mesh generation pipeline and Detour navigation components.

    See the rcPolyMesh and rcPolyMeshDetail documentation for detailed information related to mesh structure.

    Units are usually in voxels (vx) or world units (wu). The units for voxels, grid size, and cell size 
    are all based on the values of #cs and #ch.

    The standard navigation mesh build process is to create tile data using dtCreateNavMeshData, then add the tile 
    to a navigation mesh using either the dtNavMesh single tile <tt>init()</tt> function or the dtNavMesh::addTile()
    function.

    @see dtCreateNavMeshData

    */
    public class dtNavMeshCreateParams {

        /// @name Polygon Mesh Attributes
        /// Used to create the base navigation graph.
        /// See #rcPolyMesh for details related to these attributes.
        /// @{

        public ushort[] verts;			//< The polygon mesh vertices. [(x, y, z) * #vertCount] [Unit: vx]
        public int vertCount;							//< The number vertices in the polygon mesh. [Limit: >= 3]
        public ushort[] polys;			//< The polygon data. [Size: #polyCount * 2 * #nvp]
        public ushort[] polyFlags;		//< The user defined flags assigned to each polygon. [Size: #polyCount]
        public byte[] polyAreas;			//< The user defined area ids assigned to each polygon. [Size: #polyCount]
        public int polyCount;							//< Number of polygons in the mesh. [Limit: >= 1]
        public int nvp;								//< Number maximum number of vertices per polygon. [Limit: >= 3]

        /// @}
        /// @name Height Detail Attributes (Optional)
        /// See #rcPolyMeshDetail for details related to these attributes.
        /// @{

        public uint[] detailMeshes;		//< The height detail sub-mesh data. [Size: 4 * #polyCount]
        public float[] detailVerts;				//< The detail mesh vertices. [Size: 3 * #detailVertsCount] [Unit: wu]
        public int detailVertsCount;					//< The number of vertices in the detail mesh.
        public byte[] detailTris;		//< The detail mesh triangles. [Size: 4 * #detailTriCount]
        public int detailTriCount;						//< The number of triangles in the detail mesh.

        /// @}
        /// @name Off-Mesh Connections Attributes (Optional)
        /// Used to define a custom point-to-point edge within the navigation graph, an 
        /// off-mesh connection is a user defined traversable connection made up to two vertices, 
        /// at least one of which resides within a navigation mesh polygon.
        /// @{

        /// Off-mesh connection vertices. [(ax, ay, az, bx, by, bz) * #offMeshConCount] [Unit: wu]
        public float[] offMeshConVerts;
        /// Off-mesh connection radii. [Size: #offMeshConCount] [Unit: wu]
        public float[] offMeshConRad;
        /// User defined flags assigned to the off-mesh connections. [Size: #offMeshConCount]
        public ushort[] offMeshConFlags;
        /// User defined area ids assigned to the off-mesh connections. [Size: #offMeshConCount]
        public byte[] offMeshConAreas;
        /// The permitted travel direction of the off-mesh connections. [Size: #offMeshConCount]
        ///
        /// 0 = Travel only from endpoint A to endpoint B.<br/>
        /// #DT_OFFMESH_CON_BIDIR = Bidirectional travel.
        public byte[] offMeshConDir;
        /// The user defined ids of the off-mesh connection. [Size: #offMeshConCount]
        public uint[] offMeshConUserID;
        /// The number of off-mesh connections. [Limit: >= 0]
        public int offMeshConCount;

        /// @}
        /// @name Tile Attributes
        /// @note The tile grid/layer data can be left at zero if the destination is a single tile mesh.
        /// @{

        public uint userId;	//< The user defined id of the tile.
        public int tileX;				//< The tile's x-grid location within the multi-tile destination mesh. (Along the x-axis.)
        public int tileY;				//< The tile's y-grid location within the multi-tile desitation mesh. (Along the z-axis.)
        public int tileLayer;			//< The tile's layer within the layered destination mesh. [Limit: >= 0] (Along the y-axis.)
        public float[] bmin = new float[3];			//< The minimum bounds of the tile. [(x, y, z)] [Unit: wu]
        public float[] bmax = new float[3];			//< The maximum bounds of the tile. [(x, y, z)] [Unit: wu]

        /// @}
        /// @name General Configuration Attributes
        /// @{

        public float walkableHeight;	//< The agent height. [Unit: wu]
        public float walkableRadius;	//< The agent radius. [Unit: wu]
        public float walkableClimb;	//< The agent maximum traversable ledge. (Up/Down) [Unit: wu]
        public float cs;				//< The xz-plane cell size of the polygon mesh. [Limit: > 0] [Unit: wu]
        public float ch;				//< The y-axis cell height of the polygon mesh. [Limit: > 0] [Unit: wu]

        /// True if a bounding volume tree should be built for the tile.
        /// @note The BVTree is not normally needed for layered navigation meshes.
        public bool buildBvTree;

        /// @}
    }

    public class BVItem
    {
	    public ushort[] bmin = new ushort[3];
	    public ushort[] bmax = new ushort[3];
	    public int i;
    };

    //public static int compareItemX(const void* va, const void* vb)
    public class BVItemCompareX : IComparer<BVItem>
    {
        // Compares by Height, Length, and Width. 
        public int Compare(BVItem a, BVItem b)
        {
	        if (a.bmin[0] < b.bmin[0])
		        return -1;
	        if (a.bmin[0] > b.bmin[0])
		        return 1;
	        return 0;
        }
    }

    //static int compareItemY(const void* va, const void* vb)
    public class BVItemCompareY : IComparer<BVItem>
    {
        public int Compare(BVItem a, BVItem b)
        {
	        if (a.bmin[1] < b.bmin[1])
		        return -1;
	        if (a.bmin[1] > b.bmin[1])
		        return 1;
	        return 0;
        }
    }
    public class BVItemCompareZ : IComparer<BVItem>
    {
        public int Compare(BVItem a, BVItem b)
        {
	        if (a.bmin[2] < b.bmin[2])
		        return -1;
	        if (a.bmin[2] > b.bmin[2])
		        return 1;
	        return 0;
        }
    }

    static void calcExtends(BVItem[] items, int nitems, int imin, int imax,
						    ushort[] bmin, ushort[] bmax)
    {
	    bmin[0] = items[imin].bmin[0];
	    bmin[1] = items[imin].bmin[1];
	    bmin[2] = items[imin].bmin[2];
	
	    bmax[0] = items[imin].bmax[0];
	    bmax[1] = items[imin].bmax[1];
	    bmax[2] = items[imin].bmax[2];
	
	    for (int i = imin+1; i < imax; ++i)
	    {
		    //const BVItem& it = items[i];
            BVItem it = items[i];
		    if (it.bmin[0] < bmin[0]) bmin[0] = it.bmin[0];
		    if (it.bmin[1] < bmin[1]) bmin[1] = it.bmin[1];
		    if (it.bmin[2] < bmin[2]) bmin[2] = it.bmin[2];
		
		    if (it.bmax[0] > bmax[0]) bmax[0] = it.bmax[0];
		    if (it.bmax[1] > bmax[1]) bmax[1] = it.bmax[1];
		    if (it.bmax[2] > bmax[2]) bmax[2] = it.bmax[2];
	    }
    }

    public static int longestAxis(ushort x, ushort y, ushort z)
    {
	    int	axis = 0;
	    ushort maxVal = x;
	    if (y > maxVal)
	    {
		    axis = 1;
		    maxVal = y;
	    }
	    if (z > maxVal)
	    {
		    axis = 2;
		    maxVal = z;
	    }
	    return axis;
    }

    public static void subdivide(BVItem[] items, int nitems, int imin, int imax, ref int curNode, dtBVNode[] nodes)
    {
	    int inum = imax - imin;
	    int icur = curNode;
	
	    dtBVNode node = nodes[curNode++];
	
	    if (inum == 1)
	    {
		    // Leaf
		    node.bmin[0] = items[imin].bmin[0];
		    node.bmin[1] = items[imin].bmin[1];
		    node.bmin[2] = items[imin].bmin[2];
		
		    node.bmax[0] = items[imin].bmax[0];
		    node.bmax[1] = items[imin].bmax[1];
		    node.bmax[2] = items[imin].bmax[2];
		
		    node.i = items[imin].i;
	    }
	    else
	    {
		    // Split
		    calcExtends(items, nitems, imin, imax, node.bmin, node.bmax);
		
		    int	axis = longestAxis((ushort)(node.bmax[0] - node.bmin[0]),
							       (ushort)(node.bmax[1] - node.bmin[1]),
							       (ushort)(node.bmax[2] - node.bmin[2]));
		
		    if (axis == 0)
		    {
			    // Sort along x-axis
			    //qsort(items+imin, inum, sizeof(BVItem), compareItemX);
                Array.Sort(items,imin,inum, new BVItemCompareX());
		    }
		    else if (axis == 1)
		    {
			    // Sort along y-axis
			    //qsort(items+imin, inum, sizeof(BVItem), compareItemY);
                Array.Sort(items,imin,inum, new BVItemCompareY());
		    }
		    else
		    {
			    // Sort along z-axis
			    //qsort(items+imin, inum, sizeof(BVItem), compareItemZ);
                Array.Sort(items,imin,inum, new BVItemCompareZ());
		    }
		
		    int isplit = imin+inum/2;
		
		    // Left
		    subdivide(items, nitems, imin, isplit,ref curNode, nodes);
		    // Right
		    subdivide(items, nitems, isplit, imax,ref curNode, nodes);
		
		    int iescape = curNode - icur;
		    // Negative index means escape.
		    node.i = -iescape;
	    }
    }

    public static int createBVTree(ushort[] verts, int nverts,
						    ushort[] polys, int npolys, int nvp,
						    float cs, float ch,
						    int nnodes, dtBVNode[] nodes)
    {
	    // Build tree
        BVItem[] items = new BVItem[npolys];//(BVItem*)dtAlloc(sizeof(BVItem)*npolys, DT_ALLOC_TEMP);
        dtcsArrayItemsCreate(items);
	    for (int i = 0; i < npolys; i++)
	    {
		    BVItem it = items[i];
		    it.i = i;
		    // Calc polygon bounds.
		    //const ushort* p = &polys[i*nvp*2];
            int pIndex = i*nvp*2;
		    it.bmin[0] = it.bmax[0] = verts[polys[pIndex + 0]*3+0];
		    it.bmin[1] = it.bmax[1] = verts[polys[pIndex + 0]*3+1];
		    it.bmin[2] = it.bmax[2] = verts[polys[pIndex + 0]*3+2];
		
		    for (int j = 1; j < nvp; ++j)
		    {
                if (polys[pIndex + j] == MESH_NULL_IDX) break;
			    ushort x = verts[polys[pIndex + j]*3+0];
			    ushort y = verts[polys[pIndex + j]*3+1];
			    ushort z = verts[polys[pIndex + j]*3+2];
			
			    if (x < it.bmin[0]) it.bmin[0] = x;
			    if (y < it.bmin[1]) it.bmin[1] = y;
			    if (z < it.bmin[2]) it.bmin[2] = z;
			
			    if (x > it.bmax[0]) it.bmax[0] = x;
			    if (y > it.bmax[1]) it.bmax[1] = y;
			    if (z > it.bmax[2]) it.bmax[2] = z;
		    }
		    // Remap y
		    it.bmin[1] = (ushort)Math.Floor((float)it.bmin[1]*ch/cs);
		    it.bmax[1] = (ushort)Math.Ceiling((float)it.bmax[1]*ch/cs);
	    }
	
	    int curNode = 0;
	    subdivide(items, npolys, 0, npolys, ref curNode, nodes);
	
	    //dtFree(items);
	
	    return curNode;
    }

    public static byte classifyOffMeshPoint(float[] pt, int ptStart, float[] bmin, float[] bmax)
    {
	    const byte XP = 1<<0;
	    const byte ZP = 1<<1;
	    const byte XM = 1<<2;
	    const byte ZM = 1<<3;	

	    byte outcode = 0; 
	    outcode |= (pt[ptStart + 0] >= bmax[0]) ? XP : (byte)0;
	    outcode |= (pt[ptStart + 2] >= bmax[2]) ? ZP : (byte)0;
	    outcode |= (pt[ptStart + 0] < bmin[0])  ? XM : (byte)0;
	    outcode |= (pt[ptStart + 2] < bmin[2])  ? ZM : (byte)0;

	    switch (outcode)
	    {
	    case XP: return 0;
	    case XP|ZP: return 1;
	    case ZP: return 2;
	    case XM|ZP: return 3;
	    case XM: return 4;
	    case XM|ZM: return 5;
	    case ZM: return 6;
	    case XP|ZM: return 7;
	    };

	    return 0xff;	
    }

    // TODO: Better error handling.

    /// @par
    /// 
    /// The output data array is allocated using the detour allocator (dtAlloc()).  The method
    /// used to free the memory will be determined by how the tile is added to the navigation
    /// mesh.
    ///
    /// @see dtNavMesh, dtNavMesh::addTile()
    /// Builds navigation mesh tile data from the provided tile creation data.
    /// @ingroup detour
    ///  @param[in]		params		Tile creation data.
    ///  @param[out]	outData		The resulting tile data.
    ///  @param[out]	outDataSize	The size of the tile data array.
    /// @return True if the tile data was successfully created.
    public static bool dtCreateNavMeshData(dtNavMeshCreateParams createParams, out dtRawTileData outTile)//ref byte[] outData, ref int outDataSize)
    {
        outTile = null;

	    if (createParams.nvp > DT_VERTS_PER_POLYGON)
		    return false;
	    if (createParams.vertCount >= 0xffff)
		    return false;
	    if (createParams.vertCount == 0 || createParams.verts == null)
		    return false;
	    if (createParams.polyCount == 0 || createParams.polys == null)
		    return false;

	    int nvp = createParams.nvp;
	
	    // Classify off-mesh connection points. We store only the connections
	    // whose start point is inside the tile.
	    byte[] offMeshConClass = null;
	    int storedOffMeshConCount = 0;
	    int offMeshConLinkCount = 0;
	
	    if (createParams.offMeshConCount > 0)
	    {
		    //offMeshConClass = (byte*)dtAlloc(sizeof(byte)*createParams.offMeshConCount*2, DT_ALLOC_TEMP);
            offMeshConClass = new byte[createParams.offMeshConCount*2];
		    if (offMeshConClass == null)
			    return false;

		    // Find tight heigh bounds, used for culling out off-mesh start locations.
		    float hmin = float.MaxValue;
		    float hmax = -float.MaxValue;
		
		    if (createParams.detailVerts != null && createParams.detailVertsCount != 0)
		    {
			    for (int i = 0; i < createParams.detailVertsCount; ++i)
			    {
				    float h = createParams.detailVerts[i*3+1];
				    hmin = Math.Min(hmin,h);
				    hmax = Math.Max(hmax,h);
			    }
		    }
		    else
		    {
			    for (int i = 0; i < createParams.vertCount; ++i)
			    {
				    //ushort* iv = &createParams.verts[i*3];
				    float h = createParams.bmin[1] + createParams.verts[i*3 + 1] * createParams.ch;
				    hmin = Math.Min(hmin,h);
				    hmax = Math.Max(hmax,h);
			    }
		    }
		    hmin -= createParams.walkableClimb;
		    hmax += createParams.walkableClimb;
		    float[] bmin = new float[3];//, bmax[3];
            float[] bmax = new float[3];
		    dtVcopy(bmin, createParams.bmin);
		    dtVcopy(bmax, createParams.bmax);
		    bmin[1] = hmin;
		    bmax[1] = hmax;

		    for (int i = 0; i < createParams.offMeshConCount; ++i)
		    {
			    //const float* p0 = &createParams.offMeshConVerts[(i*2+0)*3];
			    //const float* p1 = &createParams.offMeshConVerts[(i*2+1)*3];
                int p0Start = (i*2+0)*3;
                int p1Start = (i*2+1)*3;
			    offMeshConClass[i*2+0] = classifyOffMeshPoint(createParams.offMeshConVerts, p0Start, bmin, bmax);
			    offMeshConClass[i*2+1] = classifyOffMeshPoint(createParams.offMeshConVerts, p1Start, bmin, bmax);

			    // Zero out off-mesh start positions which are not even potentially touching the mesh.
			    if (offMeshConClass[i*2+0] == 0xff)
			    {
				    if (createParams.offMeshConVerts[p0Start + 1] < bmin[1] || createParams.offMeshConVerts[p0Start + 1] > bmax[1]){
					    offMeshConClass[i*2+0] = 0;
                    }
			    }

			    // Cound how many links should be allocated for off-mesh connections.
			    if (offMeshConClass[i*2+0] == 0xff)
				    offMeshConLinkCount++;
			    if (offMeshConClass[i*2+1] == 0xff)
				    offMeshConLinkCount++;

			    if (offMeshConClass[i*2+0] == 0xff)
				    storedOffMeshConCount++;
		    }
	    }
	
	    // Off-mesh connectionss are stored as polygons, adjust values.
	    int totPolyCount = createParams.polyCount + storedOffMeshConCount;
	    int totVertCount = createParams.vertCount + storedOffMeshConCount*2;
	
	    // Find portal edges which are at tile borders.
	    int edgeCount = 0;
	    int portalCount = 0;
	    for (int i = 0; i < createParams.polyCount; ++i)
	    {
		    //const ushort* p = &createParams.polys[i*2*nvp];
            int pStart = i*2*nvp;
		    for (int j = 0; j < nvp; ++j)
		    {
			    if (createParams.polys[pStart + j] == MESH_NULL_IDX) break;
			    edgeCount++;
			
			    if ((createParams.polys[pStart + nvp+j] & 0x8000) != 0)
			    {
				    ushort dir = (ushort)(createParams.polys[pStart + nvp+j] & 0xf);
				    if (dir != 0xf)
					    portalCount++;
			    }
		    }
	    }

	    int maxLinkCount = edgeCount + portalCount*2 + offMeshConLinkCount*2;
	
	    // Find unique detail vertices.
	    int uniqueDetailVertCount = 0;
	    int detailTriCount = 0;
	    if (createParams.detailMeshes != null)
	    {
		    // Has detail mesh, count unique detail vertex count and use input detail tri count.
		    detailTriCount = createParams.detailTriCount;
		    for (int i = 0; i < createParams.polyCount; ++i)
		    {
			    //const ushort* p = &createParams.polys[i*nvp*2];
                int pStart = i*nvp*2;
			    int ndv = (int)createParams.detailMeshes[i*4+1];
			    int nv = 0;
			    for (int j = 0; j < nvp; ++j)
			    {
				    if (createParams.polys[pStart + j] == MESH_NULL_IDX) 
						break;
				    nv++;
			    }
			    ndv -= nv;
			    uniqueDetailVertCount += ndv;
		    }
	    }
	    else
	    {
		    // No input detail mesh, build detail mesh from nav polys.
		    uniqueDetailVertCount = 0; // No extra detail verts.
		    detailTriCount = 0;
		    for (int i = 0; i < createParams.polyCount; ++i)
		    {
			    //const ushort* p = &createParams.polys[i*nvp*2];
                int pStart = i*nvp*2;
			    int nv = 0;
			    for (int j = 0; j < nvp; ++j)
			    {
				    if (createParams.polys[pStart + j] == MESH_NULL_IDX) break;
				    nv++;
			    }
			    detailTriCount += nv-2;
		    }
	    }

        outTile = new dtRawTileData();
        /*
        	    // Calculate data size
	    const int headerSize = dtAlign4(sizeof(dtMeshHeader));
	    const int vertsSize = dtAlign4(sizeof(float)*3*totVertCount);
	    const int polysSize = dtAlign4(sizeof(dtPoly)*totPolyCount);
	    const int linksSize = dtAlign4(sizeof(dtLink)*maxLinkCount);
	    const int detailMeshesSize = dtAlign4(sizeof(dtPolyDetail)*createParams.polyCount);
	    const int detailVertsSize = dtAlign4(sizeof(float)*3*uniqueDetailVertCount);
	    const int detailTrisSize = dtAlign4(sizeof(byte)*4*detailTriCount);
	    const int bvTreeSize = createParams.buildBvTree ? dtAlign4(sizeof(dtBVNode)*createParams.polyCount*2) : 0;
	    const int offMeshConsSize = dtAlign4(sizeof(dtOffMeshConnection)*storedOffMeshConCount);
	
	    const int dataSize = headerSize + vertsSize + polysSize + linksSize +
						     detailMeshesSize + detailVertsSize + detailTrisSize +
						     bvTreeSize + offMeshConsSize;
						 
	    byte* data = (byte*)dtAlloc(sizeof(byte)*dataSize, DT_ALLOC_PERM);
	    if (!data)
	    {
		    dtFree(offMeshConClass);
		    return false;
	    }
	    memset(data, 0, dataSize);
	    
        

	    byte* d = data;
	    dtMeshHeader* header = (dtMeshHeader*)d; d += headerSize;
	    float* navVerts = (float*)d; d += vertsSize;
	    dtPoly* navPolys = (dtPoly*)d; d += polysSize;
	    d += linksSize;
	    dtPolyDetail* navDMeshes = (dtPolyDetail*)d; d += detailMeshesSize;
	    float* navDVerts = (float*)d; d += detailVertsSize;
	    byte* navDTris = (byte*)d; d += detailTrisSize;
	    dtBVNode* navBvtree = (dtBVNode*)d; d += bvTreeSize;
	    dtOffMeshConnection* offMeshCons = (dtOffMeshConnection*)d; d += offMeshConsSize;
	    */

	    outTile.header = new dtMeshHeader();
        dtMeshHeader header = outTile.header;
	    // Store header
	    header.magic = DT_NAVMESH_MAGIC;
	    header.version = DT_NAVMESH_VERSION;
	    header.x = createParams.tileX;
	    header.y = createParams.tileY;
	    header.layer = createParams.tileLayer;
	    header.userId = createParams.userId;
	    header.polyCount = totPolyCount;
	    header.vertCount = totVertCount;
	    header.maxLinkCount = maxLinkCount;
	    dtVcopy(header.bmin, createParams.bmin);
	    dtVcopy(header.bmax, createParams.bmax);
	    header.detailMeshCount = createParams.polyCount;
	    header.detailVertCount = uniqueDetailVertCount;
	    header.detailTriCount = detailTriCount;
	    header.bvQuantFactor = 1.0f / createParams.cs;
	    header.offMeshBase = createParams.polyCount;
	    header.walkableHeight = createParams.walkableHeight;
	    header.walkableRadius = createParams.walkableRadius;
	    header.walkableClimb = createParams.walkableClimb;
	    header.offMeshConCount = storedOffMeshConCount;
	    header.bvNodeCount = createParams.buildBvTree ? createParams.polyCount*2 : 0;
	
	    int offMeshVertsBase = createParams.vertCount;
	    int offMeshPolyBase = createParams.polyCount;

        outTile.links = new dtLink[header.maxLinkCount];
        dtcsArrayItemsCreate(outTile.links);

	    // Store vertices
	    // Mesh vertices
        //const int vertsSize = dtAlign4(sizeof(float)*3*totVertCount);
        //float* navVerts = (float*)d; d += vertsSize;
        outTile.verts = new float[totVertCount*3];
        float[] navVerts = outTile.verts;
	    for (int i = 0; i < createParams.vertCount; ++i)
	    {
		    //const ushort* iv = &createParams.verts[i*3];
            //float* v = &navVerts[i*3];
            int ivIndex = i*3;
            int vIndex = i*3;
		    navVerts[vIndex + 0] = createParams.bmin[0] + createParams.verts[ivIndex + 0] * createParams.cs;
		    navVerts[vIndex + 1] = createParams.bmin[1] + createParams.verts[ivIndex + 1] * createParams.ch;
		    navVerts[vIndex + 2] = createParams.bmin[2] + createParams.verts[ivIndex + 2] * createParams.cs;
	    }
	    // Off-mesh link vertices.
	    int n = 0;
	    for (int i = 0; i < createParams.offMeshConCount; ++i)
	    {
		    // Only store connections which start from this tile.
		    if (offMeshConClass[i*2+0] == 0xff)
		    {
			    //const float* linkv = &createParams.offMeshConVerts[i*2*3];
			    //float* v = &navVerts[(offMeshVertsBase + n*2)*3];
                int linkVStart = i*2*3;
                int vStart = (offMeshVertsBase + n*2)*3;
			    dtVcopy(navVerts, vStart + 0, createParams.offMeshConVerts, linkVStart + 0);
			    dtVcopy(navVerts, vStart + 3, createParams.offMeshConVerts, linkVStart + 3);
			    n++;
		    }
	    }
	
	    // Store polygons
	    // Mesh polys
	    //const ushort* src = createParams.polys;
        //ushort[] src = createParams.polys;
        int srcIndex = 0;
        //const int polysSize = dtAlign4(sizeof(dtPoly)*totPolyCount);
        //dtPoly* navPolys = (dtPoly*)d; d += polysSize;
        outTile.polys = new dtPoly[totPolyCount];
        dtcsArrayItemsCreate(outTile.polys);
        //outTile.offMeshCons ??
        dtPoly[] navPolys = outTile.polys;
        //outTile.
	    for (int i = 0; i < createParams.polyCount; ++i)
	    {
		    //dtPoly* p = &navPolys[i];
            dtPoly p =  navPolys[i];
		    p.vertCount = 0;
		    p.flags = createParams.polyFlags[i];
		    p.setArea(createParams.polyAreas[i]);
		    p.setType((byte)dtPolyTypes.DT_POLYTYPE_GROUND);
		    for (int j = 0; j < nvp; ++j)
		    {
			    if (createParams.polys[srcIndex + j] == MESH_NULL_IDX) 
                    break;
			    p.verts[j] = createParams.polys[srcIndex + j];
			    if ((createParams.polys[srcIndex + nvp+j] & 0x8000) != 0)
			    {
				    // Border or portal edge.
				    ushort dir = (ushort)(createParams.polys[srcIndex + nvp+j] & 0xf);
				    if (dir == 0xf) // Border
					    p.neis[j] = 0;
				    else if (dir == 0) // Portal x-
					    p.neis[j] = DT_EXT_LINK | 4;
				    else if (dir == 1) // Portal z+
					    p.neis[j] = DT_EXT_LINK | 2;
				    else if (dir == 2) // Portal x+
					    p.neis[j] = DT_EXT_LINK | 0;
				    else if (dir == 3) // Portal z-
					    p.neis[j] = DT_EXT_LINK | 6;
			    }
			    else
			    {
				    // Normal connection
				    p.neis[j] = (ushort) (createParams.polys[srcIndex + nvp+j]+1);
			    }
			
			    p.vertCount++;
		    }
		    srcIndex += nvp*2;
	    }
	    // Off-mesh connection vertices.
	    n = 0;
	    for (int i = 0; i < createParams.offMeshConCount; ++i)
	    {
		    // Only store connections which start from this tile.
		    if (offMeshConClass[i*2+0] == 0xff)
		    {
			    dtPoly p = navPolys[offMeshPolyBase+n];
			    p.vertCount = 2;
			    p.verts[0] = (ushort)(offMeshVertsBase + n*2+0);
			    p.verts[1] = (ushort)(offMeshVertsBase + n*2+1);
			    p.flags = createParams.offMeshConFlags[i];
			    p.setArea(createParams.offMeshConAreas[i]);
			    p.setType((byte)dtPolyTypes.DT_POLYTYPE_OFFMESH_CONNECTION);
			    n++;
		    }
	    }

	    // Store detail meshes and vertices.
	    // The nav polygon vertices are stored as the first vertices on each mesh.
	    // We compress the mesh data by skipping them and using the navmesh coordinates.
        //const int detailMeshesSize = dtAlign4(sizeof(dtPolyDetail)*createParams.polyCount);
        //dtPolyDetail* navDMeshes = (dtPolyDetail*)d; d += detailMeshesSize;
        outTile.detailMeshes = new dtPolyDetail[createParams.polyCount];
        dtPolyDetail[] navDMeshes = outTile.detailMeshes;
        dtcsArrayItemsCreate(navDMeshes);

        outTile.detailVerts = new float[3*uniqueDetailVertCount];
        float[] navDVerts = outTile.detailVerts;

        outTile.detailTris = new byte[4*detailTriCount];
        byte[] navDTris = outTile.detailTris;

	    if (createParams.detailMeshes != null)
	    {
		    ushort vbase = 0;
		    for (int i = 0; i < createParams.polyCount; ++i)
		    {
			    dtPolyDetail dtl = navDMeshes[i];
			    int vb = (int)createParams.detailMeshes[i*4+0];
			    int ndv = (int)createParams.detailMeshes[i*4+1];
			    int nv = navPolys[i].vertCount;
			    dtl.vertBase = (uint)vbase;
			    dtl.vertCount = (byte)(ndv-nv);
			    dtl.triBase = (uint)createParams.detailMeshes[i*4+2];
			    dtl.triCount = (byte)createParams.detailMeshes[i*4+3];
			    // Copy vertices except the first 'nv' verts which are equal to nav poly verts.
			    if (ndv-nv != 0)
			    {
				    //memcpy(&navDVerts[vbase*3], &createParams.detailVerts[(vb+nv)*3], sizeof(float)*3*(ndv-nv));
                    for (int j=0;j<3*(ndv-nv);++j){
                        navDVerts[j + vbase*3] = createParams.detailVerts[j + (vb+nv)*3];
                    }
				    vbase += (ushort)(ndv-nv);
			    }
		    }
		    // Store triangles.
		    //memcpy(navDTris, createParams.detailTris, sizeof(byte)*4*createParams.detailTriCount);
            for (int j=0; j< 4*createParams.detailTriCount;++j){
                navDTris[j] = createParams.detailTris[j];
            }
	    }
	    else
	    {
		    // Create dummy detail mesh by triangulating polys.
		    int tbase = 0;
		    for (int i = 0; i < createParams.polyCount; ++i)
		    {
			    dtPolyDetail dtl = navDMeshes[i];
			    int nv = navPolys[i].vertCount;
			    dtl.vertBase = 0;
			    dtl.vertCount = 0;
			    dtl.triBase = (uint)tbase;
			    dtl.triCount = (byte)(nv-2);
			    // Triangulate polygon (local indices).
			    for (int j = 2; j < nv; ++j)
			    {
				    //byte* t = &navDTris[tbase*4];
                    int tIndex = tbase*4;
				    navDTris[tIndex + 0] = 0;
				    navDTris[tIndex + 1] = (byte)(j-1);
				    navDTris[tIndex + 2] = (byte)j;
				    // Bit for each edge that belongs to poly boundary.
				    navDTris[tIndex + 3] = (1<<2);
				    if (j == 2) 
                        navDTris[tIndex + 3] |= (1<<0);
				    if (j == nv-1) 
                        navDTris[tIndex + 3] |= (1<<4);
				    tbase++;
			    }
		    }
	    }

        //createParams.buildBvTree ? dtAlign4(sizeof(dtBVNode)*createParams.polyCount*2) : 0;
        int bvTreeNodeCout = createParams.buildBvTree ? createParams.polyCount*2 : 0;
        outTile.bvTree = new dtBVNode[bvTreeNodeCout];
        dtcsArrayItemsCreate(outTile.bvTree);
        dtBVNode[] navBvtree = outTile.bvTree;

        //const int offMeshConsSize = dtAlign4(sizeof(dtOffMeshConnection)*storedOffMeshConCount);
        outTile.offMeshCons = new dtOffMeshConnection[storedOffMeshConCount];
        dtOffMeshConnection[] offMeshCons = outTile.offMeshCons;
	    // Store and create BVtree.
	    // TODO: take detail mesh into account! use byte per bbox extent?
	    if (createParams.buildBvTree)
	    {
		    createBVTree(createParams.verts, createParams.vertCount, createParams.polys, createParams.polyCount,
					     nvp, createParams.cs, createParams.ch, createParams.polyCount*2, navBvtree);
	    }
	
	    // Store Off-Mesh connections.
	    n = 0;
	    for (int i = 0; i < createParams.offMeshConCount; ++i)
	    {
		    // Only store connections which start from this tile.
		    if (offMeshConClass[i*2+0] == 0xff)
		    {
			    dtOffMeshConnection con = offMeshCons[n];
			    con.poly = (ushort)(offMeshPolyBase + n);
			    // Copy connection end-points.
			    //float[] endPts = createParams.offMeshConVerts[i*2*3];
                int endPtsStart = i*2*3;
			    dtVcopy(con.pos,0, createParams.offMeshConVerts, endPtsStart + 0);
			    dtVcopy(con.pos,3, createParams.offMeshConVerts, endPtsStart + 3);
			    con.rad = createParams.offMeshConRad[i];
			    con.flags = createParams.offMeshConDir[i] != 0 ? (byte)DT_OFFMESH_CON_BIDIR : (byte)0;
			    con.side = offMeshConClass[i*2+1];
			    if (createParams.offMeshConUserID != null){
				    con.userId = createParams.offMeshConUserID[i];
                }
			    n++;
		    }
	    }
		
	    //dtFree(offMeshConClass);
	
	    //*outData = data;
	    //*outDataSize = dataSize;
	
	    return true;
    }
    /*
    /// Swaps the endianess of the tile data's header (#dtMeshHeader).
    ///  @param[in,out]	data		The tile data array.
    ///  @param[in]		dataSize	The size of the data array.
    bool dtNavMeshHeaderSwapEndian(byte* data, const int dataSize)
    {
	    dtMeshHeader* header = (dtMeshHeader*)data;
	
	    int swappedMagic = DT_NAVMESH_MAGIC;
	    int swappedVersion = DT_NAVMESH_VERSION;
	    dtSwapEndian(&swappedMagic);
	    dtSwapEndian(&swappedVersion);
	
	    if ((header.magic != DT_NAVMESH_MAGIC || header.version != DT_NAVMESH_VERSION) &&
		    (header.magic != swappedMagic || header.version != swappedVersion))
	    {
		    return false;
	    }
		
	    dtSwapEndian(&header.magic);
	    dtSwapEndian(&header.version);
	    dtSwapEndian(&header.x);
	    dtSwapEndian(&header.y);
	    dtSwapEndian(&header.layer);
	    dtSwapEndian(&header.userId);
	    dtSwapEndian(&header.polyCount);
	    dtSwapEndian(&header.vertCount);
	    dtSwapEndian(&header.maxLinkCount);
	    dtSwapEndian(&header.detailMeshCount);
	    dtSwapEndian(&header.detailVertCount);
	    dtSwapEndian(&header.detailTriCount);
	    dtSwapEndian(&header.bvNodeCount);
	    dtSwapEndian(&header.offMeshConCount);
	    dtSwapEndian(&header.offMeshBase);
	    dtSwapEndian(&header.walkableHeight);
	    dtSwapEndian(&header.walkableRadius);
	    dtSwapEndian(&header.walkableClimb);
	    dtSwapEndian(&header.bmin[0]);
	    dtSwapEndian(&header.bmin[1]);
	    dtSwapEndian(&header.bmin[2]);
	    dtSwapEndian(&header.bmax[0]);
	    dtSwapEndian(&header.bmax[1]);
	    dtSwapEndian(&header.bmax[2]);
	    dtSwapEndian(&header.bvQuantFactor);

	    // Freelist index and pointers are updated when tile is added, no need to swap.

	    return true;
    }
    */
    /// @par
    ///
    /// @warning This function assumes that the header is in the correct endianess already. 
    /// Call #dtNavMeshHeaderSwapEndian() first on the data if the data is expected to be in wrong endianess 
    /// to start with. Call #dtNavMeshHeaderSwapEndian() after the data has been swapped if converting from 
    /// native to foreign endianess.
    /// Swaps endianess of the tile data.
    ///  @param[in,out]	data		The tile data array.
    ///  @param[in]		dataSize	The size of the data array.
    /*
    bool dtNavMeshDataSwapEndian(byte* data, const int dataSize)
    {
	    // Make sure the data is in right format.
	    dtMeshHeader* header = (dtMeshHeader*)data;
	    if (header.magic != DT_NAVMESH_MAGIC)
		    return false;
	    if (header.version != DT_NAVMESH_VERSION)
		    return false;
	
	    // Patch header pointers.
	    const int headerSize = dtAlign4(sizeof(dtMeshHeader));
	    const int vertsSize = dtAlign4(sizeof(float)*3*header.vertCount);
	    const int polysSize = dtAlign4(sizeof(dtPoly)*header.polyCount);
	    const int linksSize = dtAlign4(sizeof(dtLink)*(header.maxLinkCount));
	    const int detailMeshesSize = dtAlign4(sizeof(dtPolyDetail)*header.detailMeshCount);
	    const int detailVertsSize = dtAlign4(sizeof(float)*3*header.detailVertCount);
	    const int detailTrisSize = dtAlign4(sizeof(byte)*4*header.detailTriCount);
	    const int bvtreeSize = dtAlign4(sizeof(dtBVNode)*header.bvNodeCount);
	    const int offMeshLinksSize = dtAlign4(sizeof(dtOffMeshConnection)*header.offMeshConCount);
	
	    byte* d = data + headerSize;
	    float* verts = (float*)d; d += vertsSize;
	    dtPoly* polys = (dtPoly*)d; d += polysSize;
	    //dtLink* links = (dtLink*)d;
      d += linksSize;
      
	    dtPolyDetail* detailMeshes = (dtPolyDetail*)d; d += detailMeshesSize;
	    float* detailVerts = (float*)d; d += detailVertsSize;
	    //byte* detailTris = (byte*)d; 
    d += detailTrisSize;
	    dtBVNode* bvTree = (dtBVNode*)d; d += bvtreeSize;
	    dtOffMeshConnection* offMeshCons = (dtOffMeshConnection*)d; d += offMeshLinksSize;
	
	    // Vertices
	    for (int i = 0; i < header.vertCount*3; ++i)
	    {
		    dtSwapEndian(&verts[i]);
	    }

	    // Polys
	    for (int i = 0; i < header.polyCount; ++i)
	    {
		    dtPoly* p = &polys[i];
		    // poly.firstLink is update when tile is added, no need to swap.
		    for (int j = 0; j < DT_VERTS_PER_POLYGON; ++j)
		    {
			    dtSwapEndian(&p.verts[j]);
			    dtSwapEndian(&p.neis[j]);
		    }
		    dtSwapEndian(&p.flags);
	    }

	    // Links are rebuild when tile is added, no need to swap.

	    // Detail meshes
	    for (int i = 0; i < header.detailMeshCount; ++i)
	    {
		    dtPolyDetail* pd = &detailMeshes[i];
		    dtSwapEndian(&pd.vertBase);
		    dtSwapEndian(&pd.triBase);
	    }
	
	    // Detail verts
	    for (int i = 0; i < header.detailVertCount*3; ++i)
	    {
		    dtSwapEndian(&detailVerts[i]);
	    }

	    // BV-tree
	    for (int i = 0; i < header.bvNodeCount; ++i)
	    {
		    dtBVNode* node = &bvTree[i];
		    for (int j = 0; j < 3; ++j)
		    {
			    dtSwapEndian(&node.bmin[j]);
			    dtSwapEndian(&node.bmax[j]);
		    }
		    dtSwapEndian(&node.i);
	    }

	    // Off-mesh Connections.
	    for (int i = 0; i < header.offMeshConCount; ++i)
	    {
		    dtOffMeshConnection* con = &offMeshCons[i];
		    for (int j = 0; j < 6; ++j)
			    dtSwapEndian(&con.pos[j]);
		    dtSwapEndian(&con.rad);
		    dtSwapEndian(&con.poly);
	    }
	
	    return true;
    }
     * */
}
