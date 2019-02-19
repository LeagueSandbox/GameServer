using System.Linq;
using GameServerCore;
using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.GameObjects.Stats;
using GameServerCore.Enums;
using GameMaths.Geometry.Polygons;
using System.Collections.Generic;
using System.Numerics;

namespace LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI
{
    public class Minion : ObjAiBase, IMinion
    {
        public string Name { get; }
        public IObjAiBase Owner { get; } // We'll probably want to change this in the future
        public bool IsWard { get; }
        public bool IsPet { get; }
        public bool IsBot { get; }
        public bool IsLaneMinion { get; }
        public bool IsClone { get; }
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

            if (this is ILaneMinion)
            {
                IsLaneMinion = true;
            }
            else
            {
                IsLaneMinion = false;
            }

            //TODO: Fix health not showing unless visible to enemy and health not updating when taking damage

            // Fix issues induced by having an empty model string
            CollisionRadius = _game.Config.ContentManager.GetCharData(Model).PathfindingCollisionRadius;

            SetVisibleByTeam(Team, true);

            MoveOrder = MoveOrder.MOVE_ORDER_MOVE;

            Replication = new ReplicationMinion(this);
        }

        public void PauseAi(bool b)
        {
            _aiPaused = b;
        }

        public override void OnCollision(IGameObject collider)
        {
            base.OnCollision(collider);
            if (collider == null) return;
            var curCircle = new CirclePoly(GetPosition(), collider.CollisionRadius + 10, 72);
            var targetCircle = new CirclePoly(GetNextWaypoint(), Stats.Range.Total, 72);
            var collideCircle = new CirclePoly(collider.GetPosition(), collider.CollisionRadius + 10, 72);
            //Find optimal position...
            bool found = false;
            foreach (var point in targetCircle.Points.OrderBy(x => GetDistanceTo(X, Y)))
            {
                if (!_game.Map.NavGrid.IsWalkable(point))
                    continue;
                var positionCollide = false;
                if (collideCircle.CheckForOverLaps(new CirclePoly(point, CollisionRadius + 10, 20)))
                {
                    positionCollide = true;
                }
                if (positionCollide)
                    continue;
                positionCollide = false;
                Vector2 toCollide = Vector2.Normalize(collideCircle.Center - curCircle.Center);
                // Rotate so there isn't little collides (more than orthogonal
                toCollide = toCollide.Rotate(curCircle.Center, 90.0f);
                toCollide = GetPosition() + new Vector2(toCollide.X * curCircle.Radius, toCollide.Y * curCircle.Radius);

                found = true;
                var newWaypoints = new List<Vector2> { toCollide, point };
                newWaypoints.AddRange(Waypoints.GetRange(WaypointIndex+1, Waypoints.Count- (WaypointIndex+1)));
                SetWaypoints(newWaypoints);
                break;
            }
            if (!found && Waypoints.Any()) StopMovement();
        }

        public override void OnAdded()
        {
            base.OnAdded();
            if (!IsLaneMinion)
            {
                _game.PacketNotifier.NotifySpawn(this);
            }
            else
            {
                _game.PacketNotifier.NotifyLaneMinionSpawned((ILaneMinion)this, Team);
            }
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
                if (ScanForTargets()) // returns true if we have a target
                {
                    if (!RecalculateAttackPosition())
                    {
                        KeepFocusingTarget(); // attack/follow target
                    }
                }
            }
            Replication.Update();
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
            foreach (var it in objects.OrderBy(x => GetDistanceTo(x.Value) - Stats.Range.Total))//Find target closest to max attack range.
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
            _game.PacketNotifier.NotifyStopAutoAttack(this);
            IsAttacking = false;
            return false;
        }

        protected void KeepFocusingTarget()
        {
            if (IsAttacking && (TargetUnit == null || TargetUnit.IsDead || GetDistanceTo(TargetUnit) > Stats.Range.Total))
            // If target is dead or out of range
            {
                _game.PacketNotifier.NotifyStopAutoAttack(this);
                IsAttacking = false;
            }
        }
    }
}
