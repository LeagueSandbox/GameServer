using System;

using dtStatus = System.UInt32;
using System.Numerics;
#if DT_POLYREF64
using dtPolyRef = System.UInt64;
#else
//typedef uint dtPolyRef;
using dtPolyRef = System.UInt32;
#endif

//High level interface for placing path finding requests to a System Helper.
public static partial class RcdtcsUnityUtils{

	public class StraightPath {
		public const int MAX_POLYS = 256;
		
		public float[] m_straightPath = new float[MAX_POLYS*3];
		public byte[] m_straightPathFlags = new byte[MAX_POLYS];
		public dtPolyRef[] m_straightPathPolys = new dtPolyRef[MAX_POLYS];
		public dtPolyRef[] m_RawPathPolys = new dtPolyRef[MAX_POLYS];
		public int m_straightPathCount = 0;
		public int m_straightPathOptions = 0;
		public int m_RawPathLength = 0;
	}

	public class SmoothPath {
		public const int MAX_POLYS = 256;
		public const int MAX_SMOOTH = 2048;
		
		public int m_nsmoothPath = 0;
		public float[] m_smoothPath = new float[MAX_SMOOTH*3];
	}

	public class PathHelper {
		
		Vector3[] m_Path = null;
		float[] m_Distances = null;
		float m_Length = 0.0f;
		
		public void Init(float[] path, int pointCount) {
			m_Path = new Vector3[pointCount];
			m_Distances = new float[pointCount];
			m_Length = 0.0f;
			int coordCount = pointCount * 3;
			for (int i = 0; i < coordCount; i += 3) {
				int v = i / 3;
				m_Path[v].X = path[i];
				m_Path[v].Y = path[i+1];
				m_Path[v].Z = path[i+2];
				if (i > 0) {
                    //var distance = Vector3
					float d = Vector3.Distance(m_Path[v - 1], m_Path[v]);
					m_Distances[v - 1] = d;
					m_Length += d;
				}
			}
		}
		
		public Vector3 GetPos(float d) {
			if (m_Path == null || m_Path.Length == 0) {
				return Vector3.Zero;
			}
			if (d == 0.0f) {
				return m_Path[0];
			}
			else if (d >= m_Length) {
				return m_Path[m_Path.Length - 1];
			} else {
				for (int i = 0; i < m_Path.Length - 1; ++i) {
					float segmentD = m_Distances[i];
					if (d < segmentD) {
						float ratio = d / segmentD;
						return Vector3.Lerp(m_Path[i], m_Path[i+1], ratio);
					} else {
						d -= segmentD;
					}
				}
			}
			return m_Path[0];
		}
		
		public float GetLength() {
			return m_Length;
		}
	}

	
	static bool inRange(float[] v1, int v1Start, float[] v2, int v2Start, float r, float h)
	{
		float dx = v2[v2Start + 0] - v1[v1Start + 0];
		float dy = v2[v2Start + 1] - v1[v1Start + 1];
		float dz = v2[v2Start + 2] - v1[v1Start + 2];
		return (dx*dx + dz*dz) < r*r && Math.Abs(dy) < h;
	}
	
	static bool getSteerTarget(Detour.dtNavMeshQuery navQuery, float[] startPos, float[] endPos,
	                           float minTargetDist,
	                           dtPolyRef[] path, int pathSize,
	                           float[] steerPos, ref byte steerPosFlag, ref dtPolyRef steerPosRef,
	                           ref float[] outPoints, ref int outPointCount)							 
	{
		// Find steer target.
		const int MAX_STEER_POINTS = 3;
		float[] steerPath = new float[MAX_STEER_POINTS*3];
		byte[] steerPathFlags = new byte[MAX_STEER_POINTS];
		dtPolyRef[] steerPathPolys = new dtPolyRef[MAX_STEER_POINTS];
		int nsteerPath = 0;
		navQuery.findStraightPath(startPos, endPos, path, pathSize,
		                          steerPath, steerPathFlags, steerPathPolys, ref nsteerPath, MAX_STEER_POINTS, 0);
		if (nsteerPath == 0)
			return false;
		
		//if (outPoints && outPointCount)
		//{
		outPointCount = nsteerPath;
		for (int i = 0; i < nsteerPath; ++i){
			Detour.dtVcopy(outPoints,i*3, steerPath,i*3);
		}
		//}
		
		
		// Find vertex far enough to steer to.
		int ns = 0;
		while (ns < nsteerPath)
		{
			// Stop at Off-Mesh link or when point is further than slop away.
			if ((steerPathFlags[ns] & (byte)Detour.dtStraightPathFlags.DT_STRAIGHTPATH_OFFMESH_CONNECTION) != 0 ||
			    !inRange(steerPath, ns*3, startPos, 0, minTargetDist, 1000.0f))
				break;
			ns++;
		}
		// Failed to find good point to steer to.
		if (ns >= nsteerPath)
			return false;
		
		Detour.dtVcopy(steerPos, 0, steerPath,ns*3);
		steerPos[1] = startPos[1];
		steerPosFlag = steerPathFlags[ns];
		steerPosRef = steerPathPolys[ns];
		
		return true;
	}
	
	static bool getSteerTarget(Detour.dtNavMeshQuery navQuery, float[] startPos, float[] endPos,
	                           float minTargetDist,
	                           dtPolyRef[] path, int pathSize,
	                           float[] steerPos, ref byte steerPosFlag, ref dtPolyRef steerPosRef)							 
	{
		// Find steer target.
		const int MAX_STEER_POINTS = 3;
		float[] steerPath = new float[MAX_STEER_POINTS*3];
		byte[] steerPathFlags = new byte[MAX_STEER_POINTS];
		dtPolyRef[] steerPathPolys = new dtPolyRef[MAX_STEER_POINTS];
		int nsteerPath = 0;
		navQuery.findStraightPath(startPos, endPos, path, pathSize,
		                          steerPath, steerPathFlags, steerPathPolys, ref nsteerPath, MAX_STEER_POINTS, 0);
		if (nsteerPath == 0)
			return false;
		
		// Find vertex far enough to steer to.
		int ns = 0;
		while (ns < nsteerPath)
		{
			// Stop at Off-Mesh link or when point is further than slop away.
			if ((steerPathFlags[ns] & (byte)Detour.dtStraightPathFlags.DT_STRAIGHTPATH_OFFMESH_CONNECTION) != 0 ||
			    !inRange(steerPath, ns*3, startPos, 0, minTargetDist, 1000.0f))
				break;
			ns++;
		}
		// Failed to find good point to steer to.
		if (ns >= nsteerPath)
			return false;
		
		Detour.dtVcopy(steerPos, 0, steerPath,ns*3);
		steerPos[1] = startPos[1];
		steerPosFlag = steerPathFlags[ns];
		steerPosRef = steerPathPolys[ns];
		
		return true;
	}
	
	static int fixupCorridor(dtPolyRef[] path, int npath, int maxPath,
	                         dtPolyRef[] visited, int nvisited)
	{
		int furthestPath = -1;
		int furthestVisited = -1;
		
		// Find furthest common polygon.
		for (int i = npath-1; i >= 0; --i)
		{
			bool found = false;
			for (int j = nvisited-1; j >= 0; --j)
			{
				if (path[i] == visited[j])
				{
					furthestPath = i;
					furthestVisited = j;
					found = true;
				}
			}
			if (found)
				break;
		}
		
		// If no intersection found just return current path. 
		if (furthestPath == -1 || furthestVisited == -1)
			return npath;
		
		// Concatenate paths.	
		
		// Adjust beginning of the buffer to include the visited.
		int req = nvisited - furthestVisited;
		int orig = Math.Min(furthestPath+1, npath);
		int size = Math.Max(0, npath-orig);
		if (req+size > maxPath)
			size = maxPath-req;
		if (size != 0){
			//memmove(path+req, path+orig, size*sizeof(dtPolyRef));
			dtPolyRef[] buf = new dtPolyRef[size];
			for (int i=0;i<size;++i){
				buf[i] = path[orig+i];
			}
			for (int i=0;i<size;++i){
				path[req+i] = buf[i];
			}
			/*
			for (int i=0;i<size;++i){

				path[req + i] = path[orig + i];
			}
			*/
		}
		
		// Store visited
		for (int i = 0; i < req; ++i)
			path[i] = visited[(nvisited-1)-i];				
		
		return req+size;
	}
	
	// This function checks if the path has a small U-turn, that is,
	// a polygon further in the path is adjacent to the first polygon
	// in the path. If that happens, a shortcut is taken.
	// This can happen if the target (T) location is at tile boundary,
	// and we're (S) approaching it parallel to the tile edge.
	// The choice at the vertex can be arbitrary, 
	//  +---+---+
	//  |:::|:::|
	//  +-S-+-T-+
	//  |:::|   | <-- the step can end up in here, resulting U-turn path.
	//  +---+---+
	static int fixupShortcuts(dtPolyRef[] path, int npath, Detour.dtNavMeshQuery navQuery)
	{
		if (npath < 3)
			return npath;
		
		// Get connected polygons
		const int maxNeis = 16;
		dtPolyRef[] neis = new dtPolyRef[maxNeis];
		int nneis = 0;
		
		Detour.dtMeshTile tile = null;
		Detour.dtPoly poly = null;
		if (Detour.dtStatusFailed(navQuery.getAttachedNavMesh().getTileAndPolyByRef(path[0], ref tile, ref poly)))
			return npath;
		
		for (uint k = poly.firstLink; k != Detour.DT_NULL_LINK; k = tile.links[k].next)
		{
			Detour.dtLink link = tile.links[k];
			if (link.polyRef != 0)
			{
				if (nneis < maxNeis)
					neis[nneis++] = link.polyRef;
			}
		}
		
		// If any of the neighbour polygons is within the next few polygons
		// in the path, short cut to that polygon directly.
		const int maxLookAhead = 6;
		int cut = 0;
		for (int i = Math.Min(maxLookAhead, npath) - 1; i > 1 && cut == 0; i--) {
			for (int j = 0; j < nneis; j++)
			{
				if (path[i] == neis[j]) {
					cut = i;
					break;
				}
			}
		}
		if (cut > 1)
		{
			int offset = cut-1;
			npath -= offset;
			for (int i = 1; i < npath; i++)
				path[i] = path[i+offset];
		}
		
		return npath;
	}
	
	
	public static float[] Vector3ToArray(Vector3 vec){
		float[] arr = new float[3];
		arr[0] = vec.X;
		arr[1] = vec.Y;
		arr[2] = vec.Z;
		return arr;
	}
	
	public static Vector3 ArrayToVector3(float[] pos, int start = 0) {
		return new Vector3(pos[start], pos[start + 1], pos[start + 2]);
	}
	
	public static StraightPath ComputeStraightPath(Detour.dtNavMeshQuery navQuery, Vector3 startPos, Vector3 endPos){
		return ComputeStraightPath(navQuery, Vector3ToArray(startPos), Vector3ToArray(endPos));
	}
	
	public static StraightPath ComputeStraightPath(Detour.dtNavMeshQuery navQuery, float[] startPos, float[] endPos){
		//m_ComputedPathType = PathType.Straight;
		
		StraightPath path = new StraightPath();
		
		float[] extents = new float[3];
		for (int i=0;i<3;++i){
			extents[i] = 10.0f;
		}
		
		dtPolyRef startRef = 0;
		dtPolyRef endRef = 0;
		
		float[] startPt = new float[3];
		float[] endPt = new float[3];
		
		Detour.dtQueryFilter filter = new Detour.dtQueryFilter();
		
		navQuery.findNearestPoly( startPos, extents, filter, ref startRef, ref startPt );
		navQuery.findNearestPoly( endPos, extents, filter, ref endRef, ref endPt );
		
		int pathCount = -1;
		
		navQuery.findPath(startRef, endRef, startPt, endPt, filter, path.m_RawPathPolys, ref pathCount, StraightPath.MAX_POLYS);
		
		path.m_RawPathLength = pathCount;
		
		if (pathCount > 0)
		{
			// In case of partial path, make sure the end point is clamped to the last polygon.
			float[] epos = new float[3];
			Detour.dtVcopy(epos, endPt);
			if (path.m_RawPathPolys[pathCount - 1] != endRef) {
				bool posOverPoly = false;
				navQuery.closestPointOnPoly(path.m_RawPathPolys[pathCount - 1], endPt, epos, ref posOverPoly);
			}
			
			navQuery.findStraightPath(startPt, endPt, path.m_RawPathPolys, pathCount,
			                          path.m_straightPath, path.m_straightPathFlags, 
			                          path.m_straightPathPolys, ref path.m_straightPathCount,
			                          StraightPath.MAX_POLYS, path.m_straightPathOptions);
		}
		
		return path;
	}
	
	public static float[] GetClosestPointOnNavMesh(Detour.dtNavMeshQuery navQuery, float[] pos) {
		
		float[] extents = new float[3];
		for (int i = 0; i < 3; ++i) {
			extents[i] = 10.0f;
		}
		
		Detour.dtQueryFilter filter = new Detour.dtQueryFilter();
		dtPolyRef startRef = 0;
		float[] res = new float[3];
		
		navQuery.findNearestPoly(pos, extents, filter, ref startRef, ref res);
		
		return res;
	}
	
	public static SmoothPath ComputeSmoothPath(Detour.dtNavMeshQuery navQuery, Vector3 startPos, Vector3 endPos){
		return ComputeSmoothPath(navQuery, Vector3ToArray(startPos), Vector3ToArray(endPos));
	}
	
	public static SmoothPath ComputeSmoothPath(Detour.dtNavMeshQuery navQuery, float[] startWorldPos, float[] endWorldPos){
		
		SmoothPath smoothPath = new SmoothPath();
		
		if (navQuery == null){
			return smoothPath;
		}
		
		float[] extents = new float[3];
		for (int i=0;i<3;++i){
			extents[i] = 10.0f;
		}
		
		dtPolyRef startRef = 0;
		dtPolyRef endRef = 0;
		
		float[] startPt = new float[3];
		float[] endPt = new float[3];
		
		Detour.dtQueryFilter filter = new Detour.dtQueryFilter();
		
		navQuery.findNearestPoly(startWorldPos, extents, filter, ref startRef, ref startPt);
		navQuery.findNearestPoly(endWorldPos, extents, filter, ref endRef, ref endPt);
		
		const int maxPath = SmoothPath.MAX_POLYS;
		dtPolyRef[] path = new dtPolyRef[maxPath]; 
		
		int pathCount = -1;
		
		navQuery.findPath(startRef, endRef, startPt, endPt, filter, path, ref pathCount, maxPath );
		
		smoothPath.m_nsmoothPath = 0;
		
		if (pathCount > 0)
		{
			// Iterate over the path to find smooth path on the detail mesh surface.
			dtPolyRef[] polys = new dtPolyRef[SmoothPath.MAX_POLYS];
			for (int i=0;i<pathCount;++i){
				polys[i] = path[i];
			}
			int npolys = pathCount;
			
			float[] iterPos = new float[3];
			float[] targetPos = new float[3];
			bool posOverPoly_dummy = false;
			navQuery.closestPointOnPoly(startRef, startPt, iterPos, ref posOverPoly_dummy);
			navQuery.closestPointOnPoly(polys[npolys-1], endPt, targetPos, ref posOverPoly_dummy);
			
			const float STEP_SIZE = 0.5f;
			const float SLOP = 0.01f;
			
			smoothPath.m_nsmoothPath = 0;
			
			Detour.dtVcopy(smoothPath.m_smoothPath,smoothPath.m_nsmoothPath*3, iterPos, 0);
			smoothPath.m_nsmoothPath++;
			
			// Move towards target a small advancement at a time until target reached or
			// when ran out of memory to store the path.
			while (npolys != 0 && smoothPath.m_nsmoothPath < SmoothPath.MAX_SMOOTH)
			{
				// Find location to steer towards.
				float[] steerPos = new float[3];
				byte steerPosFlag = 0;
				dtPolyRef steerPosRef = 0;
				
				if (!getSteerTarget(navQuery, iterPos, targetPos, SLOP,
				                    polys, npolys, steerPos, ref steerPosFlag, ref steerPosRef))
					break;
				
				bool endOfPath = (steerPosFlag & (byte)Detour.dtStraightPathFlags.DT_STRAIGHTPATH_END) != 0 ? true : false;
				bool offMeshConnection = (steerPosFlag & (byte)Detour.dtStraightPathFlags.DT_STRAIGHTPATH_OFFMESH_CONNECTION) != 0 ? true : false;
				
				// Find movement delta.
				float[] delta = new float[3];//, len;
				float len = .0f;
				Detour.dtVsub(delta, steerPos, iterPos);
				len = (float)Math.Sqrt(Detour.dtVdot(delta,delta));
				// If the steer target is end of path or off-mesh link, do not move past the location.
				if ((endOfPath || offMeshConnection) && len < STEP_SIZE)
					len = 1;
				else
					len = STEP_SIZE / len;
				float[] moveTgt = new float[3];
				Detour.dtVmad(moveTgt, iterPos, delta, len);
				
				// Move
				float[] result = new float[3];
				dtPolyRef[] visited = new dtPolyRef[16];
				int nvisited = 0;
				navQuery.moveAlongSurface(polys[0], iterPos, moveTgt, filter,
				                          result, visited, ref nvisited, 16);
				
				npolys = fixupCorridor(polys, npolys, SmoothPath.MAX_POLYS, visited, nvisited);
				npolys = fixupShortcuts(polys, npolys, navQuery);
				
				float h = 0;
				dtStatus getHeightStatus = navQuery.getPolyHeight(polys[0], result, ref h);
				result[1] = h;
				
				if ((getHeightStatus & Detour.DT_FAILURE) != 0) {
					//Debug.LogError("Failed to getPolyHeight " + polys[0] + " pos " + result[0] + " " + result[1] + " " + result[2] + " h " + h);
				}
				
				Detour.dtVcopy(iterPos, result);
				
				// Handle end of path and off-mesh links when close enough.
				if (endOfPath && inRange(iterPos, 0, steerPos, 0, SLOP, 1.0f))
				{
					// Reached end of path.
					Detour.dtVcopy(iterPos, targetPos);
					if (smoothPath.m_nsmoothPath < SmoothPath.MAX_SMOOTH)
					{
						Detour.dtVcopy(smoothPath.m_smoothPath,smoothPath.m_nsmoothPath*3, iterPos, 0);
						smoothPath.m_nsmoothPath++;
					}
					break;
				}
				else if (offMeshConnection && inRange(iterPos, 0, steerPos, 0, SLOP, 1.0f))
				{
					// Reached off-mesh connection.
					float[] startPos = new float[3];//, endPos[3];
					float[] endPos = new float[3];
					
					// Advance the path up to and over the off-mesh connection.
					dtPolyRef prevRef = 0, polyRef = polys[0];
					int npos = 0;
					while (npos < npolys && polyRef != steerPosRef)
					{
						prevRef = polyRef;
						polyRef = polys[npos];
						npos++;
					}
					for (int i = npos; i < npolys; ++i)
						polys[i-npos] = polys[i];
					
					npolys -= npos;
					
					// Handle the connection.
					
					dtStatus status = navQuery.getAttachedNavMesh().getOffMeshConnectionPolyEndPoints(prevRef, polyRef, startPos, endPos);
					if (Detour.dtStatusSucceed(status))
					{
						if (smoothPath.m_nsmoothPath < SmoothPath.MAX_SMOOTH)
						{
							Detour.dtVcopy(smoothPath.m_smoothPath,smoothPath.m_nsmoothPath*3, startPos, 0);
							smoothPath.m_nsmoothPath++;
							// Hack to make the dotted path not visible during off-mesh connection.
							if ((smoothPath.m_nsmoothPath & 1) != 0)
							{
								Detour.dtVcopy(smoothPath.m_smoothPath, smoothPath.m_nsmoothPath * 3, startPos, 0);
								smoothPath.m_nsmoothPath++;
							}
						}
						// Move position at the other side of the off-mesh link.
						Detour.dtVcopy(iterPos, endPos);
						float eh = 0.0f;
						navQuery.getPolyHeight(polys[0], iterPos, ref eh);
						iterPos[1] = eh;
					}
				}
				
				// Store results.
				if (smoothPath.m_nsmoothPath < SmoothPath.MAX_SMOOTH)
				{
					Detour.dtVcopy(smoothPath.m_smoothPath, smoothPath.m_nsmoothPath * 3, iterPos, 0);
					smoothPath.m_nsmoothPath++;
				}
			}
		}
		return smoothPath;
	}
}