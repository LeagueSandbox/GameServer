using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Scripting.CSharp;
using System.Linq;
using System.Collections.Generic;
using System;

namespace AIScripts
{
    public class LaneMinionAI : IAIScript
    {
        public IAIScriptMetaData AIScriptMetaData { get; set; } = new AIScriptMetaData
        {
            HandlesCallsForHelp = true
        };
        ILaneMinion LaneMinion;
        int currentWaypointIndex = 0;
        float minionActionTimer = 250f;
        bool targetIsStillValid = false;
        Dictionary<uint, float> temporaryIgnored = new Dictionary<uint, float>();
        public Dictionary<IAttackableUnit, int> unitsAttackingAllies { get; } = new Dictionary<IAttackableUnit, int>();
        float timeSinceLastAttack = 0f;
        int targetUnitPriority = (int) ClassifyUnit.DEFAULT;
        float localTime = 0f;
        bool callsForHelpMayBeCleared = false;
        bool followsWaypoints = true;
        public void OnActivate(IObjAiBase owner)
        {
            LaneMinion = owner as ILaneMinion;
        }
        
        public void OnUpdate(float delta)
        {
            localTime += delta;

            if(LaneMinion != null && !LaneMinion.IsDead)
            {

                if(LaneMinion.IsAttacking || LaneMinion.TargetUnit == null)
                {
                    timeSinceLastAttack = 0f;
                }
                else
                {
                    timeSinceLastAttack += delta;
                }

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
                    //&& !LaneMinion.RecalculateAttackPosition()
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
                    unitsAttackingAllies.Clear();
                }
            }
        }

        public void OnCallForHelp(IAttackableUnit attacker, IAttackableUnit victium)
        {
            if(unitsAttackingAllies != null)
            {
                int priority = Math.Min(
                    unitsAttackingAllies.GetValueOrDefault(attacker, (int)ClassifyUnit.DEFAULT),
                    (int)LaneMinion.ClassifyTarget(attacker, victium)
                );
                unitsAttackingAllies[attacker] = priority;
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
                && UnitInRange(u, LaneMinion.Stats.AcquisitionRange.Total)
                && u.IsVisibleByTeam(LaneMinion.Team)
                && u.Status.HasFlag(StatusFlags.Targetable)
                && !UnitIsProtectionActive(u)
            );
        }

        void Ignore(IAttackableUnit unit, float time = 500)
        {
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
            float acquisitionRange = LaneMinion.Stats.AcquisitionRange.Total;
            float nextTargetDistanceSquared = acquisitionRange * acquisitionRange;
            int nextTargetAttackers = 0;
            if(targetIsStillValid)
            {
                currentTarget = LaneMinion.TargetUnit;
                nextTarget = currentTarget;
                nextTargetPriority = targetUnitPriority;
                nextTargetDistanceSquared = Vector2.DistanceSquared(LaneMinion.Position, nextTarget.Position);
                nextTargetAttackers = 0; //LaneMinion.IsMelee ? CountUnitsAttackingUnit(nextTarget) : 0; // First Wave Behaviour is unfinished
            }
            
            FilterTemporaryIgnoredList();

            IEnumerable<IAttackableUnit> nearestObjects;
            if(handleOnlyCallsForHelp)
            {
                if(unitsAttackingAllies.Count == 0)
                {
                    return false;
                }
                nearestObjects = unitsAttackingAllies.Keys;
            }
            else
            {
                nearestObjects = EnumerateUnitsInRange(LaneMinion.Position, acquisitionRange, true);
            }
            foreach (var it in nearestObjects)
            {
                if (it is IAttackableUnit u && IsValidTarget(u) && !temporaryIgnored.ContainsKey(u.NetId))
                {
                    int priority = unitsAttackingAllies.ContainsKey(u) ?
                        unitsAttackingAllies[u]
                        : (int)LaneMinion.ClassifyTarget(u)
                    ;
                    float distanceSquared = Vector2.DistanceSquared(LaneMinion.Position, u.Position);
                    int attackers = 0; //LaneMinion.IsMelee ? CountUnitsAttackingUnit(u) : 0; // First Wave Behaviour is unfinished
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
                followsWaypoints = false;

                return true;
            }
            return false;
        }

        bool WaypointReached()
        {
            Vector2 currentWaypoint = LaneMinion.PathingWaypoints[currentWaypointIndex];

            float radius = LaneMinion.CollisionRadius;
            Vector2 center = LaneMinion.Position;

            var nearestMinions = EnumerateUnitsInRange(LaneMinion.Position, LaneMinion.Stats.AcquisitionRange.Total, true)
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
            
            LaneMinion.CancelAutoAttack(false, true);
            LaneMinion.SetTargetUnit(null, true);

            //Quote: Find a new valid target in the minion’s acquisition range to attack.
            //Quote: If multiple valid targets, prioritize based on “how hard is it for me to path there?”
            if (FoundNewTarget())
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

                if(currentDestination != currentWaypoint)
                {
                    List<Vector2> path = null;
                    
                    // If the minion returns to lane
                    // Instead of continuing to move along the waypoints
                    if(!followsWaypoints)
                    {
                        followsWaypoints = true;
                        path = GetPath(LaneMinion.Position, currentWaypoint);
                    }

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