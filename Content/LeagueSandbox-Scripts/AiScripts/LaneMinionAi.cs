using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Scripting.CSharp;
using System.Linq;
using System.Collections.Generic;

namespace AIScripts
{
    public class LaneMinionAI : IAIScript
    {
        public IAIScriptMetaData AIScriptMetaData { get; set; } = new AIScriptMetaData();
        ILaneMinion LaneMinion;
        int _curMainWaypoint = 0;
        internal const float DETECT_RANGE = 475.0f;
        float minionActionTimer = 2500.0f;
        public void OnActivate(IObjAiBase owner)
        {
            LaneMinion = owner as ILaneMinion;
        }
        public void OnUpdate(float diff)
        {
            if (LaneMinion != null && !LaneMinion.IsDead)
            {
                minionActionTimer -= diff;
                if (minionActionTimer >= 0)
                {
                    if (LaneMinion.MovementParameters != null || LaneMinion.IsAiPaused())
                    {
                        return;
                    }

                    AIMove();
                    minionActionTimer = 2500.0f;
                }
            }
        }

        protected bool ScanForTargets()
        {
            if (LaneMinion.TargetUnit != null && !LaneMinion.TargetUnit.IsDead)
            {
                return true;
            }

            IAttackableUnit nextTarget = null;
            var nextTargetPriority = 14;
            var nearestObjects = GetUnitsInRange(LaneMinion.Position, LaneMinion.Stats.Range.Total, true);
            //Find target closest to max attack range.
            foreach (var it in nearestObjects.OrderBy(x => Vector2.DistanceSquared(LaneMinion.Position, x.Position) - (LaneMinion.Stats.Range.Total * LaneMinion.Stats.Range.Total)))
            {
                if (!(it is IAttackableUnit u)
                    || u.IsDead
                    || u.Team == LaneMinion.Team
                    || Vector2.DistanceSquared(LaneMinion.Position, u.Position) > DETECT_RANGE * DETECT_RANGE
                    || !TeamHasVision(LaneMinion.Team, u)
                    || !u.Status.HasFlag(StatusFlags.Targetable)
                    || UnitIsProtected(u))
                {
                    continue;
                }

                var priority = (int)LaneMinion.ClassifyTarget(u);// get the priority.
                if (priority < nextTargetPriority) // if the priority is lower than the target we checked previously
                {
                    nextTarget = u;                // make it a potential target.
                    nextTargetPriority = priority;
                }
            }

            if (nextTarget != null) // If we have a target
            {
                // Set the new target and refresh waypoints
                LaneMinion.SetTargetUnit(nextTarget, true);

                return true;
            }

            return false;
        }
        public bool AIMove()
        {
            // TODO: Use unique LaneMinion AI instead of normal Minion AI and add here for return values.
            if (ScanForTargets()) // returns true if we have a target
            {
                if (!LaneMinion.RecalculateAttackPosition())
                {
                    KeepFocusingTarget(); // attack/follow target
                }
                return false;
            }

            // If we have lane path instructions from the map
            if (LaneMinion.PathingWaypoints.Count > 0 && LaneMinion.TargetUnit == null)
            {
                WalkToDestination();
            }
            return true;
        }
        protected void KeepFocusingTarget()
        {
            if (LaneMinion.IsAttacking && (LaneMinion.TargetUnit == null
                || LaneMinion.TargetUnit.IsDead
                || Vector2.DistanceSquared(LaneMinion.Position, LaneMinion.TargetUnit.Position) > LaneMinion.Stats.Range.Total * LaneMinion.Stats.Range.Total))
            // If target is dead or out of range
            {
                LaneMinion.CancelAutoAttack(false, true);
            }
        }
        public void WalkToDestination()
        {
            // TODO: Use the methods used in this function for any other minion pathfinding (attacking, targeting, etc).

            var mainWaypoint = LaneMinion.PathingWaypoints[_curMainWaypoint];
            var lastWaypoint = Vector2.Zero;

            if (LaneMinion.Waypoints.Count > 0)
            {
                lastWaypoint = LaneMinion.Waypoints[LaneMinion.Waypoints.Count - 1];
            }

            // First, we make sure we are pathfinding to our current main waypoint.
            if (lastWaypoint != mainWaypoint)
            {
                // Pathfind to the next waypoint.
                var path = new List<Vector2>() { LaneMinion.Position, LaneMinion.PathingWaypoints[_curMainWaypoint] };
                var tempPath = GetPath(LaneMinion.Position, LaneMinion.PathingWaypoints[_curMainWaypoint]);
                if (tempPath != null)
                {
                    path = tempPath;
                }

                LaneMinion.SetWaypoints(path);
                LaneMinion.UpdateMoveOrder(OrderType.MoveTo);

                //TODO: Here we need a certain way to tell if the Minion is in the path/lane, else use pathfinding to return to the lane.
                //I think in league when minion start chasing they save Current Position and
                //when it stop chasing the minion return to the last saved position, and then continue main waypoints from there.

                /*var path = _game.Map.NavGrid.GetPath(GetPosition(), _mainWaypoints[_curMainWaypoint + 1]);
                if(path.Count > 1)
                {
                    SetWaypoints(path);
                }*/
            }

            var waypointSuccessRange = LaneMinion.CollisionRadius;
            var nearestObjects = GetUnitsInRange(LaneMinion.Position, LaneMinion.Stats.Range.Total, true);

            // This is equivalent to making any colliding minions equal to a single minion to save on pathfinding resources.
            foreach (IAttackableUnit obj in nearestObjects)
            {
                if (obj is ILaneMinion minion)
                {
                    // If the closest minion is in collision range, add its collision radius to the waypoint success range.
                    if (GameServerCore.Extensions.IsVectorWithinRange(minion.Position, LaneMinion.Position, waypointSuccessRange))
                    {
                        waypointSuccessRange += minion.CollisionRadius;
                    }
                    // If the closest minion (above) is not in collision range, then we stop the loop.
                    else
                    {
                        continue;
                    }
                }
            }

            // Since we are pathfinding to our current main waypoint, we check if 
            if (GameServerCore.Extensions.IsVectorWithinRange(LaneMinion.Position, LaneMinion.PathingWaypoints[_curMainWaypoint], waypointSuccessRange) && LaneMinion.PathingWaypoints.Count > _curMainWaypoint + 1)
            {
                ++_curMainWaypoint;

                // Pathfind to the next waypoint.
                var path = new List<Vector2>() { LaneMinion.Position, LaneMinion.PathingWaypoints[_curMainWaypoint] };
                var tempPath = GetPath(LaneMinion.Position, LaneMinion.PathingWaypoints[_curMainWaypoint]);
                if (tempPath != null)
                {
                    path = tempPath;
                }

                LaneMinion.SetWaypoints(path);
                LaneMinion.UpdateMoveOrder(OrderType.AttackMove);
            }
        }

        // TODO: Override KeepFocusingTarget and use unique LaneMinion AI
    }
}