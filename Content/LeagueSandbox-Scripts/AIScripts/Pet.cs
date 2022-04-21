using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using LeagueSandbox.GameServer.API;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Scripting.CSharp;
using System.Linq;
using GameServerCore;

namespace AIScripts
{
    public class Pet : IAIScript
    {
        public IAIScriptMetaData AIScriptMetaData { get; set; } = new AIScriptMetaData();

        internal const float FAR_MOVEMENT_DISTANCE = 1000.0f;
        internal const float TELEPORT_DISTANCE = 2000.0f;
        internal const float FEAR_WANDER_DISTANCE = 500.0f;
        internal const int PET_COMMAND_BUFF_TYPE = 0;
        internal const float DETECT_RANGE = 475.0f;
        float _timerScan;
        float _timerScanThreshold = 0.15f;
        float _timerFindEnemies;
        float _timerFindEnemiesThreshold = 0.15f;
        float _timerFeared;
        float _timerFearedThreshold = 1.0f;
        IMinion minion;

        public void OnActivate(IObjAiBase owner)
        {
            minion = owner as IMinion;

            ApiEventManager.OnUnitUpdateMoveOrder.AddListener(this, owner, OnOrder, false);
            ApiEventManager.OnTargetLost.AddListener(this, owner, OnTargetLost, false);
        }

        public bool OnOrder(IObjAiBase ai, OrderType order)
        {
            IAttackableUnit target = ai.TargetUnit;
            AIState state = ai.GetAIState();

            if (state == AIState.AI_HALTED)
            {
                return true;
            }
            if (state == AIState.AI_TAUNTED || state == AIState.AI_FEARED)
            {
                return true;
            }
            if ((state == AIState.AI_PET_HARDATTACK
                || state == AIState.AI_PET_HARDMOVE
                || state == AIState.AI_PET_HARDIDLE
                || state == AIState.AI_PET_HARDIDLE_ATTACKING
                || state == AIState.AI_PET_HARDRETURN
                || state == AIState.AI_PET_HARDSTOP)
                &&
                (order == OrderType.AttackTo
                || order == OrderType.MoveTo
                || order == OrderType.AttackMove
                || order == OrderType.Stop))
            {
                return true;
            }

            if (ai is IPet pet)
            {
                IObjAiBase owner = pet.Owner;
                if (owner == null)
                {
                    pet.Die(CreateDeathData(false, 0, pet, pet, DamageType.DAMAGE_TYPE_TRUE, DamageSource.DAMAGE_SOURCE_INTERNALRAW, 0.0f));
                    return false;
                }
                if (order == OrderType.MoveTo)
                {
                    if (Vector2.DistanceSquared(ai.Position, ai.Waypoints.Last()) > FAR_MOVEMENT_DISTANCE * FAR_MOVEMENT_DISTANCE
                        || state == AIState.AI_PET_HOLDPOSITION || state == AIState.AI_PET_HOLDPOSITION_ATTACKING)
                    {
                        ai.SetAIState(AIState.AI_PET_MOVE);
                        // TODO: Move to owner.
                        RemoveBuff(ai, "PetCommandParticle");
                    }
                    return true;
                }
                if (order == OrderType.AttackMove)
                {
                    ai.SetAIState(AIState.AI_PET_ATTACKMOVE);
                    // TODO: Move to owner.
                    RemoveBuff(ai, "PetCommandParticle");
                    return true;
                }
                if (order == OrderType.PetHardReturn)
                {
                    ai.SetAIState(AIState.AI_PET_HARDRETURN);
                    // TODO: Move to owner.
                    // TODO: source & target may be backwards
                    AddBuff("PetCommandParticle", 45.0f, 1, null, ai, owner);
                    return true;
                }
            }

            if (order == OrderType.AttackTo)
            {
                if (target == null)
                {
                    return false;
                }
                // TODO: Disable attacking
                ai.SetAIState(AIState.AI_PET_ATTACK);
                // TODO: Move to target.
                RemoveBuff(ai, "PetCommandParticle");
                return true;
            }
            if (order == OrderType.Stop)
            {
                RemoveBuff(ai, "PetCommandParticle");
                return true;
            }
            if (order == OrderType.PetHardStop)
            {
                // TODO: Disable attacking
                ai.SetAIState(AIState.AI_PET_HARDSTOP);
                // TODO: May not cause correct behavior.
                // The correct behavior is "moving" towards our current position.
                ai.StopMovement();
                RemoveBuff(ai, "PetCommandParticle");
                return true;
            }
            if (order == OrderType.PetHardAttack)
            {
                if (target == null)
                {
                    return false;
                }
                // TODO: Disable attacking
                ai.SetAIState(AIState.AI_PET_HARDATTACK);
                // TODO: Move to target.
                if (target is IObjAiBase targetAi)
                {
                    // TODO: source & target may be backwards
                    AddBuff("PetCommandParticle", 45.0f, 1, null, ai, targetAi);
                }
                return true;
            }
            if (order == OrderType.PetHardMove)
            {
                ai.SetAIState(AIState.AI_PET_HARDMOVE);
                // TODO: Move to given position.
                AddBuff("PetCommandParticle", 45.0f, 1, null, ai, ai);
                return true;
            }
            if (order == OrderType.Hold)
            {
                RemoveBuff(ai, "PetCommandParticle");
                ai.SetAIState(AIState.AI_PET_HOLDPOSITION);
                // TODO: May not cause correct behavior.
                // The correct behavior is "moving" towards our current position.
                ai.StopMovement();
                return true;
            }

            PrintChat("BAD ORDER " + order + ", HOW DID YOU EVEN SEND THIS?");
            return false;
        }

        public void OnTargetLost(IAttackableUnit lostTarget)
        {
            if (minion != null)
            {
                AIState state = minion.GetAIState();
                IObjAiBase owner = minion.Owner;

                if (state == AIState.AI_HALTED)
                {
                    return;
                }
                if (state == AIState.AI_PET_MOVE
                    || state == AIState.AI_PET_HARDMOVE
                    || state == AIState.AI_PET_HARDRETURN
                    || state == AIState.AI_FEARED
                    || state == AIState.AI_PET_HARDSTOP)
                {
                    return;
                }
                IAttackableUnit newTarget = GetTargetInAttackRange();
                if (newTarget == null)
                {
                    if (owner == null)
                    {
                        minion.Die(CreateDeathData(false, 0, minion, minion, DamageType.DAMAGE_TYPE_TRUE, DamageSource.DAMAGE_SOURCE_INTERNALRAW, 0.0f));
                        return;
                    }
                    if (state == AIState.AI_PET_HARDIDLE_ATTACKING)
                    {
                        //NotifySetState(AIState.AI_PET_HARDIDLE);
                        return;
                    }
                    else if (state == AIState.AI_PET_HOLDPOSITION_ATTACKING)
                    {
                        //NotifySetState(AI_PET_HOLDPOSITION);
                        return;
                    }
                    else if (state == AIState.AI_PET_RETURN_ATTACKING)
                    {
                        minion.SetAIState(AIState.AI_PET_RETURN);
                        // TODO: Move to owner
                        return;
                    }
                    else if (state == AIState.AI_PET_ATTACKMOVE_ATTACKING)
                    {
                        minion.SetAIState(AIState.AI_PET_ATTACKMOVE);
                        // TODO: Move to owner
                        return;
                    }
                }
                else if (state == AIState.AI_PET_HARDATTACK || state == AIState.AI_PET_ATTACK || state == AIState.AI_TAUNTED)
                {
                    minion.SetAIState(AIState.AI_PET_ATTACK);
                    // TODO: Move to newTarget
                    return;
                }
                else if (state == AIState.AI_PET_HARDATTACK || state == AIState.AI_PET_ATTACK || state == AIState.AI_TAUNTED)
                {
                    minion.SetAIState(AIState.AI_PET_ATTACK);
                    // TODO: Move to newTarget
                    return;
                }
                else if (state == AIState.AI_PET_HARDIDLE_ATTACKING)
                {
                    //NotifySetState(AIState.AI_PET_HARDIDLE_ATTACKING);
                    minion.SetTargetUnit(newTarget);
                    return;
                }
                else if (state == AIState.AI_PET_HOLDPOSITION_ATTACKING)
                {
                    //NotifySetState(AIState.AI_PET_HOLDPOSITION_ATTACKING);
                    minion.SetTargetUnit(newTarget);
                    return;
                }
                else if (state == AIState.AI_PET_RETURN_ATTACKING)
                {
                    minion.SetAIState(AIState.AI_PET_RETURN_ATTACKING);
                    // TODO: Move to newTarget
                    return;
                }
                else if (state == AIState.AI_PET_ATTACKMOVE_ATTACKING)
                {
                    minion.SetAIState(AIState.AI_PET_ATTACKMOVE_ATTACKING);
                    // TODO: Move to newTarget
                    return;
                }
                //NotifySetState(AIState.AI_PET_IDLE);
                return;
            }
        }

        public void OnUpdate(float diff)
        {
            if (!minion.IsDead)
            {
                if (minion.MovementParameters != null || minion.IsAiPaused())
                {
                    return;
                }

                _timerScan += diff;
                if (_timerScan / 1000f >= _timerScanThreshold)
                {
                    TimerScan();
                }

                _timerFindEnemies += diff;
                if (_timerFindEnemies / 1000f >= _timerFindEnemiesThreshold)
                {
                    TimerFindEnemies();
                }

                _timerFeared += diff;
                if (_timerFeared / 1000f >= _timerFearedThreshold)
                {

                }
            }
        }

        public void TimerScan()
        {
            if (minion != null)
            {
                AIState state = minion.GetAIState();

                if (state == AIState.AI_HALTED)
                {
                    return;
                }

                IObjAiBase owner = minion.Owner;
                if (owner == null)
                {
                    minion.Die(CreateDeathData(false, 0, minion, minion, DamageType.DAMAGE_TYPE_TRUE, DamageSource.DAMAGE_SOURCE_INTERNALRAW, 0.0f));
                }

                Vector2 myEdge = Extensions.GetClosestCircleEdgePoint(owner.Position, minion.Position, minion.CollisionRadius);
                Vector2 ownerEdge = Extensions.GetClosestCircleEdgePoint(minion.Position, owner.Position, owner.CollisionRadius);
                float distToOwner = Vector2.Distance(myEdge, ownerEdge);

                if (distToOwner > TELEPORT_DISTANCE)
                {
                    TeleportTo(minion, ownerEdge.X, ownerEdge.Y);
                    RemoveBuff(minion, "PetCommandParticle");
                    //NotifySetState(AIState.AI_PET_IDLE);
                    return;
                }

                bool noEnemiesNearby = GetTargetInAttackRange() == null;

                if (noEnemiesNearby && state == AIState.AI_PET_IDLE && distToOwner > GetPetReturnRadius(minion))
                {
                    minion.SetAIState(AIState.AI_PET_RETURN);
                    // TODO: Move to owner
                    return;
                }
                if ((state == AIState.AI_PET_RETURN || state == AIState.AI_PET_HARDRETURN) && distToOwner <= GetPetReturnRadius(minion))
                {
                    //NotifySetState(AIState.AI_PET_IDLE);
                    return;
                }
                if (minion.IsPathEnded() && state == AIState.AI_PET_HARDMOVE)
                {
                    //NotifySetState(AIState.AI_PET_HARDIDLE);
                    return;
                }
            }
        }

        public void TimerFindEnemies()
        {
            if (minion != null)
            {
                AIState state = minion.GetAIState();

                if (state == AIState.AI_HALTED)
                {
                    return;
                }

                IObjAiBase owner = minion.Owner;
                if (owner == null)
                {
                    minion.Die(CreateDeathData(false, 0, minion, minion, DamageType.DAMAGE_TYPE_TRUE, DamageSource.DAMAGE_SOURCE_INTERNALRAW, 0.0f));
                }

                if (state == AIState.AI_PET_MOVE
                    || state == AIState.AI_PET_HARDMOVE
                    || state == AIState.AI_PET_HARDSTOP)
                {
                    return;
                }
                if (state == AIState.AI_PET_IDLE
                    || state == AIState.AI_PET_RETURN
                    || state == AIState.AI_PET_ATTACKMOVE
                    || state == AIState.AI_PET_HARDIDLE
                    || state == AIState.AI_PET_HOLDPOSITION)
                {
                    IAttackableUnit newTarget = GetTargetInAttackRange();

                    if (newTarget == null)
                    {
                        minion.CancelAutoAttack(false, true);
                        return;
                    }

                    if (state != AIState.AI_PET_HARDATTACK
                        && state != AIState.AI_PET_HARDMOVE
                        && state != AIState.AI_PET_HARDIDLE
                        && state != AIState.AI_PET_HARDIDLE_ATTACKING
                        && state != AIState.AI_PET_HARDRETURN)
                    {
                        RemoveBuff(minion, "PetCommandParticle");
                    }
                    if (state == AIState.AI_PET_IDLE)
                    {
                        minion.SetAIState(AIState.AI_PET_ATTACK);
                        // TODO: Move to newTarget
                    }
                    else if (state == AIState.AI_PET_RETURN)
                    {
                        minion.SetAIState(AIState.AI_PET_RETURN_ATTACKING);
                        // TODO: Move to newTarget
                    }
                    else if (state == AIState.AI_PET_ATTACKMOVE)
                    {
                        minion.SetAIState(AIState.AI_PET_ATTACKMOVE_ATTACKING);
                        // TODO: Move to newTarget
                    }
                    else if (state == AIState.AI_PET_HARDIDLE)
                    {
                        //NotifySetState(AIState.AI_PET_HARDIDLE_ATTACKING);
                        minion.SetTargetUnit(newTarget);
                    }
                }
                //if (state == AIState.AI_PET_ATTACK
                //    || state == AIState.AI_PET_HARDATTACK
                //    || state == AIState.AI_PET_RETURN_ATTACKING
                //    || state == AIState.AI_PET_ATTACKMOVE_ATTACKING
                //    || state == AIState.AI_PET_HARDIDLE_ATTACKING
                //    || state == AIState.AI_PET_HOLDPOSITION_ATTACKING
                //    || state == AIState.AI_TAUNTED)
                //{
                //    if (TargetInAttackRange())
                //    {
                //        // TODO: Enable attacking
                //    }
                //    else if (!TargetInCancelAttackRange())
                //    {
                //        // TODO: Disable attacking
                //    }
                //    return;
                //}
            }
        }

        private IAttackableUnit GetTargetInAttackRange()
        {
            IAttackableUnit nextTarget = null;
            var nextTargetPriority = 14;
            var nearestObjects = GetUnitsInRange(minion.Position, minion.Stats.Range.Total, true);
            //Find target closest to max attack range.
            foreach (var it in nearestObjects.OrderBy(x => Vector2.DistanceSquared(minion.Position, x.Position) - (minion.Stats.Range.Total * minion.Stats.Range.Total)))
            {
                if (!(it is IAttackableUnit u)
                    || u.IsDead
                    || u.Team == minion.Team
                    || Vector2.DistanceSquared(minion.Position, u.Position) > DETECT_RANGE * DETECT_RANGE
                    || !u.IsVisibleByTeam(minion.Team)
                    || !u.Status.HasFlag(StatusFlags.Targetable)
                    || UnitIsProtectionActive(u))
                {
                    continue;
                }

                // get the priority.
                var priority = (int)minion.ClassifyTarget(u);
                // if the priority is lower than the target we checked previously
                if (priority < nextTargetPriority)
                {
                    // make it a potential target.
                    nextTarget = u;
                    nextTargetPriority = priority;
                }
            }

            return nextTarget;
        }
    }
}