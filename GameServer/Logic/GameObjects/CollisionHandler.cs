using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Maps;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class CollisionHandler
    {
        private Logger _logger = Program.ResolveDependency<Logger>();
        private Game _game = Program.ResolveDependency<Game>();

        private float width, height;
        private CollisionDivision[] managedDivisions = new CollisionDivision[3 * 3];
        private CollisionDivision unmanagedDivision = new CollisionDivision();
        private int divisionCount;
        private Map chart;
        private bool simple = false;

        public CollisionHandler(Map map)
        {
            chart = map;
            Pathfinder.setMap(map);
            // Initialise the pathfinder.

            divisionCount = -1;
            // We have no divisions yet. Waiting for the AIMesh to initialise.

            if (simple)
                _logger.LogCoreWarning(
                    "Using simple collision. This could impact performance with larger amounts of minions."
                );
        }
        public void init(int divisionsOverWidth)
        {
            for (int i = 0; i < managedDivisions.Length; i++)
                managedDivisions[i] = new CollisionDivision();

            width = chart.AIMesh.getWidth();
            height = chart.AIMesh.getHeight();

            // Get the square root of the division count.
            // This is why it requires to be squared. (It's a map of 3x3 by default)
            //CORE_INFO("CollisionHandler has %d divisions.", divisionsOverWidth*divisionsOverWidth);
            divisionCount = divisionsOverWidth;

            // Setup the division dimensions
            for (int y = 0; y < divisionsOverWidth; y++)
            {
                for (int x = 0; x < divisionsOverWidth; x++)
                {
                    managedDivisions[y * divisionsOverWidth + x].min.X = x * (width / divisionsOverWidth);
                    managedDivisions[y * divisionsOverWidth + x].min.Y = y * (height / divisionsOverWidth);

                    managedDivisions[y * divisionsOverWidth + x].max.X = (x + 1) * (width / divisionsOverWidth);
                    managedDivisions[y * divisionsOverWidth + x].max.Y = (y + 1) * (height / divisionsOverWidth);

                    managedDivisions[y * divisionsOverWidth + x].objectCount = 0;
                }
            }
        }

        public void addObject(GameObject obj)
        {
            if (divisionCount == -1) // If we have not initialised..
            {
                _logger.LogCoreError("Tried to add an object before we initialised the CollisionHandler!");
                return;
            }

            var map = _game.Map;
            if (map != null && map != chart)
            {
                _logger.LogCoreInfo(string.Format(
                    "Map is adding an object that is not healthy. His map pointer is {0} (not {1}). Not adding it.",
                    _game.Map,
                    chart
                ));
                return;
            }

            float divX = obj.X / (float)(width / divisionCount); // Get the division position.
            float divY = obj.Y / (float)(height / divisionCount);

            int divi = (int)divY * divisionCount + (int)divX;

            // We're not inside the map! Add to the unmanaged objects.
            if (divX < 0 || divX > divisionCount || divY < 0 || divY > divisionCount)
            {
                _logger.LogCoreError(string.Format(
                    "Object spawned outside of map. ({0}, {1})",
                    obj.X,
                    obj.Y
                ));
                //addUnmanagedObject(object);
            }
            else
            {
                addToDivision(obj, (int)divX, (int)divY);
                CollisionDivision curDiv = managedDivisions[divi];

                bool a = false, b = false;
                // Are we in the one to the right?
                if (Math.Abs(obj.X - curDiv.max.X) < obj.CollisionRadius)
                    addToDivision(obj, (int)divX + 1, (int)divY); // Add it there too.

                // Maybe on the left?
                if (Math.Abs(obj.X - curDiv.min.X) < obj.CollisionRadius)
                {
                    a = true;
                    addToDivision(obj, (int)divX - 1, (int)divY);
                }

                // Are we touching below us?
                if (Math.Abs(obj.Y - curDiv.max.Y) < obj.CollisionRadius)
                    addToDivision(obj, (int)divX, (int)divY + 1);

                // Or above?
                if (Math.Abs(obj.Y - curDiv.min.Y) < obj.CollisionRadius)
                {
                    b = true;
                    addToDivision(obj, (int)divX, (int)divY - 1);
                }

                // If we are touching all four, add the left-upper one.
                if (a && b)
                {
                    b = true;
                    addToDivision(obj, (int)divX - 1, (int)divY - 1);
                }

            }
        }
        public void removeObject(GameObject obj)
        {
            CollisionDivision curDiv;
            for (int i = -1; i < divisionCount * divisionCount; i++)
            {
                if (i == -1) curDiv = unmanagedDivision;
                else curDiv = managedDivisions[i];

                var j = curDiv.find(obj);
                while (j != -1)
                {
                    curDiv.remove(j);
                    j = curDiv.find(obj);
                }
            }
        }
        public void getDivisions(GameObject obj, CollisionDivision[] divs, out int divCount)
        {
            for (int i = 0; i < 4; i++) // Prepare the divisions
            {
                divs[i] = null;
            }

            divCount = 0; // How many divisions the object is in.
            float divX = obj.X / (float)(width / divisionCount);
            float divY = obj.Y / (float)(height / divisionCount);

            int divi = (int)divY * divisionCount + (int)divX; // Current division index

            if (divY >= 0 && divY < divisionCount) // If we're inside the map
            {
                divs[divCount] = managedDivisions[divi];
                divCount++;
            }

            // Below is same principle from addObject.
            bool a = false, b = false;
            var curDiv = managedDivisions[divi];
            if (Math.Abs(obj.X - curDiv.max.X) < obj.CollisionRadius
                && divX + 1 >= 0 && divX + 1 < divisionCount
            )
            {
                divs[divCount] = managedDivisions[(int)divY * divisionCount + (int)divX + 1];
                divCount++;
            }
            else if (Math.Abs(obj.X - curDiv.min.X) < obj.CollisionRadius
                && divX - 1 >= 0 && divX - 1 < divisionCount
            )
            {
                divs[divCount] = managedDivisions[(int)divY * divisionCount + (int)divX - 1];
                divCount++;
                a = true;
            }
            if (Math.Abs(obj.Y - curDiv.max.Y) < obj.CollisionRadius
                && divY + 1 >= 0 && divY + 1 < divisionCount
            )
            {
                divs[divCount] = managedDivisions[(int)divY * divisionCount + (int)divX + 1];
                divCount++;
            }
            else if (Math.Abs(obj.Y - curDiv.min.Y) < obj.CollisionRadius
                && divY - 1 >= 0 && divY - 1 < divisionCount
            )
            {
                divs[divCount] = managedDivisions[(int)divY * divisionCount + (int)divX + 1];
                divCount++;
                b = true;
            }

            if (a && b && divX + 1 >= 0 && divX + 1 < divisionCount)
            {
                divs[divCount] = managedDivisions[(int)divY * divisionCount + (int)divX + 1];
                divCount++;
            }
        }

        public void update(float a_DT)
        {
            if (!simple) // Faster
            {
                // Correct the unmanaged division (minions outside the map)
                //correctUnmanagedDivision();

                // For every managed division
                for (int i = 0; i < divisionCount * divisionCount; i++)
                {
                    // Correct the divisions it should be in
                    correctDivisions(i);

                    // Check for collisions inside this division
                    checkForCollisions(i);
                }
            }
            else // Slower, but works
            {
                foreach (var objectRef in chart.GetObjects())
                {
                    var o1 = objectRef.Value;
                    if (!(o1 is Minion) && !(o1 is Champion))
                        continue;

                    foreach (var objectRef2 in chart.GetObjects())
                    {
                        if (objectRef.Key != objectRef2.Key)
                        {
                            var o2 = objectRef2.Value;
                            if (!(o2 is Minion) && !(o2 is Champion))
                                continue;

                            var displ = (o2.GetPosition() - o1.GetPosition());
                            if (
                                displ.SqrLength() <
                                (o1.CollisionRadius + o2.CollisionRadius) *
                                (o1.CollisionRadius + o2.CollisionRadius)
                            )
                            {
                                o1.onCollision(o2);
                                //o2->onCollision(o1); // Is being done by the second iteration.
                            }
                        }
                    }
                }
            }
        }


        private void checkForCollisions(int pos)
        {
            var curDiv = managedDivisions[pos];

            for (int i = 0; i < curDiv.objects.Count; i++) // for each object in the current division
            {
                var o1 = curDiv.objects[i];

                for (int j = 0; j < curDiv.objects.Count; j++) if (j != i)
                    {
                        var o2 = curDiv.objects[j];

                        var displ = (o2.GetPosition() - o1.GetPosition());
                        if (displ.SqrLength() <
                            (o1.CollisionRadius + o2.CollisionRadius) *
                            (o1.CollisionRadius + o2.CollisionRadius))
                        {
                            o1.onCollision(o2);
                            //o2->onCollision(o1); // Is being done by the second iteration.
                        }
                    }
            }
        }

        private void correctDivisions(int pos)
        {
            CollisionDivision curDiv = managedDivisions[pos];
            for (int j = 0; j < curDiv.objects.Count; j++) // For all objects inside this division
            {
                var o = curDiv.objects[j];

                if (o != null)
                //if (o->isMovementUpdated())  // Only check if they moved around.
                {
                    while (_game.Map.Id != chart.Id)
                    {
                        _logger.LogCoreWarning(string.Format(
                            "I have found an object that is not healthy. " +
                            "His map pointer is {0} (not {1}). " +
                            "Removing it from the database ({2}/{3} in div {4}).",
                            _game.Map.Id,
                            chart.Id,
                            j,
                            curDiv.objects.Count,
                            pos
                        ));
                        removeObject(o);
                        if (j < curDiv.objects.Count)
                            o = curDiv.objects[j];
                        else break;
                    }

                    // If they are no longer in this division..
                    if ((o.X - o.CollisionRadius > curDiv.max.X
                        || o.Y - o.CollisionRadius > curDiv.max.Y
                        || o.X + o.CollisionRadius < curDiv.min.X
                        || o.Y + o.CollisionRadius < curDiv.min.Y))
                    {
                        removeFromDivision(o, pos); // Remove them from it.
                        addObject(o); // Reset in what divisions this object is.
                    }

                    // If they've entered another division, but not left this one yet..
                    else if ((o.X + o.CollisionRadius > curDiv.max.X
                        || o.Y + o.CollisionRadius > curDiv.max.Y
                        || o.X - o.CollisionRadius < curDiv.min.X
                        || o.Y - o.CollisionRadius < curDiv.min.Y))
                    {
                        addObject(o); // Reset in what divisions this object is.
                    }
                }
            }
        }
        private void correctUnmanagedDivision()
        {
            var curDiv = unmanagedDivision;
            for (int j = 0; j < curDiv.objects.Count; j++) // For everything outside the map
            {
                var o = (curDiv.objects[j]);
                var center = curDiv.min + ((curDiv.max - curDiv.min) * 0.5f);

                //if (o->isMovementUpdated()) // if they moved
                {
                    // If they're inside the map.
                    if ((o.X - o.CollisionRadius > width
                        || o.Y - o.CollisionRadius > height
                        || o.X + o.CollisionRadius < 0
                        || o.Y + o.CollisionRadius < 0))
                    {
                        removeFromDivision(o, -1);
                        //CORE_INFO("Minion moving from unmanaged!");
                        addObject(o);
                    }
                }
            }
        }

        private void addToDivision(GameObject obj, int x, int y)
        {
            if (y >= 0 && y < divisionCount && x >= 0 && x < divisionCount)
            {
                int pos = y * divisionCount + x; // get the division position
                if (managedDivisions[pos].find(obj) == -1) // Are we not in this division?
                {
                    managedDivisions[pos].push(obj); // Add it
                    //CORE_INFO("MINION %d ADDED TO DIVISION %d", managedDivisions[pos].objects.size() - 1, pos);
                }
            }
        }

        private void addUnmanagedObject(GameObject obj)
        {
            //if(y < 0 || y >= divisionCount || x < 0 || x >= divisionCount))
            {
                if (unmanagedDivision.find(obj) == -1)
                {
                    unmanagedDivision.push(obj);
                    //CORE_INFO("MINION %d ADDED TO UNMANAGED DIVISION", unmanagedDivision.objects.size() - 1);
                }
            }
        }

        private void removeFromDivision(GameObject obj, int i)
        {
            CollisionDivision curDiv;
            if (i == -1)
                curDiv = unmanagedDivision;
            else
                curDiv = managedDivisions[i];

            curDiv.remove(curDiv.find(obj));
            //	auto j = curDiv->find(object);
            //   while (j != -1)
            //	{
            //		curDiv->remove(j);
            //
            //		j = curDiv->find(object);
            //   }
        }

    }
    public class CollisionDivision
    {
        public Vector2 min, max;
        public List<GameObject> objects = new List<GameObject>();
        public int objectCount = 0;

        public void push(GameObject a)
        {
            objects.Add(a);
        }
        public void remove(int i)
        {
            if (i >= 0 && i < objects.Count)
            {
                objects.RemoveAt(i);
            }
        }
        public int find(GameObject a)
        {
            for (var i = 0; i < objects.Count; i++)
                if (a == objects[i])
                    return i;

            return -1;
        }
        public void clear()
        {
            for (var i = 0; i < objects.Count; i++)
                objects[i] = null;
        }
    };
}
