using System;
using System.Collections.Generic;
using System.Numerics;
using System.Collections;
using System.Diagnostics;
using Priority_Queue;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Maps;
using LeagueSandbox.GameServer.Logic.RAF;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    class Pathfinder
    {
        protected static Map map;
        protected static int successes = 0, oot = 0, empties = 0;
        protected static int totalDuration = 0, durations = 0;
        protected static DateTime g_Clock = System.DateTime.Now;
        protected static bool debugOutput = false;

        public Pathfinder()/*:mesh(0),chart(0)*/ { }


        public static Path getPath(Vector2 from, Vector2 to)
        {
            Path path = new Path();
            PathJob job = new PathJob(getMesh());


            if (map == null) Logger.LogCoreError("Tried to find a path without setting the map.");
            if (getMesh() == null) Logger.LogCoreError("Can't start pathfinding without initialising the AIMesh");

            int fromTri = getMesh().GetTriAtPos(from);

            if (fromTri<0)
            {
                Logger.LogCoreError("Trying to find path from invalid place! " + from);
                path.error = PathError.PATH_ERROR_INVALID_START;
                return path;
            }
           
            int toTri = getMesh().GetTriAtPos(to);
            if(toTri<0)
            {
                if(debugOutput)
                    Logger.LogCoreInfo("Pathfinder: Invalid destination, finding closest point");
                
                to = getMesh().GetClosestTerrainExit(to);

                toTri = getMesh().GetTriAtPos(to);
            }

        
            if (fromTri==toTri) //simple path
            {
                path.waypoints.Add(from);
                path.waypoints.Add(to);
                return path;
            }


            if (debugOutput)
            {
                Logger.LogCoreInfo("Going from (" + from + ") to (" + to + ")");
            }

            job.start = from;
            job.destination = to;
            job.fromIndex = fromTri;
            job.toIndex = toTri;

            List<int> navTriPath = null;
            if (debugOutput)
            {
                Stopwatch sw = new Stopwatch();
                sw.Restart();
                navTriPath = job.FindNavPath();
                sw.Stop();
                Logger.LogCoreInfo("Finding path in navmesh took " + sw.ElapsedMilliseconds + "ms");
            }
            else
            navTriPath = job.FindNavPath();
            
            if(navTriPath.Count==0)
            {
                Logger.LogCoreError("Could not find path on NavMesh!");
                path.error = PathError.PATH_ERROR_NO_PATH;
                return path;
            }

            
            if(debugOutput)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                path.waypoints = job.FunnelPath(navTriPath, from, to);
                sw.Stop();
                Logger.LogCoreInfo("Path funneling took " + sw.ElapsedMilliseconds + "ms");
            }
            else
                path.waypoints = job.FunnelPath(navTriPath, from, to);

            if(debugOutput)
                DebugHelper.getInstance().ImageAddTriangles(getMesh(), navTriPath, System.Drawing.Color.Yellow, new List<Vector2>() { from, to }, path.waypoints);

            return path;
        }

        public static void setMap(Map nMap)
        {
            map = nMap;
        }
        public static AIMesh getMesh()
        {
            if (map == null)
            {
                Logger.LogCoreError("The map hasn't been set but the mesh was requested.");
                return null;
            }
            return map.getAIMesh();
        }


    }

    
    class Path
    {
        public PathError error = PathError.PATH_ERROR_NONE;
        public List<Vector2> waypoints;

        public Path()
        {
            waypoints = new List<Vector2>();
        }
        public bool isPathed()
        {
            return error == PathError.PATH_ERROR_NONE;
        }
        public PathError getError()
        {
            return error;
        }

        public List<Vector2> getWaypoints()
        {
            return waypoints;
        }
    };


    /// <summary>
    /// Holds together data needed for NavMesh pathfinding
    /// </summary>
    internal struct PathJob
    {
        private AIMesh _mesh;
        private SimplePriorityQueue<PathNode> _openList;
        private BitArray _openListMask;
        private PathNode[] _nodeArray; //used for node lookups
        private BitArray _closedListMask;


        public Vector2 destination { get; set; }
        public Vector2 start { get; set; }

        public int fromIndex { get; set; }
        public int toIndex { get; set; }

        

        public PathJob(AIMesh mesh)
        {
            this._mesh = mesh;
            this._openList = new SimplePriorityQueue<PathNode>();
            this._nodeArray = new PathNode[_mesh.TriangleCount];
            this._closedListMask = new BitArray(_mesh.TriangleCount);
            this._openListMask = new BitArray(_mesh.TriangleCount);
            this.destination = new Vector2();
            this.start = new Vector2();
            this.fromIndex = -1;
            this.toIndex = -1;
        }

        /// <summary>
        /// Finds all NavMesh triangles in a path (A*)
        /// </summary>
        public List<int> FindNavPath()
        {
            int triangleCount = _mesh.TriangleCount;

            if (!(fromIndex >= 0 && fromIndex < triangleCount))
                return new List<int>();

            if (!(toIndex >= 0 && toIndex < triangleCount))
                return new List<int>();


            PathNode startNode = new PathNode(fromIndex, null, start);
            startNode.point = start;
            startNode.SetScore(0, CalcNodeH(startNode));
            
            AddToOpen(startNode);

            PathNode currentNode = null;
            while (_openList.Count > 0)
            {
                currentNode = _openList.First;
                _openList.Remove(currentNode);

                if (currentNode.Index == toIndex)
                    return ReconstructPathFromNode(currentNode);


                AddToClosed(currentNode);
                ExpandNode(currentNode);
            }

            //should not get this far (navMesh is probably disconnected)
            Logger.LogCoreError("Could not find path in NavMesh, Is NavMesh fully connected?");
            return new List<int>();
        }

        
        private void AddToOpen(PathNode node)
        {
            _nodeArray[node.Index] = node;
            _closedListMask.Set(node.Index, false);
            _openList.Enqueue(node, node.GetScore());
        }

        private List<int> ReconstructPathFromNode(PathNode last)
        {

            List<int> ret = new List<int>();
            if (last == null)
            {
                Logger.LogCoreWarning("Tried to reconstruct path from invalid node!");
                return ret;
            }

            //go through node parents back to the first node
            do
            {
                ret.Add(last.Index);
                last = last.Parent;
            } while (last != null);

            ret.Reverse();

            return ret;
        }

        private float CalcNodeG(PathNode parent,Vector2 nodePos)
        {
            return Vector2.Distance(parent.point, nodePos) + parent.g;
        }

        private float CalcNodeH(PathNode node)
        {
            if (node.Index == toIndex)
                return 0;
            else
                return Vector2.Distance(node.point, destination);
        }

        private void ExpandNode(PathNode node)
        {
            List<int> neighbours = _mesh.GetTriNeighbours(node.Index);
            foreach (int nIndex in neighbours)
            {
                if (IsOpen(nIndex) || IsClosed(nIndex)) //this implementation allows updating of closed list
                {
                    PathNode neighbour = _nodeArray[nIndex];
                    float newG = CalcNodeG(node, neighbour.point);
                    
                    if (newG < neighbour.g)
                    {
                        neighbour.SetScore(newG, neighbour.h);
                        neighbour.Parent = node;

                        if(IsClosed(nIndex))
                        {
                            AddToOpen(neighbour);
                        }
                        else
                            _openList.UpdatePriority(neighbour, neighbour.GetScore());
                    }

                }
                else
                {
                    Vector2 midpoint = GetEdgeMidpoint(node.Index, nIndex);
                    PathNode newNode = new PathNode(nIndex, node, midpoint);
                    newNode.SetScore(CalcNodeG(node, newNode.point), CalcNodeH(newNode));
                    
                    AddToOpen(newNode);
                }
            }

        }

        private void AddToClosed(PathNode node)
        {
            _openListMask.Set(node.Index, false);
            _closedListMask.Set(node.Index, true);
        }

        private bool IsClosed(int triIndex)
        {
            return _closedListMask.Get(triIndex);
        }

        private bool IsOpen(int triIndex)
        {
            return _openListMask.Get(triIndex);
        }

        /// <summary>
        /// Uses Simple Stupid Funnel algorithm to get the real path out of nav path
        /// </summary>
        internal List<Vector2> FunnelPath(List<int> navTriPath, Vector2 from, Vector2 to)
        {
            List<Vector2> path = new List<Vector2>();
            path.Add(from);

            if (navTriPath.Count < 2)
                return path;

            List<Tuple<Vector2, Vector2>> portals = FindPortals(navTriPath);

            Vector2 portalApex = from;
            Vector2 portalLeft = portals[0].Item1;
            Vector2 portalRight = portals[0].Item2;

            int rightIndex = 0;
            int leftIndex = 0;

            Vector2 prevLeftSegment = new Vector2();
            Vector2 prevRightSegment = new Vector2();

            //Right side
            //Does new 'right' reduce the funnel?
            for (int i = 0; i < portals.Count; i++)
            {
                
                Vector2 left = portals[i].Item1;
                Vector2 right = portals[i].Item2;

                Vector2 curLeftSegment = left - portalApex;
                Vector2 curRightSegment = right - portalApex;

                //right vertex
                if (MathExt.Vec2Cross(prevRightSegment,curRightSegment)>-MathExt.FP_TOLERANCE)
                {
                    // Does it NOT cross the left side?
                    // Is the apex the same as portal right? (if true, no chance but to move)
                    if (MathExt.VectorEqual(portalApex,portalRight) ||
                        MathExt.Vec2Cross(prevLeftSegment,curRightSegment) < MathExt.FP_TOLERANCE)
                    {
                        //Tighten the funnel
                        portalRight = right;
                        prevRightSegment = curRightSegment;
                        rightIndex = i;
                    }
                    else
                    {
                        //Collapse
                        portalApex = portalLeft;
                        portalRight = portalApex;

                        path.Add(portalApex);

                        i = leftIndex;
                        rightIndex = leftIndex;

                        prevLeftSegment = Vector2.Zero;
                        prevRightSegment = Vector2.Zero;

                        continue;
                    }



                }
                // Left Side
                // Does new 'left' reduce the funnel?
                if (MathExt.Vec2Cross(prevLeftSegment,curLeftSegment)< MathExt.FP_TOLERANCE)
                {
                    // Does it NOT cross the right side?
                    // Is the apex the same as portal left? (if true, no chance but to move)
                    if (MathExt.VectorEqual(portalApex, portalLeft) ||
                        MathExt.Vec2Cross(prevRightSegment,curLeftSegment)> -MathExt.FP_TOLERANCE)
                    {
                        // Tighten the funnel.
                        portalLeft = left;
                        prevLeftSegment = curLeftSegment;
                        leftIndex = i;
                    }
                    else
                    {
                        //Collapse
                        portalApex = portalRight;
                        portalLeft = portalApex;

                        path.Add(portalRight);

                        leftIndex = rightIndex;
                        i = rightIndex;

                        prevLeftSegment = Vector2.Zero;
                        prevRightSegment = Vector2.Zero;

                        continue;
                    }
                }
            }

            path.Add(to);

            return path;
        }


        private bool CmpSign(float a, float b)
        {
            return (a >= 0 && b >= 0) || (b < 0 && a < 0);
        }

        private float TriArea(Vector2 a, Vector2 b, Vector2 c)
        {
            float ax = b.X - a.X;
            float ay = b.Y - a.Y;
            float bx = c.X - a.X;
            float by = c.Y - a.Y;
            return bx * ay - ax * by;
        }


        /// <summary>
        /// Finds shared vertices between navmesh triangles
        /// </summary>
        private List<Tuple<Vector2, Vector2>> FindPortals(List<int> navTriPath)
        {
            List<Tuple<Vector2, Vector2>> ret = new List<Tuple<Vector2, Vector2>>();
            for (int i = 0; i < navTriPath.Count - 1; i++)
            {

                var portal = _mesh.GetPortalPoints(navTriPath[i], navTriPath[i + 1]);
                ret.Add(portal);
            }
            return ret;
        }

        private Vector2 GetEdgeMidpoint(int fromIndex,int toIndex)
        {
            var portal = _mesh.GetPortalPoints(fromIndex, toIndex);
            return (portal.Item1 + portal.Item2) * 0.5f;
        }
    }

    class PathNode
    {
        public int Index { get; set; }

        public float h { get; protected set; } //heuristic distance to target
        public float g { get; protected set; } //calculated distance from origin

        public PathNode Parent { get; set; }

        public Vector2 point { get; set; } // closest point to parent node

        public PathNode(int triIndex, PathNode parent, Vector2 point)
        {
            this.Index = triIndex;
            this.Parent = parent;
            this.h = h;
            this.g = g;
            this.point = point;
        }
      
        public void SetScore(float ag, float ah)
        {
            this.g = ag;
            this.h = ah;
        }

        public float GetScore()
        {
            return g + h;
        }


    }
    public enum PathError
    {
        PATH_ERROR_NONE = 0,
        PATH_ERROR_NO_PATH = 1,
        PATH_ERROR_INVALID_START = 2
    };
}
