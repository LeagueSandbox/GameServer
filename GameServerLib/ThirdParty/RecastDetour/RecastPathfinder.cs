using System.IO;
using System.Collections.Generic;
using System;
using System.Numerics;
using LeagueSandbox.GameServer;

namespace RecastDetour
{
    public class RecastPathfinder
    {
        private RcdtcsUnityUtils.RecastMeshParams m_NavMeshParams = new RcdtcsUnityUtils.RecastMeshParams();
        private Dictionary<int, RcdtcsUnityUtils.SystemHelper> NavMeshes = new Dictionary<int, RcdtcsUnityUtils.SystemHelper>();

        public bool LoadMap(string filePath, int mapId)
        {
            try
            {
                RcdtcsUnityUtils.SystemHelper navmesh = new RcdtcsUnityUtils.SystemHelper();

                //Interface.Log("Loading navmesh.");

                NavMeshData navMeshData;
                using (var file = File.OpenRead(filePath))
                {
                    navMeshData = ProtoSerializer.Deserialize<NavMeshData>(file);
                }
                //Interface.Log("Navmesh loaded.");
                //
                navmesh.SetNavMeshParams(m_NavMeshParams);
                navmesh.ClearComputedData();
                navmesh.ClearMesh();
                //
                navmesh.AddMesh(navMeshData);
                navmesh.ComputeSystem();

                NavMeshes.Add(mapId, navmesh);
                return true;
            }
            catch(Exception ex)
            {
                //Interface.LogError(ex.StackTrace);
            }
            return false;
        }

        public Vector3[] GetPath(Vector3 from, Vector3 to, int mapId)
        {
            List<Vector3> path = new List<Vector3>();
            if(NavMeshes.ContainsKey(mapId))
            {
                var computedPath = RcdtcsUnityUtils.ComputeSmoothPath(NavMeshes[mapId].m_navQuery, from, to);
                for (int i = 1; i < computedPath.m_nsmoothPath; ++i)
                {
                    int v = i * 3;
                    //Vector3 a = new Vector3(path[v - 3], path[v - 2], path[v - 1]);
                    path.Add(new Vector3(computedPath.m_smoothPath[v + 0], computedPath.m_smoothPath[v + 1], computedPath.m_smoothPath[v + 2]));
                }
            }
            return path.ToArray();
        }

        public Vector3[] GetPath(Vector2 from, Vector3 to, int mapId)
        {
            return new Vector3[] { };
        }

        public Vector2[] GetPath2D(Vector2 from, Vector3 to, int mapId)
        {
            return new Vector2[] { };
        }
    }
}