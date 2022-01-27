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
        int currentWaypointIndex = 0;
        internal const float DETECT_RANGE = 475.0f;
        float minionActionTimer = 250f;
        bool targetIsStillValid = false;
        public void OnActivate(IObjAiBase owner)
        {
            LaneMinion = owner as ILaneMinion;
        }
        
        public void OnUpdate(float delta)
        {
            if(LaneMinion != null && !LaneMinion.IsDead)
            {

                targetIsStillValid = checkIfTargetIsValid();

                minionActionTimer += delta;
                if(
                    minionActionTimer >= 250.0f
                    //Quote: There’s also a number of things that can occur between the 0.25 second interval of the normal sweep through the AI Priority List:

                    //Quote: Their current attack target dies. Minions who witness the death of their foe will check for a new valid target in their acquisition range.
                    || !targetIsStillValid
                ) {
                    OrderType nextBehaviour = ReevaluateBehavior();
                    LaneMinion.UpdateMoveOrder(nextBehaviour);
                    minionActionTimer = 0;
                }

                if(LaneMinion.MoveOrder == OrderType.AttackTo)
                    LaneMinion.RecalculateAttackPosition();
            }
        }

        bool UnitInRange(IAttackableUnit u, float range)
        {
            return Vector2.DistanceSquared(LaneMinion.Position, u.Position) < (range * range);
        }

        // Probably should be merged with isValidTarget
        bool checkIfTargetIsValid()
        {
            IAttackableUnit u = LaneMinion.TargetUnit;
            return (
                u != null
                && !u.IsDead
                && UnitInRange(u, DETECT_RANGE /*LaneMinion.Stats.Range.Total*/)
            );
        }

        // Probably should be merged with checkIfTargetIsValid
        bool isValidTarget(IAttackableUnit u)
        {
            return (
                u != null
                && !u.IsDead
                && u.Team != LaneMinion.Team
                && UnitInRange(u, DETECT_RANGE)
                && TeamHasVision(LaneMinion.Team, u)
                && u.Status.HasFlag(StatusFlags.Targetable)
                && !UnitIsProtected(u)
            );
        }

        bool FoundNewTarget()
        {
            IAttackableUnit nextTarget = null;
            int nextTargetPriority = (int)ClassifyUnit.DEFAULT;
            float nextTargetDistanceSquared = DETECT_RANGE * DETECT_RANGE;

            var nearestObjects = GetUnitsInRange(LaneMinion.Position, LaneMinion.Stats.Range.Total, true);
            foreach (var it in nearestObjects)
            {
                if (it is IAttackableUnit u && isValidTarget(u))
                {
                    int priority = (int)LaneMinion.ClassifyTarget(u);
                    var distanceSquared = Vector2.DistanceSquared(LaneMinion.Position, u.Position);
                    if (
                        nextTarget == null
                        || priority < nextTargetPriority
                        || (
                            priority == nextTargetPriority
                            && distanceSquared < nextTargetDistanceSquared
                        )
                    ) {
                        nextTarget = u;
                        nextTargetPriority = priority;
                        nextTargetDistanceSquared = distanceSquared;
                    }
                }
            }
            
            if(nextTarget != null)
            {
                LaneMinion.SetTargetUnit(nextTarget, true);
                /*
                LogDebug("{0} FROM TEAM {1} TARGETS {2} FROM {3} AT DISTANCE {4} WITH PRIO {5}",
                    LaneMinion.Model, LaneMinion.Team,
                    nextTarget.Model, nextTarget.Team,
                    nextTargetDistanceSquared,
                    nextTargetPriority
                );
                */
                return true;
            }
            return false;
        }

        bool WaypointReached()
        {
            Vector2 currentWaypoint = LaneMinion.PathingWaypoints[currentWaypointIndex];

            var waypointSuccessRange = LaneMinion.CollisionRadius;
            var nearestObjects = /*(List<ILaneMinion>)*/ GetUnitsInRange(LaneMinion.Position, LaneMinion.Stats.Range.Total, true);
                                 //.FindAll(obj => obj is ILaneMinion minion)
                                 //.OrderBy(minion => Vector2.DistanceSquared(minion.Position, currentWaypoint));

            //TODO: Fix the code stupidly copied from the previous version of the script so that it actually does what is written in the comments.
            
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
                }
            }

            if (GameServerCore.Extensions.IsVectorWithinRange(LaneMinion.Position, LaneMinion.PathingWaypoints[currentWaypointIndex], waypointSuccessRange))
                return true;

            return false;
        }

        OrderType ReevaluateBehavior()
        {
            //Quote: Follow any current specialized behavior rules, such as from CC (Taunts, Flees, Fears)
            
            //Quote: Continue attacking (or moving towards) their current target if that target is still valid.
            if(targetIsStillValid)
            {
                //LaneMinion.RecalculateAttackPosition();
                return OrderType.AttackTo;
            }
            else if(LaneMinion.IsAttacking)
            {
                LaneMinion.CancelAutoAttack(false, true);
            }

            //Quote: If they have failed to attack their target for 4 seconds, they temporarily ignore them instead.
            
            //Quote: Find a new valid target in the minion’s acquisition range to attack.
            //Quote: If multiple valid targets, prioritize based on “how hard is it for me to path there?”
            if(FoundNewTarget())
            {
                //LaneMinion.RecalculateAttackPosition();
                return OrderType.AttackTo;
            }
            
            //Quote: Check if near a target waypoint, if so change the target waypoint to the next in the line.
            
            bool notYetOutOfRange = true;
            while(
                (notYetOutOfRange = currentWaypointIndex < LaneMinion.PathingWaypoints.Count)
                && WaypointReached()
            ) {
                currentWaypointIndex++;
            }

            //Quote: Walk towards the target waypoint.
            if(notYetOutOfRange)
            {
                Vector2 currentWaypoint = LaneMinion.PathingWaypoints[currentWaypointIndex];
                Vector2 currentDestination = LaneMinion.Waypoints[LaneMinion.Waypoints.Count - 1];

                bool waypointChanged = (
                    LaneMinion.Waypoints == null
                    || LaneMinion.Waypoints.Count < 2
                    || currentDestination != currentWaypoint
                );
                
                if(waypointChanged)
                {
                    List<Vector2> path = null;
                    
                    // If the minion returns to lane
                    // Instead of continuing to move along the waypoints
                    //if(LaneMinion.MoveOrder != OrderType.AttackMove)
                    path = GetPath(LaneMinion.Position, currentWaypoint);

                    if(path == null)
                    {
                        path = new List<Vector2>()
                        {
                            LaneMinion.Position,
                            currentWaypoint
                        };
                    }
                    LaneMinion.SetWaypoints(path);
                }
                
                return OrderType.AttackMove;
            }

            return OrderType.Stop;
        }
    }
}