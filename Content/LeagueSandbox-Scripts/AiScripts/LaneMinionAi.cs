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
        float minionActionTimer = 250f;
        bool targetIsStillValid = false;
        Dictionary<uint, float> temporaryIgnored = new Dictionary<uint, float>();
        float timeSinceLastAttack = 0f;
        int targetUnitPriority = (int) ClassifyUnit.DEFAULT;
        float localTime = 0f;
        bool callsForHelpMayBeCleared = false;
        public void OnActivate(IObjAiBase owner)
        {
            LaneMinion = owner as ILaneMinion;
            LaneMinion.HandlesCallsForHelp = true;
        }
        
        public void OnUpdate(float delta)
        {
            localTime += delta;

            if(LaneMinion != null && !LaneMinion.IsDead)
            {

                if(LaneMinion.IsAttacking || LaneMinion.TargetUnit == null)
                {
                    //if(LaneMinion.TargetUnit != null && LaneMinion.TargetUnit is IChampion)
                    //    LogDebug("TimeScinceLastAttack = {0}", timeSinceLastAttack);
                    timeSinceLastAttack = 0f;
                }
                else
                {
                    timeSinceLastAttack += delta;
                }

                /*
                foreach(var unit in LaneMinion.unitsAttackingAllies)
                {
                    LogDebug(
                        "#{0}({1}) received call for help against #{2}({3})",
                        LaneMinion.NetId, LaneMinion.Model,
                        unit.NetId, unit.Model
                    );
                }
                */

                minionActionTimer += delta;
                if(
                    //Quote: There’s also a number of things
                    //       that can occur between the 0.25 second interval
                    //       of the normal sweep through the AI Priority List:
                    //Quote: Taunt/Fear/Flee/Movement Disable/Attack Disable.
                    //       All of these cause a minion to freshly reevaluate its behavior immediately.
                    LaneMinion.MovementParameters == null
                    //Quote: Collisions.
                    //       Minions that end up overlapping other minions will reevaluate their behavior immediately.
                    && !LaneMinion.RecalculateAttackPosition()
                    && (
                        //Quote: Their current attack target dies.
                        //       Minions who witness the death of their foe will check for a new valid target in their acquisition range.
                        !(targetIsStillValid = IsValidTarget(LaneMinion.TargetUnit))
                        || (
                            !(
                                //Quote: Call for Help.
                                FoundNewTarget(true)
                                // Found a new valid target, no need to check for autoattack timer
                            )
                            && minionActionTimer >= 250.0f
                        )
                    )
                ) {
                    OrderType nextBehaviour = ReevaluateBehavior(delta);
                    LaneMinion.UpdateMoveOrder(nextBehaviour);
                    minionActionTimer = 0;
                }

                if(callsForHelpMayBeCleared)
                {
                    callsForHelpMayBeCleared = false;
                    LaneMinion.ClearCallsForHelp();
                }
            }
        }

        bool UnitInRange(IAttackableUnit u, float range)
        {
            return Vector2.DistanceSquared(LaneMinion.Position, u.Position) < (range * range);
        }

        bool IsValidTarget(IAttackableUnit u)
        {
            return (
                u != null
                && !u.IsDead
                && u.Team != LaneMinion.Team
                && UnitInRange(u, LaneMinion.AcquisitionRange)
                && TeamHasVision(LaneMinion.Team, u)
                && u.Status.HasFlag(StatusFlags.Targetable)
                && !UnitIsProtected(u)
            );
        }

        void Ignore(IAttackableUnit unit, float time = 500)
        {
            //LogDebug("Temporary ignoring #{0}({1})", LaneMinion.TargetUnit.NetId, LaneMinion.TargetUnit.Model);
            temporaryIgnored[unit.NetId] = localTime + time;
        }

        void FilterTemporaryIgnoredList()
        {
            List<uint> keysToRemove = new List<uint>();
            foreach (var pair in temporaryIgnored)
            {
                if(pair.Value <= localTime)
                    keysToRemove.Add(pair.Key);
            }
            foreach (var key in keysToRemove)
            {
                temporaryIgnored.Remove(key);
            }
        }

        bool FoundNewTarget(bool handleOnlyCallsForHelp = false)
        {
            callsForHelpMayBeCleared = true;

            IAttackableUnit currentTarget = null;
            IAttackableUnit nextTarget = currentTarget;
            int nextTargetPriority = (int)ClassifyUnit.DEFAULT;
            float nextTargetDistanceSquared = LaneMinion.AcquisitionRange * LaneMinion.AcquisitionRange;
            int nextTargetAttackers = 0;
            if(targetIsStillValid)
            {
                currentTarget = LaneMinion.TargetUnit;
                nextTarget = currentTarget;
                nextTargetPriority = targetUnitPriority;
                nextTargetDistanceSquared = Vector2.DistanceSquared(LaneMinion.Position, nextTarget.Position);
                nextTargetAttackers = LaneMinion.IsMelee ? CountUnitsAttackingUnit(nextTarget) : 0; // First Wave Behaviour
            }
            
            FilterTemporaryIgnoredList();

            List<IAttackableUnit> nearestObjects;
            if(handleOnlyCallsForHelp)
            {
                if(LaneMinion.unitsAttackingAllies.Count == 0)
                {
                    return false;
                }
                nearestObjects = LaneMinion.unitsAttackingAllies.Keys.ToList();
            }
            else
            {
                nearestObjects = GetUnitsInRange(LaneMinion.Position, LaneMinion.AcquisitionRange, true);
            }
            foreach (var it in nearestObjects)
            {
                if (it is IAttackableUnit u && IsValidTarget(u) && !temporaryIgnored.ContainsKey(u.NetId))
                {
                    int priority = LaneMinion.unitsAttackingAllies.ContainsKey(u) ?
                        LaneMinion.unitsAttackingAllies[u]
                        : (int)LaneMinion.ClassifyTarget(u)
                    ;
                    float distanceSquared = Vector2.DistanceSquared(LaneMinion.Position, u.Position);
                    int attackers = LaneMinion.IsMelee ? CountUnitsAttackingUnit(u) : 0; // First Wave Behaviour
                    if (
                        nextTarget == null
                        || attackers < nextTargetAttackers
                        || (
                            attackers == nextTargetAttackers
                            && (
                                priority < nextTargetPriority
                                || (
                                    priority == nextTargetPriority
                                    && distanceSquared < nextTargetDistanceSquared
                                )
                            )
                        )
                    ) {
                        nextTarget = u;
                        nextTargetPriority = priority;
                        nextTargetDistanceSquared = distanceSquared;
                        nextTargetAttackers = attackers;
                    }
                }
            }
            
            if(nextTarget != null && nextTarget != currentTarget)
            {
                // This is the only place where the target is set
                LaneMinion.SetTargetUnit(nextTarget, true);
                targetUnitPriority = nextTargetPriority;
                timeSinceLastAttack = 0f;
                /*
                LogDebug("#{0}({1}) FROM TEAM {2} TARGETS #{3}({4}) FROM {5} AT DISTANCE {6} WITH PRIO {7}",
                    LaneMinion.NetId, LaneMinion.Model, LaneMinion.Team,
                    nextTarget.NetId, nextTarget.Model, nextTarget.Team,
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

            float radius = LaneMinion.CollisionRadius;
            Vector2 center = LaneMinion.Position;

            var nearestMinions = GetUnitsInRange(LaneMinion.Position, LaneMinion.AcquisitionRange, true)
                                .OfType<ILaneMinion>()
                                .OrderBy(minion => Vector2.DistanceSquared(LaneMinion.Position, minion.Position) - minion.CollisionRadius);

            // This is equivalent to making any colliding minions equal to a single minion to save on pathfinding resources.
            foreach (ILaneMinion minion in nearestMinions)
            {
                if(minion != LaneMinion){
                    // If the closest minion is in collision range, add its collision radius to the waypoint success range.
                    if (GameServerCore.Extensions.IsVectorWithinRange(minion.Position, center, radius + minion.CollisionRadius))
                    {
                        Vector2 dir = Vector2.Normalize(minion.Position - center);
                        //Vector2 pa = center + dir * (radius + minion.CollisionRadius * 2f);
                        //Vector2 pb = center - dir * radius;
                        //center = (pa + pb) * 0.5f;
                        //radius = (minion.CollisionRadius * 2f + radius * 2f) * 0.5f;

                        // Or simply
                        center += dir * minion.CollisionRadius;
                        radius += minion.CollisionRadius;
                    }
                    // If the closest minion (above) is not in collision range, then we stop the loop.
                    else break;
                }
            }

            float margin = 25f; // Otherwise, interferes with AttackableUnit which stops a little earlier 
            if (GameServerCore.Extensions.IsVectorWithinRange(currentWaypoint, center, radius + margin))
            {
                /*
                LogDebug(
                    "REACHED WAYPOINT {0} CENTER {1} RADIUS {2} ({3}, {4})",
                    currentWaypoint, center, radius,
                    LaneMinion.Position, LaneMinion.CollisionRadius
                );
                */
                return true;
            }
            return false;
        }

        OrderType ReevaluateBehavior(float delta)
        {

            //Quote: Follow any current specialized behavior rules, such as from CC (Taunts, Flees, Fears)
            
            //Quote: Continue attacking (or moving towards) their current target if that target is still valid.
            if(targetIsStillValid)
            {
                //Quote: If they have failed to attack their target for 4 seconds, they temporarily ignore them instead.
                if(timeSinceLastAttack >= 4000f)
                {
                    Ignore(LaneMinion.TargetUnit);
                    targetIsStillValid = false;
                }
                else
                {
                    return OrderType.AttackTo;
                }
                    
            }
            //if(LaneMinion.IsAttacking)
            LaneMinion.CancelAutoAttack(false, true);
            
            //Quote: Find a new valid target in the minion’s acquisition range to attack.
            //Quote: If multiple valid targets, prioritize based on “how hard is it for me to path there?”
            if(FoundNewTarget())
            {
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
                
                // It might be better to use the AttackMove state,
                // but it forces ObjAIBase to look for a target instead of LaneMinionAI 
                return OrderType.MoveTo;
            }

            return OrderType.Stop;
        }
    }
}