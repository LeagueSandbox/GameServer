using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.Maps;
using LeagueSandbox.GameServer.Logic.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public enum MinionSpawnPosition : uint
    {
        SPAWN_BLUE_TOP = 0xeb364c40,
        SPAWN_BLUE_BOT = 0x53b83640,
        SPAWN_BLUE_MID = 0xb7717140,
        SPAWN_RED_TOP = 0xe647d540,
        SPAWN_RED_BOT = 0x5ec9af40,
        SPAWN_RED_MID = 0xba00e840
    };

    public enum MinionSpawnType : byte
    {
        MINION_TYPE_MELEE = 0x00,
        MINION_TYPE_CASTER = 0x03,
        MINION_TYPE_CANNON = 0x02,
        MINION_TYPE_SUPER = 0x01
    };
    public class Minion : Unit
    {
        /**
 * Const waypoints that define the minion's route
 */
        protected List<Vector2> mainWaypoints;
        protected int curMainWaypoint = 0;
        protected MinionSpawnPosition spawnPosition;
        protected MinionSpawnType minionType;

        public Minion(Map map, int id, MinionSpawnType type, MinionSpawnPosition position, List<Vector2> mainWaypoints) : base(map, id, "", new MinionStats(), 40, 0, 0, 1100)
        {
            this.minionType = type;
            this.spawnPosition = position;
            this.mainWaypoints = mainWaypoints;
            this.curMainWaypoint = 0;

            var spawnSpecifics = map.getMinionSpawnPosition(spawnPosition);
            setTeam(CustomConvert.toTeamId(spawnSpecifics.Item1));
            setPosition(spawnSpecifics.Item2.X, spawnSpecifics.Item2.Y);

            map.setMinionStats(this); // Let the map decide how strong this minion has to be.

            string minionModel = "";
            if (spawnSpecifics.Item1 == 0) // If we're the blue side
                minionModel += "Blue_Minion_"; // make it a blue minion
            else
                minionModel += "Red_Minion_"; // otherwise make it a red minion

            // Finish model name with type
            if (type == MinionSpawnType.MINION_TYPE_MELEE)
                minionModel += "Basic";
            else if (type == MinionSpawnType.MINION_TYPE_CASTER)
                minionModel += "Wizard";
            else
                minionModel += "MechCannon";

            // Set model
            setModel(minionModel);
            


            if (mainWaypoints.Count > 0)                                                      // If we have lane path instructions from the map
                setWaypoints(new List<Vector2> { mainWaypoints[0], mainWaypoints[1] });       // Follow these instructions
            else
                setWaypoints(new List<Vector2> { new Vector2(x, y), new Vector2(x, y) });     // Otherwise path to own position. (Stand still)

            setMoveOrder(MoveOrder.MOVE_ORDER_ATTACKMOVE);
        }

        public Minion(Map map, int id, MinionSpawnType type, MinionSpawnPosition position) : this(map, id, type, position, new List<Vector2>())
        {

        }
        public MinionSpawnPosition getSpawnPosition()
        {
            return spawnPosition;
        }

        public MinionSpawnType getType()
        {
            return minionType;
        }

        public override void update(long diff)
        {
            base.update(diff);

            if (!isDead())
            {
                if (dashing)
                    return;
                else if (scanForTargets())     // returns true if we have a target
                    keepFocussingTarget(); // fight target
                else
                    walkToDestination(); // walk to destination (or target)
            }
        }

        public override void onCollision(GameObject a_Collider)
        {
            if (a_Collider == targetUnit) // If we're colliding with the target, don't do anything.
                return;

            if (targetUnit != null)
            {
                // Thread this?
                //Path newPath = Pathfinder::getPath(getPosition(), targetUnit->getPosition());
                //if (newPath.error == PATH_ERROR_NONE)
                //setWaypoints(newPath.getWaypoints());
            }
        }

        public override bool isInDistress()
        {
            return distressCause != null;
        }

        // AI tasks
        protected bool scanForTargets()
        {
            Unit nextTarget = null;
            double nextTargetPriority = 9e5;

            var objects = map.getObjects();
            foreach (var it in objects)
            {
                var u = it.Value as Unit;

                // Targets have to be:
                if (u == null ||                                    // a unit
                     u.isDead() ||                                  // alive
                     u.getTeam() == getTeam() ||                    // not on our team
                     distanceWith(u) > DETECT_RANGE ||              // in range
                     !getMap().teamHasVisionOn(getTeam(), u))       // visible to this minion
                    continue;                                       // If not, look for something else

                var priority = classifyTarget(u);  // get the priority.
                if (priority < nextTargetPriority) // if the priority is lower than the target we checked previously
                {
                    nextTarget = u;                // make him a potential target.
                    nextTargetPriority = priority;
                }
            }

            if (nextTarget != null) // If we have a target
            {
                setTargetUnit(nextTarget); // Set the new target and refresh waypoints
                PacketNotifier.notifySetTarget(this, nextTarget);
                return true;
            }
            return false;
        }
        protected void walkToDestination()
        {
            if (mainWaypoints.Count > curMainWaypoint + 1)
            {
                if ((waypoints.Count == 1) || (curWaypoint == 2 && ++curMainWaypoint < mainWaypoints.Count))
                {
                    //CORE_INFO("Minion reached a point! Going to %f; %f", mainWaypoints[curMainWaypoint].X, mainWaypoints[curMainWaypoint].Y);
                    List<Vector2> newWaypoints = new List<Vector2> { new Vector2(x, y), mainWaypoints[curMainWaypoint] };
                    setWaypoints(newWaypoints);
                }
            }
        }
        protected void keepFocussingTarget()
        {
            if (isAttacking && (targetUnit == null || distanceWith(targetUnit) > stats.getRange()))
            // If target is dead or out of range
            {
                PacketNotifier.notifyStopAutoAttack(this);
                isAttacking = false;
            }
        }
    }
}
