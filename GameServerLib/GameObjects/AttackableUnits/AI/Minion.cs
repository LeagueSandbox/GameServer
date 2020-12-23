﻿using System.Linq;
using GameServerCore;
using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.GameObjects.Stats;
using GameServerCore.Enums;

namespace LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI
{
    public class Minion : ObjAiBase, IMinion
    {
        public string Name { get; }
        public IObjAiBase Owner { get; } // We'll probably want to change this in the future
        public bool IsWard { get; protected set; }
        public bool IsPet { get; protected set; }
        public bool IsBot { get; protected set; }
        public bool IsLaneMinion { get; protected set; }
        public bool IsClone { get; protected set; }
        protected bool _aiPaused;

        public Minion(
            Game game,
            IObjAiBase owner,
            float x,
            float y,
            string model,
            string name,
            int visionRadius = 0,
            uint netId = 0
        ) : base(game, model, new Stats.Stats(), 40, x, y, visionRadius, netId)
        {
            Name = name;

            Owner = owner;

            if (!(Owner == null) && Owner is IChampion)
            {
                SetTeam(Owner.Team);
                IsPet = true;
                if (model == Owner.Model) // Placeholder, should be changed
                {
                    IsClone = true;
                }
            }
            else
            {
                SetTeam(Team);
                IsPet = false;
            }
            IsLaneMinion = false;

            // Fix issues induced by having an empty model string
            CollisionRadius = CharData.PathfindingCollisionRadius;

            SetVisibleByTeam(Team, true);

            MoveOrder = MoveOrder.MOVE_ORDER_MOVE;

            Replication = new ReplicationMinion(this);
        }

        public void PauseAi(bool b)
        {
            _aiPaused = b;
        }

        public override void OnAdded()
        {
            base.OnAdded();
            _game.PacketNotifier.NotifySpawn(this, Team);
        }

        public override void Update(float diff)
        {
            base.Update(diff);
            if (!IsDead)
            {
                if (IsDashing || _aiPaused)
                {
                    Replication.Update();
                    return;
                }
                AIMove();
            }
            Replication.Update();
        }

        public virtual bool AIMove()
        {
            if (ScanForTargets()) // returns true if we have a target
            {
                if (!RecalculateAttackPosition())
                {
                    KeepFocusingTarget(); // attack/follow target
                }
                return false;
            }
            return true;
        }

        // AI tasks
        protected bool ScanForTargets()
        {
            if(TargetUnit != null && !TargetUnit.IsDead)
            {
                return true;
            }
            IAttackableUnit nextTarget = null;
            var nextTargetPriority = 14;
            var objects = _game.ObjectManager.GetObjects();
            //Find target closest to max attack range.
            foreach (var it in objects.OrderBy(x => GetDistanceTo(x.Value) - Stats.Range.Total))
            {
                if (!(it.Value is IAttackableUnit u) ||
                    u.IsDead ||
                    u.Team == Team ||
                    GetDistanceTo(u) > DETECT_RANGE ||
                    !_game.ObjectManager.TeamHasVisionOn(Team, u))
                    continue;
                var priority = (int)ClassifyTarget(u);  // get the priority.
                if (priority < nextTargetPriority) // if the priority is lower than the target we checked previously
                {
                    nextTarget = u;                // make him a potential target.
                    nextTargetPriority = priority;
                }
            }
            if (nextTarget != null) // If we have a target
            {
                TargetUnit = nextTarget; // Set the new target and refresh waypoints
                _game.PacketNotifier.NotifySetTarget(this, nextTarget);
                return true;
            }
            _game.PacketNotifier.NotifyNPC_InstantStopAttack(this, false);
            IsAttacking = false;
            return false;
        }

        protected void KeepFocusingTarget()
        {
            if (IsAttacking && (TargetUnit == null || TargetUnit.IsDead || GetDistanceTo(TargetUnit) > Stats.Range.Total))
            // If target is dead or out of range
            {
                _game.PacketNotifier.NotifyNPC_InstantStopAttack(this, false);
                IsAttacking = false;
            }
        }
    }
}
