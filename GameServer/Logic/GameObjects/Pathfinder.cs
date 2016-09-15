using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Maps;
using LeagueSandbox.GameServer.Logic.RAF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    class Pathfinder
    {
        private static Logger _logger = Program.ResolveDependency<Logger>();

        protected static Map chart;
        private const int GRID_SIZE = 1024;
        protected static int successes = 0, oot = 0, empties = 0;
        protected static int totalDuration = 0, durations = 0;
        protected static DateTime g_Clock = System.DateTime.Now;
        protected static bool debugOutput = false;
        protected static int MAX_PATHFIND_TRIES = 100;

        public Pathfinder()/*:mesh(0),chart(0)*/ { }

        public static Path getPath(Vector2 from, Vector2 to, float boxSize)
        {

            Path path = new Path();
            PathJob job = new PathJob();

            if ((System.DateTime.Now - g_Clock).Milliseconds > 4000 && (successes + oot + empties) > 0)
                _logger.LogCoreInfo(string.Format(
                    "Pathfinding successrate: {0}",
                    (float)successes / (float)(successes + oot + empties) * (100.0f)
                ));

            if (debugOutput)
                _logger.LogCoreInfo("Recording this minion movement.");

            if (chart == null) _logger.LogCoreError("Tried to find a path without setting the map.");
            if (getMesh() == null) _logger.LogCoreError("Can't start pathfinding without initialising the AIMesh");


            job.start = job.fromPositionToGrid(from); // Save start in grid info
            job.destination = job.fromPositionToGrid(to); // Save destination in grid info

            if (debugOutput)
            {
                _logger.LogCoreInfo(string.Format(
                    "Going from ({0}, {1}) to ({2}, {3})",
                    job.start.X,
                    job.start.Y,
                    job.destination.X,
                    job.destination.Y
                ));
            }

            job.insertObstructions(chart, getMesh()); // Ready the map.

            job.addToOpenList(job.start, null); // Let's start at the start.

            int tries;
            for (tries = 0; job.openList.Count != 0; tries++) // Go through the open list while it's not empty
            {
                if (debugOutput)
                    _logger.LogCoreInfo("Going through openlist. Tries: " + tries + " | Objects on list: " + job.openList.Count);


                if (tries == MAX_PATHFIND_TRIES)
                {
                    path.error = PathError.PATH_ERROR_OUT_OF_TRIES;
                    oot++;
                    //CORE_WARNING("PATH_ERROR_OUT_OF_TRIES");
                    path.waypoints = job.reconstructUnfinishedPath();
                    job.cleanPath(path);
                    job.cleanLists();
                    return path;
                }
                else if (job.traverseOpenList(tries == 0))
                {
                    path.error = PathError.PATH_ERROR_NONE;
                    successes++;
                    //CORE_INFO("We finished a path.");
                    path.waypoints = job.reconstructPath();
                    job.cleanPath(path);
                    job.cleanLists();
                    return path;
                }
            }

            if (debugOutput)
                _logger.LogCoreInfo("Going through openlist. Tries: " + tries + " | Objects on list: " + job.openList.Count);

            //CORE_WARNING("PATH_ERROR_OPENLIST_EMPTY");
            path.error = PathError.PATH_ERROR_OPENLIST_EMPTY;
            empties++;
            path.waypoints.Add(from);
            job.cleanPath(path);
            job.cleanLists();
            return path;
        }
        public static Path getPath(Vector2 from, Vector2 to)// { if (!chart->getAIMesh()) CORE_FATAL("Can't get path because of a missing AIMesh."); return getPath(from, to, PATH_DEFAULT_BOX_SIZE(mesh->getSize())); }
        {
            if (chart.AIMesh == null)
            {
                _logger.LogCoreError("Can't get path because of a missing AIMesh.");
            }
            return getPath(from, to, PATH_DEFAULT_BOX_SIZE(getMesh().getSize()));
        }
        public static void setMap(Map map)// { chart = map; mesh = chart->getAIMesh(); }
        {
            chart = map;
        }
        public static AIMesh getMesh()
        {
            if (chart == null)
            {
                _logger.LogCoreError("The map hasn't been set but the mesh was requested.");
                return null;
            }
            return chart.AIMesh;
        }
        private static float PATH_DEFAULT_BOX_SIZE(float map_size)
        {
            return map_size / (float)GRID_SIZE;
        }
    }
    class Path
    {
        public PathError error = PathError.PATH_ERROR_NONE;
        public List<Vector2> waypoints;

        public bool isPathed()
        {
            return error == PathError.PATH_ERROR_NONE;
        }
        public PathError getError()
        {
            return error;
        }
        //std::vector<Vector2> getWaypoints() { return waypoints; }
        public List<Vector2> getWaypoints()
        {
            return waypoints;
        }
    };

    class PathJob
    {
        private Logger _logger = Program.ResolveDependency<Logger>();
        private static int GRID_SIZE = 1024;
        private static int GRID_WIDTH = GRID_SIZE;
        private static int GRID_HEIGHT = GRID_SIZE;
        public List<PathNode> openList, closedList;
        public Grid[,] map = new Grid[GRID_WIDTH, GRID_HEIGHT];
        public Vector2 start, destination;

        public PathJob()
        {
            openList = new List<PathNode>();
            closedList = new List<PathNode>();
            start = new Vector2();
            destination = new Vector2();
            for (var i = 0; i < GRID_WIDTH; i++)
                for (var j = 0; j < GRID_HEIGHT; j++)
                    map[i, j] = new Grid();
        }

        public Vector2 fromGridToPosition(Vector2 position)
        {
            AIMesh mesh = Pathfinder.getMesh();
            if (mesh == null)
            {
                _logger.LogCoreError("Tried to get a grid location without an initialised AIMesh!");
                return new Vector2();
            }

            return position * PATH_DEFAULT_BOX_SIZE(mesh.getSize());
        }

        public Vector2 fromPositionToGrid(Vector2 position)
        {
            AIMesh mesh = Pathfinder.getMesh();
            if (mesh == null)
            {
                _logger.LogCoreError("Tried to get a position without an initialised AIMesh!");
                return new Vector2();
            }

            return (position / (float)PATH_DEFAULT_BOX_SIZE(mesh.getSize()));
        }

        public List<Vector2> reconstructPath()
        {
            List<Vector2> ret = new List<Vector2>();
            var i = closedList.ToList();
            i.Reverse();

            foreach (var v in i)
                ret.Add(fromGridToPosition(new Vector2(v.x, v.y)));

            return ret;
        }

        public List<Vector2> reconstructUnfinishedPath()
        {
            List<Vector2> ret = new List<Vector2>();

            var a = closedList.ToList();
            a.Reverse();
            PathNode pathnode = null;

            var lowestG = 9e7;
            foreach (var i in a)
            {
                if (i.g < lowestG)
                {
                    lowestG = i.g;
                    pathnode = i;
                }
            }
            if (pathnode != null)
                a.RemoveRange(0, a.IndexOf(pathnode) + 1);

            foreach (var i in a)
                ret.Add(fromGridToPosition(new Vector2(i.x, i.y)));

            return ret;
        }
        public void cleanPath(Path path)
        {
            return;
            /* if (path.waypoints.size() < 2) return;
             int startSize = path.waypoints.size();
             CORE_WARNING("Cleaning path.. Current size is %d", startSize);

             int dirX = 0, dirY = 0;
             auto prevPoint = path.waypoints.begin();
             for (auto i = path.waypoints.begin() + 1; i != path.waypoints.end(); i++)
             {
                 if (((*i).X - (*prevPoint).X == dirX) &&
                    ((*i).Y - (*prevPoint).Y == dirY))
                 {
                     path.waypoints.erase(prevPoint);
                     CORE_WARNING("Erased a waypoint");
                 }
                 else
                 {
                     dirX = ((*i).X - (*prevPoint).X);
                     dirY = ((*i).Y - (*prevPoint).Y);
                 }

                 prevPoint = i;
             }

             CORE_WARNING("Done cleaning. New size is %d", path.waypoints.size());
             if (startSize != path.waypoints.size())
                 CORE_WARNING("Removed %d nodes", startSize - path.waypoints.size());*/
        }
        public bool traverseOpenList(bool first)
        {
            if (openList.Count == 0)
            {
                return false;
            }

            // This sorts every iteration, which means that everything but the last couple of elements are sorted.
            // TODO: That means, this can probably be optimised. Sort only the last elements and add them into the vector where they belong.
            // But honestly, it's running pretty fast so why bother
            openList.Sort((a, b) => (a.g + a.h).CompareTo(b.g + b.h));


            PathNode currentNode = openList.Last();
            openList.RemoveAt(openList.Count - 1);

            bool atDestination = (Math.Abs(currentNode.x - (int)destination.X) <= 1 && Math.Abs(currentNode.y - (int)destination.Y) <= 1);

            if (!atDestination) // While we're not there
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (currentNode.x + dx >= 0 && currentNode.x + dx < GRID_WIDTH) // Search in 8 directions, but we're supposed to stay in map
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            if (!(dx == 0 && dy == 0)) // in all 8 directions, ignore the x==y==0 where we dont move
                            {
                                if (!isGridNodeOccupied(currentNode.x + dx, currentNode.y + dy)) // Is something here?
                                {
                                    PathNode conflictingNode = isNodeOpen(currentNode.x + dx, currentNode.y + dy); // Nothing is here, did we already add this to the open list?
                                    if (conflictingNode == null) // We did not, add it
                                    {
                                        addToOpenList(new Vector2(currentNode.x + dx, currentNode.y + dy), currentNode);
                                    }
                                    else if (conflictingNode.g > CALC_G(currentNode.g)) // I found a shorter route to this node.
                                    {
                                        conflictingNode.setParent(currentNode); // Give it a new parent
                                        conflictingNode.setScore((int)CALC_H(conflictingNode.x, conflictingNode.y, destination.X, destination.Y), (int)CALC_G(currentNode.g)); // set the new score.
                                    }
                                }
                            }
                        }
                    }
                }
            }


            closedList.Add(currentNode);
            return atDestination;
        }
        public float CALC_H(float CURX, float CURY, float ENDX, float ENDY)
        {
            return Math.Abs(CURX - ENDX) + Math.Abs(CURY - ENDY);
        }

        public float CALC_G(float PARENT_G)
        {
            return PARENT_G + 1;
        }

        public void addRealPosToOpenList(Vector2 position, PathNode parent)
        {
            addGridPosToOpenList(fromPositionToGrid(position), parent);
        }

        public void addGridPosToOpenList(Vector2 position, PathNode parent)
        {
            openList.Add(new PathNode(position, (int)CALC_G((parent != null) ? (parent.g) : (0)), (int)CALC_H(position.X, position.Y, destination.X, destination.Y), parent));
        }

        public void addToOpenList(Vector2 position, PathNode parent)
        {
            addGridPosToOpenList(position, parent);
        }

        public bool isGridNodeOccupied(Vector2 pos)
        {
            return isGridNodeOccupied((int)pos.X, (int)pos.Y);
        }
        public bool isGridNodeOccupied(int x, int y)
        {
            if ((x >= 0 && x < GRID_SIZE) && (y >= 0 && y < GRID_SIZE))
            {
                return map[x, y].isOccupied();
            }
            else return true;
        }
        public PathNode isNodeOpen(int x, int y)
        {
            // TODO: Optimise? This is where the application is residing in 96% of the time during pathfinding.

            // It checks if we've already added this x and y to the openlist. If we did, return it.
            foreach (var i in openList)
            {
                if (i.x == x && i.y == y)
                    return i;
            }

            return null;
        }

        public void cleanLists()
        {
            openList.Clear();
            closedList.Clear();
        }
        public void insertObstructions(Map chart, AIMesh mesh)
        {
            if (mesh != null)
            {
                // Now to draw the mesh onto the thing.
                if (mesh.isLoaded()) // if we have loaded the mesh
                    for (int x = 0; x < GRID_WIDTH; x++) // for every grid piece
                        for (int y = 0; y < GRID_HEIGHT; y++)
                        {
                            Vector2 translated = fromGridToPosition(new Vector2(x, y));
                            if (!mesh.isWalkable(translated.X, translated.Y)) // If there's nothing at this position
                                map[x, y].occupied = true; // This is obstructed
                        }
            }

            if (chart != null)
            {
                var objects = chart.GetObjects();
                foreach (var i in objects) // For every object
                {
                    if (!(i.Value is Minion) && !(i.Value is Champion))
                        continue;

                    Vector2 gridPos = fromPositionToGrid(i.Value.GetPosition()); // get the position in grid size

                    int radius = ((int)Math.Ceiling((float)i.Value.CollisionRadius / (float)PATH_DEFAULT_BOX_SIZE(mesh.getSize()))) / 2; // How many boxes does the radius of this object cover?

                    for (int dx = -radius; dx < radius; dx++) // For the whole radius in the width
                        if (gridPos.X + dx >= 0 && gridPos.X + dx < GRID_WIDTH) // As long as we're in the map (x)
                            for (int dy = -radius; dy < radius; dy++) // for the whole radius in the y
                                if (gridPos.Y + dy >= 0 && gridPos.Y + dy < GRID_HEIGHT) // As long as we're in the map (y)
                                    map[(int)gridPos.X + dx, (int)gridPos.Y + dy].occupied = true; // Occupy this piece of the map.
                }
            }

            /*  if (debugOutput())
              {
                  auto width = GRID_WIDTH;
                  auto height = GRID_HEIGHT;
  #define MIN(a,b) (((a)>(b))?(b):(a))
  #define MAX(a,b) (((a)>(b))?(a):(b))
                  std::ofstream imageFile("..\\..\\test.tga", std::ios::out | std::ios::binary);
                  if (!imageFile) return;

                  // The image header
                  unsigned char header[18] = { 0 };
                  header[2] = 1;  // truecolor
                  header[12] = width & 0xFF;
                  header[13] = (width >> 8) & 0xFF;
                  header[14] = height & 0xFF;
                  header[15] = (height >> 8) & 0xFF;
                  header[16] = 24;  // bits per pixel

                  imageFile.write((const char*)header, 18);

                  //for (int y = 0; y < height; y++)
                  for (int y = height - 1; y >= 0; y--)
                      for (int x = 0; x < width; x++)
                      {
                          // blue
                          imageFile.put(map[x][y].occupied * 128);
                          // green
                          imageFile.put(map[x][y].occupied * 128);
                          // red
                          imageFile.put(map[x][y].occupied * 128);
                      }

                  // The file footer. This part is totally optional.
                  static const char footer[26] =
                      "\0\0\0\0"  // no extension area

              "\0\0\0\0"  // no developer directory

              "TRUEVISION-XFILE"  // Yep, this is a TGA file

              ".";
                  imageFile.write(footer, 26);

                  imageFile.close();
              }*/
        }
        private static float PATH_DEFAULT_BOX_SIZE(float map_size)
        {
            return map_size / (float)GRID_SIZE;
        }
    }
    class Grid
    {
        public bool occupied;
        public bool isOccupied()
        {
            return occupied;/*return (occupant != NULL);*/
        }
        //Object * occupant;
    }
    class PathNode
    {
        public int x, y, h, g;
        public PathNode parent;
        private static int tableInitialised;
        private const int TABLE_SIZE = (2 << 15);
        private static List<PathNode> nodeTable = new List<PathNode>();

        public PathNode()
        {
            InitTable();
        }

        public PathNode(int ax, int ay, int ag, int ah, PathNode p)
        {
            Init(ax, ay, ag, ah, p);
        }

        public PathNode(Vector2 pos, int ag, int ah, PathNode p)
        {
            Init((int)pos.X, (int)pos.Y, ag, ah, p);
        }

        public void Init(int ax, int ay, int ag, int ah, PathNode p)
        {
            InitTable();
            x = ax;
            y = ay;
            h = ah;
            g = ag;
            parent = p;
        }

        public void setScore(int ah, int ag)
        {
            g = ag;
            h = ah;
        }

        public void setParent(PathNode p)
        {
            parent = p;
        }

        public static void DestroyTable()
        {
            tableInitialised = 2;
            nodeTable.Clear();
            tableInitialised = -1;
        }
        public static bool isTableEmpty()
        {
            InitTable();
            return nodeTable.Count == 0;
        }
        public static int missingNodes()
        {
            InitTable();
            return TABLE_SIZE - nodeTable.Capacity;
        }

        private static void InitTable()
        {
            if (tableInitialised != -1) return; // We have already initialised it or we're busy doing it.
            tableInitialised = 0;
            nodeTable = new List<PathNode>(TABLE_SIZE);
            for (int i = 0; i < TABLE_SIZE; i++)
                nodeTable.Add(new PathNode());

            tableInitialised = 1;
        }

    }
    public enum PathError
    {
        PATH_ERROR_NONE = 0,
        PATH_ERROR_OUT_OF_TRIES = 1,
        PATH_ERROR_OPENLIST_EMPTY = 2
    };
}
