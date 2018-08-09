using System.Text;
using System;

using dtStatus = System.UInt32;
using System.Numerics;
#if DT_POLYREF64
using dtPolyRef = System.UInt64;
#else
//typedef uint dtPolyRef;
using dtPolyRef = System.UInt32;
#endif

//In this file, System refers to the SystemHelper class, which holds all necessary 
//data relating to a navmesh and provides a high level interface
public static partial class RcdtcsUnityUtils{
	/// These are just sample areas to use consistent values across the samples.
	/// The user should specify these base on his needs.
	public enum SamplePolyAreas {
		GROUND,
		//SAMPLE_POLYAREA_WATER,
		//SAMPLE_POLYAREA_ROAD,
		//SAMPLE_POLYAREA_DOOR,
		//SAMPLE_POLYAREA_GRASS,
		//SAMPLE_POLYAREA_JUMP,
	};
	public enum SamplePolyFlags {
		WALK = 0x01,		// Ability to walk (ground, grass, road)
		//SAMPLE_POLYFLAGS_SWIM		= 0x02,		// Ability to swim (water).
		//SAMPLE_POLYFLAGS_DOOR		= 0x04,		// Ability to move through doors.
		//SAMPLE_POLYFLAGS_JUMP		= 0x08,		// Ability to jump.
		//SAMPLE_POLYFLAGS_DISABLED	= 0x10,		// Disabled polygon
		ALL = 0xffff	// All abilities.
	};

	[Serializable()]
	public class RecastMeshParams {
		
		public float m_cellSize = 50f;
		public float m_cellHeight = 50f;
		public float m_agentHeight = 2.0f;
		public float m_agentRadius = 80f;
		public float m_agentMaxClimb = .9f;
		public float m_agentMaxSlope = 90f;
		public float m_regionMinSize = 8.0f;
		public float m_regionMergeSize = 20.0f;
		public bool m_monotonePartitioning = false;
		public float m_edgeMaxLen = 12.0f;
		public float m_edgeMaxError = 1.3f;
		public float m_vertsPerPoly = 6.0f;
		public float m_detailSampleDist = 6.0f;
		public float m_detailSampleMaxError= 1.0f;
		
		public void Copy(RecastMeshParams param){
			m_cellSize = param.m_cellSize;
			m_cellHeight = param.m_cellHeight;
			m_agentHeight = param.m_agentRadius;
			m_agentRadius = param.m_agentRadius;
			m_agentMaxClimb = param.m_agentMaxClimb;
			m_agentMaxSlope = param.m_agentMaxSlope;
			m_regionMinSize = param.m_regionMinSize;
			m_regionMergeSize = param.m_regionMergeSize;
			m_monotonePartitioning = param.m_monotonePartitioning;
			m_edgeMaxLen = param.m_edgeMaxLen;
			m_edgeMaxError = param.m_edgeMaxError;
			m_vertsPerPoly = param.m_vertsPerPoly;
			m_detailSampleDist = param.m_detailSampleDist;
			m_detailSampleMaxError = param.m_detailSampleMaxError;
		}
		
		public override string ToString() {
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("=== C# Version ===");
			sb.AppendLine("=== NavMesh Build Params ===");
			sb.AppendLine("Rasterization:");
			sb.AppendFormat("\tCell Size: {0:2}\n", m_cellSize);
			sb.AppendFormat("\tCell Height: {0:2}\n", m_cellHeight);
			sb.AppendLine("Agent:");
			sb.AppendFormat("\tHeight: {0:2}\n", m_agentHeight);
			sb.AppendFormat("\tRadius: {0:2}\n", m_agentRadius);
			sb.AppendFormat("\tMax Climb: {0:2}\n", m_agentMaxClimb);
			sb.AppendFormat("\tMax Slope: {0:2}\n", m_agentMaxSlope);
			sb.AppendLine("Region:");
			sb.AppendFormat("\tMin Size: {0:2}\n", m_regionMinSize);
			sb.AppendFormat("\tMerge Size: {0:2}\n", m_regionMergeSize);
			sb.AppendLine("\tMonotone Partiontioning: " + m_monotonePartitioning.ToString());
			sb.AppendLine("Polygonization:");
			sb.AppendFormat("\tEdge Max Length: {0:2}\n", m_edgeMaxLen);
			sb.AppendFormat("\tEdge Max Error: {0:2}\n", m_edgeMaxError);
			sb.AppendFormat("\tVerts Per Poly: {0:2}\n", m_vertsPerPoly);
			sb.AppendLine("Detail Mesh:");
			sb.AppendFormat("\tDetail Sample Dist: {0:2}\n", m_detailSampleDist);
			sb.AppendFormat("\tDetail Sample Max Error: {0:2}\n", m_detailSampleMaxError);
			return sb.ToString();
		}
	}

    [Serializable()]
    public class SystemHelper {
		private RecastMeshParams m_RecastMeshParams = new RecastMeshParams();
		
		public Recast.BuildContext m_ctx = new Recast.BuildContext();
		
		public bool m_keepInterResults = true;
		public float m_totalBuildTimeMs = 0.0f;
		
		public byte[] m_triareas = null;
		public Recast.rcHeightfield m_solid = null;
		public Recast.rcCompactHeightfield m_chf = null;
		public Recast.rcContourSet m_cset = null;
		
		public Recast.rcPolyMesh m_pmesh = null;
		public Recast.rcConfig m_cfg = null;
		public Recast.rcPolyMeshDetail m_dmesh = null;
		
		public Detour.dtNavMesh m_navMesh = null;
		public Detour.dtNavMeshQuery m_navQuery = null;
		
		public int m_vertCount = 0;
		public int m_triCount = 0;
		public float[] m_verts = null;
		public int[] m_tris = null;
		public float[] m_bmin = new float[3];
		public float[] m_bmax = new float[3];
		
		public Detour.dtRawTileData m_rawTileData = null;
		
		public void Clear() {
			m_RecastMeshParams = null;			
			ClearComputedData();			
			ClearMesh();
		}
		
		public void SetNavMeshParams(RecastMeshParams param){
			m_RecastMeshParams.Copy(param);
		}
		
		public RecastMeshParams GetNavMeshParams(){
			return m_RecastMeshParams;
		}
		
		public void ClearComputedData() {
			m_ctx = new Recast.BuildContext();
			
			m_triareas = null;
			m_solid = null;
			m_chf = null;
			m_cset = null;
			m_pmesh = null;
			m_cfg = null;
			m_dmesh = null;
			
			m_navMesh = null;
			m_navQuery = null;
			
			m_bmin = new float[3];
			m_bmax = new float[3];            
		}
		
		public void ClearMesh() {
			m_tris = null;
			m_verts = null;
			m_triCount = 0;
			m_vertCount = 0;
		}
		
		public void AddMesh(NavMeshData mesh) {
			int vertStart = 0;
			int prevVertCount = 0;
			int newVertCount = mesh.Vertices.Count;
			
			int minTri = 0;
			int maxTri = int.MaxValue;
			
			if (m_verts != null) {
				vertStart = m_verts.Length;
				prevVertCount = vertStart / 3;
				m_verts = ResizeStaticArray(m_verts, typeof(float), vertStart + newVertCount*3) as float[];
			} else {
				m_verts = new float[newVertCount * 3];
			}
			int triStart = 0;
			int newTriCount = mesh.Triangles.Length;
			if (m_tris != null) {
				triStart = m_tris.Length;
				m_tris = ResizeStaticArray(m_tris, typeof(int), triStart + newTriCount) as int[];
				int[] tris = mesh.Triangles;
				for (int i = 0; i < newTriCount; ++i) {
					m_tris[i + triStart] = prevVertCount + tris[i];
				}
			} else {
				m_tris = mesh.Triangles;
			}
			
			for (int i = 0; i < m_tris.Length; ++i) {
				minTri = Math.Max(m_tris[i], minTri);
				maxTri = Math.Min(m_tris[i], maxTri);
			}

            var vertices = new System.Collections.Generic.List<Vector3>();
            foreach (var vert in mesh.Vertices)
            {
                vertices.Add(new Vector3(vert.x, vert.y, vert.z));
            }
            Vector3[] verts = vertices.ToArray();

            mesh.Vertices.ToArray();
			
			for (int i = 0; i < newVertCount; ++i) {
				int v = vertStart + i * 3;
				m_verts[v + 0] = verts[i].X;
				m_verts[v + 1] = verts[i].Y;
				m_verts[v + 2] = verts[i].Z;
			}
			
			m_vertCount += mesh.Vertices.Count;
            m_triCount += mesh.Triangles.Length / 3;
		}
        
		public bool ComputeSystem(byte[] tileRawData, int start) {
			
			m_ctx.enableLog(true);
			
			m_ctx.resetTimers();

			// Start the build process.	
			m_ctx.startTimer(Recast.rcTimerLabel.RC_TIMER_TOTAL);
			
			m_rawTileData = new Detour.dtRawTileData();
			m_rawTileData.FromBytes(tileRawData, start);
			
			m_navMesh = new Detour.dtNavMesh();
			if (m_navMesh == null) {
				m_ctx.log(Recast.rcLogCategory.RC_LOG_ERROR, "Could not create Detour navmesh");
				return false;
			}
			
			dtStatus status;
			
			status = m_navMesh.init(m_rawTileData, (int)Detour.dtTileFlags.DT_TILE_FREE_DATA);
			if (Detour.dtStatusFailed(status)) {
				m_ctx.log(Recast.rcLogCategory.RC_LOG_ERROR, "Could not init Detour navmesh");
				return false;
			}
			
			m_navQuery = new Detour.dtNavMeshQuery();
			status = m_navQuery.init(m_navMesh, 2048);
			if (Detour.dtStatusFailed(status)) {
				m_ctx.log(Recast.rcLogCategory.RC_LOG_ERROR, "Could not init Detour navmesh query");
				return false;
			}
			
			m_ctx.stopTimer(Recast.rcTimerLabel.RC_TIMER_TOTAL);
			
			//m_ctx.log(Recast.rcLogCategory.RC_LOG_PROGRESS, ">> Polymesh: " + m_pmesh.nverts + " vertices  " + m_pmesh.npolys + " polygons");
			
			m_totalBuildTimeMs = (float) m_ctx.getAccumulatedTime(Recast.rcTimerLabel.RC_TIMER_TOTAL);
			
			return true;
		}
				
		//Compute Recast and Detour navmesh
		public bool ComputeSystem() {
			
			ClearComputedData();
			
			Recast.rcCalcBounds(m_verts, m_vertCount, m_bmin, m_bmax);
			
			//
			// Step 1. Initialize build config.
			//
			
			// Init build configuration from GUI
			m_cfg = new Recast.rcConfig();
			
			m_cfg.cs = m_RecastMeshParams.m_cellSize;
			m_cfg.ch = m_RecastMeshParams.m_cellHeight;
			m_cfg.walkableSlopeAngle = m_RecastMeshParams.m_agentMaxSlope;
			m_cfg.walkableHeight = (int)Math.Ceiling(m_RecastMeshParams.m_agentHeight / m_cfg.ch);
			m_cfg.walkableClimb = (int)Math.Floor(m_RecastMeshParams.m_agentMaxClimb / m_cfg.ch);
			m_cfg.walkableRadius = (int)Math.Ceiling(m_RecastMeshParams.m_agentRadius / m_cfg.cs);
			m_cfg.maxEdgeLen = (int)(m_RecastMeshParams.m_edgeMaxLen / m_RecastMeshParams.m_cellSize);
			m_cfg.maxSimplificationError = m_RecastMeshParams.m_edgeMaxError;
			m_cfg.minRegionArea = (int)(m_RecastMeshParams.m_regionMinSize * m_RecastMeshParams.m_regionMinSize);		// Note: area = size*size
			m_cfg.mergeRegionArea = (int)(m_RecastMeshParams.m_regionMergeSize * m_RecastMeshParams.m_regionMergeSize);	// Note: area = size*size
			m_cfg.maxVertsPerPoly = (int)m_RecastMeshParams.m_vertsPerPoly;
			m_cfg.detailSampleDist = m_RecastMeshParams.m_detailSampleDist < 0.9f ? 0 : m_RecastMeshParams.m_cellSize * m_RecastMeshParams.m_detailSampleDist;
			m_cfg.detailSampleMaxError = m_RecastMeshParams.m_cellHeight * m_RecastMeshParams.m_detailSampleMaxError;
			
			// Set the area where the navigation will be build.
			// Here the bounds of the input mesh are used, but the
			// area could be specified by an user defined box, etc.
			Recast.rcVcopy(m_cfg.bmin, m_bmin);
			Recast.rcVcopy(m_cfg.bmax, m_bmax);
			Recast.rcCalcGridSize(m_cfg.bmin, m_cfg.bmax, m_cfg.cs, out m_cfg.width, out m_cfg.height);
			
			// Reset build times gathering.
			m_ctx.resetTimers();

			// Start the build process.	
			m_ctx.startTimer(Recast.rcTimerLabel.RC_TIMER_TOTAL);
			
			m_ctx.log(Recast.rcLogCategory.RC_LOG_PROGRESS, "Building navigation:");
			m_ctx.log(Recast.rcLogCategory.RC_LOG_PROGRESS, " - " + m_cfg.width + " x " + m_cfg.height + " cells");
			m_ctx.log(Recast.rcLogCategory.RC_LOG_PROGRESS, " - " + m_vertCount / 1000.0f + "K verts, " + m_triCount / 1000.0f + "K tris");
			
			//
			// Step 2. Rasterize input polygon soup.
			//
			
			// Allocate voxel heightfield where we rasterize our input data to.
			m_solid = new Recast.rcHeightfield();
			if (m_solid == null) {
				m_ctx.log(Recast.rcLogCategory.RC_LOG_ERROR, "buildNavigation: Out of memory 'solid'.");
				return false;
			}
			if (!Recast.rcCreateHeightfield(m_ctx, m_solid, m_cfg.width, m_cfg.height, m_cfg.bmin, m_cfg.bmax, m_cfg.cs, m_cfg.ch)) {
				m_ctx.log(Recast.rcLogCategory.RC_LOG_ERROR, "buildNavigation: Could not create solid heightfield.");
				return false;
			}
			
			// Allocate array that can hold triangle area types.
			// If you have multiple meshes you need to process, allocate
			// and array which can hold the max number of triangles you need to process.
			m_triareas = new byte[m_triCount];
			if (m_triareas == null) {
				m_ctx.log(Recast.rcLogCategory.RC_LOG_ERROR, "buildNavigation: Out of memory 'm_triareas' (" + m_triCount + ").");
				return false;
			}
			
			// Find triangles which are walkable based on their slope and rasterize them.
			// If your input data is multiple meshes, you can transform them here, calculate
			// the are type for each of the meshes and rasterize them.
			//memset(m_triareas, 0, ntris*sizeof(byte));
			
			Recast.rcMarkWalkableTriangles(m_ctx, m_cfg.walkableSlopeAngle, m_verts, m_vertCount, m_tris, m_triCount, m_triareas);
			Recast.rcRasterizeTriangles(m_ctx, m_verts, m_vertCount, m_tris, m_triareas, m_triCount, m_solid, m_cfg.walkableClimb);
			
			if (!m_keepInterResults) {
				m_triareas = null;
			}
			
			//
			// Step 3. Filter walkables surfaces.
			//
			
			// Once all geoemtry is rasterized, we do initial pass of filtering to
			// remove unwanted overhangs caused by the conservative rasterization
			// as well as filter spans where the character cannot possibly stand.
			Recast.rcFilterLowHangingWalkableObstacles(m_ctx, m_cfg.walkableClimb, m_solid);
			Recast.rcFilterLedgeSpans(m_ctx, m_cfg.walkableHeight, m_cfg.walkableClimb, m_solid);
			Recast.rcFilterWalkableLowHeightSpans(m_ctx, m_cfg.walkableHeight, m_solid);
			
			
			//
			// Step 4. Partition walkable surface to simple regions.
			//
			
			// Compact the heightfield so that it is faster to handle from now on.
			// This will result more cache coherent data as well as the neighbours
			// between walkable cells will be calculated.
			m_chf = new Recast.rcCompactHeightfield();
			if (m_chf == null) {
				m_ctx.log(Recast.rcLogCategory.RC_LOG_ERROR, "buildNavigation: Out of memory 'chf'.");
				return false;
			}
			if (!Recast.rcBuildCompactHeightfield(m_ctx, m_cfg.walkableHeight, m_cfg.walkableClimb, m_solid, m_chf)) {
				m_ctx.log(Recast.rcLogCategory.RC_LOG_ERROR, "buildNavigation: Could not build compact data.");
				return false;
			}
			
			if (!m_keepInterResults) {
				m_solid = null;
			}
			
			// Erode the walkable area by agent radius.
			if (!Recast.rcErodeWalkableArea(m_ctx, m_cfg.walkableRadius, m_chf)) {
				m_ctx.log(Recast.rcLogCategory.RC_LOG_ERROR, "buildNavigation: Could not erode.");
				return false;
			}
			
			/*
	            // (Optional) Mark areas.
	            ConvexVolume[] vols = m_geom.getConvexVolumes();
	            for (int i  = 0; i < m_geom.getConvexVolumeCount(); ++i)
	                rcMarkConvexPolyArea(m_ctx, vols[i].verts, vols[i].nverts, vols[i].hmin, vols[i].hmax, (byte)vols[i].area, *m_chf);
	            */
			
			if (m_RecastMeshParams.m_monotonePartitioning) {
				// Partition the walkable surface into simple regions without holes.
				// Monotone partitioning does not need distancefield.
				if (!Recast.rcBuildRegionsMonotone(m_ctx, m_chf, 0, m_cfg.minRegionArea, m_cfg.mergeRegionArea)) {
					m_ctx.log(Recast.rcLogCategory.RC_LOG_ERROR, "buildNavigation: Could not build regions.");
					return false;
				}
			} else {
				// Prepare for region partitioning, by calculating distance field along the walkable surface.
				if (!Recast.rcBuildDistanceField(m_ctx, m_chf)) {
					m_ctx.log(Recast.rcLogCategory.RC_LOG_ERROR, "buildNavigation: Could not build distance field.");
					return false;
				}
				
				// Partition the walkable surface into simple regions without holes.
				if (!Recast.rcBuildRegions(m_ctx, m_chf, 0, m_cfg.minRegionArea, m_cfg.mergeRegionArea)) {
					m_ctx.log(Recast.rcLogCategory.RC_LOG_ERROR, "buildNavigation: Could not build regions.");
					return false;
				}
			}
			
			//
			// Step 5. Trace and simplify region contours.
			//
			
			// Create contours.
			m_cset = new Recast.rcContourSet();
			if (m_cset == null) {
				m_ctx.log(Recast.rcLogCategory.RC_LOG_ERROR, "buildNavigation: Out of memory 'cset'.");
				return false;
			}
			if (!Recast.rcBuildContours(m_ctx, m_chf, m_cfg.maxSimplificationError, m_cfg.maxEdgeLen, m_cset, -1)) {
				m_ctx.log(Recast.rcLogCategory.RC_LOG_ERROR, "buildNavigation: Could not create contours.");
				return false;
			}
			
			//m_cset.dumpToTxt("Data/CSET_dump.txt");
			
			//
			// Step 6. Build polygons mesh from contours.
			//
			
			// Build polygon navmesh from the contours.
			m_pmesh = new Recast.rcPolyMesh();
			if (m_pmesh == null) {
				m_ctx.log(Recast.rcLogCategory.RC_LOG_ERROR, "buildNavigation: Out of memory 'pmesh'.");
				return false;
			}
			if (!Recast.rcBuildPolyMesh(m_ctx, m_cset, m_cfg.maxVertsPerPoly, m_pmesh)) {
				m_ctx.log(Recast.rcLogCategory.RC_LOG_ERROR, "buildNavigation: Could not triangulate contours.");
				return false;
			}
			
			//m_pmesh.dumpToObj("Data/navmesh.obj");
			//m_pmesh.dumpToText("Data/navmesh.txt");
			
			//
			// Step 7. Create detail mesh which allows to access approximate height on each polygon.
			//
			
			m_dmesh = new Recast.rcPolyMeshDetail();//rcAllocPolyMeshDetail();
			if (m_dmesh == null) {
				m_ctx.log(Recast.rcLogCategory.RC_LOG_ERROR, "buildNavigation: Out of memory 'pmdtl'.");
				return false;
			}
			
			if (!Recast.rcBuildPolyMeshDetail(m_ctx, m_pmesh, m_chf, m_cfg.detailSampleDist, m_cfg.detailSampleMaxError, m_dmesh)) {
				m_ctx.log(Recast.rcLogCategory.RC_LOG_ERROR, "buildNavigation: Could not build detail mesh.");
				return false;
			}
			
			//m_dmesh.dumpToText("Data/polymeshdetail_cs.txt");
			//m_dmesh.dumpToObj("Data/polymeshdetail_cs.obj");
			
			if (!m_keepInterResults) {
				
				m_chf = null;
				m_cset = null;
			}
			
			// At this point the navigation mesh data is ready, you can access it from m_pmesh.
			// See duDebugDrawPolyMesh or dtCreateNavMeshData as examples how to access the data.
			
			//
			// (Optional) Step 8. Create Detour data from Recast poly mesh.
			//
			
			// The GUI may allow more max points per polygon than Detour can handle.
			// Only build the detour navmesh if we do not exceed the limit.
			if (m_cfg.maxVertsPerPoly <= Detour.DT_VERTS_PER_POLYGON) {
				//unsigned char* navData = 0;
				Detour.dtRawTileData navData = null;
				//int navDataSize = 0;
				
				// Update poly flags from areas.
				for (int i = 0; i < m_pmesh.npolys; ++i) {
					if (m_pmesh.areas[i] == Recast.RC_WALKABLE_AREA)
						m_pmesh.areas[i] = (byte)SamplePolyAreas.GROUND;
					
					if (m_pmesh.areas[i] == (byte)SamplePolyAreas.GROUND) {
						m_pmesh.flags[i] = (ushort)SamplePolyFlags.WALK;
					}
					
					/*
	                    if (m_pmesh.areas[i] == Recast.RC_WALKABLE_AREA)
	                        m_pmesh.areas[i] =  SAMPLE_POLYAREA_GROUND;
				
	                    if (m_pmesh.areas[i] == SAMPLE_POLYAREA_GROUND ||
	                        m_pmesh.areas[i] == SAMPLE_POLYAREA_GRASS ||
	                        m_pmesh.areas[i] == SAMPLE_POLYAREA_ROAD)
	                    {
	                        m_pmesh.flags[i] = SAMPLE_POLYFLAGS_WALK;
	                    }
	                    else if (m_pmesh.areas[i] == SAMPLE_POLYAREA_WATER)
	                    {
	                        m_pmesh.flags[i] = SAMPLE_POLYFLAGS_SWIM;
	                    }
	                    else if (m_pmesh.areas[i] == SAMPLE_POLYAREA_DOOR)
	                    {
	                        m_pmesh.flags[i] = SAMPLE_POLYFLAGS_WALK | SAMPLE_POLYFLAGS_DOOR;
	                    }*/
				}
				
				Detour.dtNavMeshCreateParams navMeshCreateParams = new Detour.dtNavMeshCreateParams();
				navMeshCreateParams.verts = m_pmesh.verts;
				navMeshCreateParams.vertCount = m_pmesh.nverts;
				navMeshCreateParams.polys = m_pmesh.polys;
				navMeshCreateParams.polyAreas = m_pmesh.areas;
				navMeshCreateParams.polyFlags = m_pmesh.flags;
				navMeshCreateParams.polyCount = m_pmesh.npolys;
				navMeshCreateParams.nvp = m_pmesh.nvp;
				navMeshCreateParams.detailMeshes = m_dmesh.meshes;
				navMeshCreateParams.detailVerts = m_dmesh.verts;
				navMeshCreateParams.detailVertsCount = m_dmesh.nverts;
				navMeshCreateParams.detailTris = m_dmesh.tris;
				navMeshCreateParams.detailTriCount = m_dmesh.ntris;
				navMeshCreateParams.offMeshConVerts = null;//m_geom.getOffMeshConnectionVerts();
				navMeshCreateParams.offMeshConRad = null;//m_geom.getOffMeshConnectionRads();
				navMeshCreateParams.offMeshConDir = null;//m_geom.getOffMeshConnectionDirs();
				navMeshCreateParams.offMeshConAreas = null;//m_geom.getOffMeshConnectionAreas();
				navMeshCreateParams.offMeshConFlags = null;//m_geom.getOffMeshConnectionFlags();
				navMeshCreateParams.offMeshConUserID = null;//m_geom.getOffMeshConnectionId();
				navMeshCreateParams.offMeshConCount = 0;//m_geom.getOffMeshConnectionCount();
				navMeshCreateParams.walkableHeight = m_RecastMeshParams.m_agentHeight;
				navMeshCreateParams.walkableRadius = m_RecastMeshParams.m_agentRadius;
				navMeshCreateParams.walkableClimb = m_RecastMeshParams.m_agentMaxClimb;
				Recast.rcVcopy(navMeshCreateParams.bmin, m_pmesh.bmin);
				Recast.rcVcopy(navMeshCreateParams.bmax, m_pmesh.bmax);
				navMeshCreateParams.cs = m_cfg.cs;
				navMeshCreateParams.ch = m_cfg.ch;
				navMeshCreateParams.buildBvTree = true;
				
				if (!Detour.dtCreateNavMeshData(navMeshCreateParams, out navData)) {
					m_ctx.log(Recast.rcLogCategory.RC_LOG_ERROR, "Could not build Detour navmesh.");
					return false;
				}
				
				m_navMesh = new Detour.dtNavMesh();
				if (m_navMesh == null) {
					m_ctx.log(Recast.rcLogCategory.RC_LOG_ERROR, "Could not create Detour navmesh");
					return false;
				}
				
				dtStatus status;
				
				status = m_navMesh.init(navData, (int)Detour.dtTileFlags.DT_TILE_FREE_DATA);
				if (Detour.dtStatusFailed(status)) {
					m_ctx.log(Recast.rcLogCategory.RC_LOG_ERROR, "Could not init Detour navmesh");
					return false;
				}
				
				m_navQuery = new Detour.dtNavMeshQuery();
				status = m_navQuery.init(m_navMesh, 2048);
				if (Detour.dtStatusFailed(status)) {
					m_ctx.log(Recast.rcLogCategory.RC_LOG_ERROR, "Could not init Detour navmesh query");
					return false;
				}
				
				m_rawTileData = navData;
			} else {
				m_ctx.log(Recast.rcLogCategory.RC_LOG_WARNING, "Detour does not support more than " + Detour.DT_VERTS_PER_POLYGON + " verts per polygon. A navmesh has not been generated.");
			}
			
			m_ctx.stopTimer(Recast.rcTimerLabel.RC_TIMER_TOTAL);
			
			// Show performance stats.
			m_ctx.logBuildTimes();
			m_ctx.log(Recast.rcLogCategory.RC_LOG_PROGRESS, ">> Polymesh: " + m_pmesh.nverts + " vertices  " + m_pmesh.npolys + " polygons");
			
			m_totalBuildTimeMs = (float) m_ctx.getAccumulatedTime(Recast.rcTimerLabel.RC_TIMER_TOTAL);
			
			return true;
		}
		
		public float[] GetClosestPointOnNavMesh(float[] pos){
			return RcdtcsUnityUtils.GetClosestPointOnNavMesh(m_navQuery, pos);
		}
        public Vector3 GetClosestPosOnNavMesh(Vector3 pos)
        {
            var point = GetClosestPointOnNavMesh(new float[] { pos.X, pos.Y, pos.Z });
            return new Vector3(point[0], point[1], point[2]);
        }
    }
    	
	public static System.Array ResizeStaticArray(System.Array array, System.Type eltType, int newSize) {
		return ResizeStaticArray(array, eltType, newSize, false, false, "");
	}
	public static System.Array ResizeStaticArray(System.Array array, System.Type eltType, int newSize, bool fillUpRefArraysWithNewElements) {
		return ResizeStaticArray(array, eltType, newSize, fillUpRefArraysWithNewElements, false, "");
	}
	public static System.Array ResizeStaticArray(System.Array array, System.Type eltType, int newSize, bool fillUpRefArraysWithNewElements, bool logWarningIfResized, string warningHeader) {
		System.Array newArray = null;
		if (newSize > 0) {
			int oldSize = 0;
			
			if (array != null) {
				oldSize = array.Length;
			}
			
			if (newSize == oldSize) {
				newArray = array;
			} else {
				if (logWarningIfResized) {
					//Debug.LogWarning(warningHeader + " Array Resize Warning : " + eltType.ToString() + " array was " + oldSize + " long, now is " + newSize);
				}
				newArray = System.Array.CreateInstance(eltType, newSize);
				int valueToCopy = System.Math.Min(oldSize, newSize);
				if (valueToCopy > 0) {
					System.Array.Copy(array, newArray, valueToCopy);
				}
				if (fillUpRefArraysWithNewElements && newSize > oldSize) {
					for (int i = oldSize; i < newSize; i++) {
						newArray.SetValue(System.Activator.CreateInstance(eltType), i);
					}
				}
			}
		} else {
			if (logWarningIfResized && array != null && array.Length > 0) {
				//Debug.LogWarning(warningHeader + " Array Resize Warning : " + eltType.ToString() + " array was " + array.Length + " long, now is null");
			}
		}
		return newArray;
	}
}