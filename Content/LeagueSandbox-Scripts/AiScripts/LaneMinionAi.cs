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
        Dictionary<uint, float> temporaryIgnored = new Dictionary<uint, float>();
        float timeSinceLastAttack = 0f;
        int targetUnitPriority = (int) ClassifyUnit.DEFAULT;
        float localTime = 0f;
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
                && UnitInRange(u, DETECT_RANGE)
                && TeamHasVision(LaneMinion.Team, u)
                && u.Status.HasFlag(StatusFlags.Targetable)
                && !UnitIsProtected(u)
            );
        }

        void Ignore(IAttackableUnit unit, float time = 500)
        {
            LogDebug("Temporary ignoring #{0}({1})", LaneMinion.TargetUnit.NetId, LaneMinion.TargetUnit.Model);
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
            IAttackableUnit currentTarget = null;
            IAttackableUnit nextTarget = currentTarget;
            int nextTargetPriority = (int)ClassifyUnit.DEFAULT;
            float nextTargetDistanceSquared = DETECT_RANGE * DETECT_RANGE;
            int nextTargetAttackers = 0;
            if(targetIsStillValid)
            {
                //TODO: single metric?
                currentTarget = LaneMinion.TargetUnit;
                nextTarget = currentTarget;
                nextTargetPriority = targetUnitPriority;
                nextTargetDistanceSquared = Vector2.DistanceSquared(LaneMinion.Position, nextTarget.Position);
                nextTargetAttackers = 0; //LaneMinion.IsMelee ? CountUnitsAttackingUnit(nextTarget) : 0;
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
                nearestObjects = GetUnitsInRange(LaneMinion.Position, DETECT_RANGE, true);
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
                    int attackers = 0; // LaneMinion.IsMelee ? CountUnitsAttackingUnit(u) : 0;
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
                LaneMinion.SetTargetUnit(nextTarget, true);
                targetUnitPriority = nextTargetPriority;
                //TODO: Move out of there? On the other hand, this is the only place where the target is set
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

            var waypointSuccessRange = LaneMinion.CollisionRadius;
            var nearestObjects = /*(List<ILaneMinion>)*/ GetUnitsInRange(LaneMinion.Position, DETECT_RANGE, true);
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