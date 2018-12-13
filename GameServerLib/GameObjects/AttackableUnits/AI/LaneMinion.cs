using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects.Stats;

namespace LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI
{
    public class LaneMinion : Minion, ILaneMinion
    {
        /// <summary>
        /// Const waypoints that define the minion's route
        /// </summary>
        protected List<Vector2> _mainWaypoints;
        protected int _curMainWaypoint;
        public MinionSpawnPosition SpawnPosition { get; }
        public MinionSpawnType MinionSpawnType { get; }
        protected bool _aiPaused;

        public LaneMinion(
            Game game,
            MinionSpawnType spawnType,
            MinionSpawnPosition position,
            List<Vector2> mainWaypoints,
            uint netId = 0
        ) : base(game, null, 0, 0, "", "", 1100, netId)
        {
            MinionSpawnType = spawnType;
            SpawnPosition = position;
            _mainWaypoints = mainWaypoints;
            _curMainWaypoint = 0;
            _aiPaused = false;

            var spawnSpecifics = _game.Map.MapProperties.GetMinionSpawnPosition(SpawnPosition);
            SetTeam(spawnSpecifics.Item1);
            SetPosition(spawnSpecifics.Item2.X, spawnSpecifics.Item2.Y);

            _game.Map.MapProperties.SetMinionStats(this); // Let the map decide how strong this minion has to be.

            // Set model
            Model = _game.Map.MapProperties.GetMinionModel(spawnSpecifics.Item1, spawnType);

            // Fix issues induced by having an empty model string
            CollisionRadius = _game.Config.ContentManager.GetCharData(Model).PathfindingCollisionRadius;

            // If we have lane path instructions from the map
            if (mainWaypoints.Count > 0)
            {
                // Follow these instructions
                SetWaypoints(new List<Vector2> { mainWaypoints[0], mainWaypoints[1] });
            }
            else
            {
                // Otherwise path to own position. (Stand still)
                SetWaypoints(new List<Vector2> { new Vector2(X, Y), new Vector2(X, Y) });
            }

            MoveOrder = MoveOrder.MOVE_ORDER_ATTACKMOVE;
            Replication = new ReplicationLaneMinion(this);
        }

        public LaneMinion(
            Game game,
            MinionSpawnType spawnType,
            MinionSpawnPosition position,
            uint netId = 0
        ) : this(game, spawnType, position, new List<Vector2>(), netId)
        {

        }

        public void PauseAi(bool b)
        {
            _aiPaused = b;
        }

        public override void OnAdded()
        {
            base.OnAdded();
            _game.PacketNotifier.NotifyLaneMinionSpawned(this, Team);
        }

        public override void Update(float diff)
        {
            base.Update(diff);
            if (!IsDead)
            {
                if (IsDashing || _aiPaused)
                {
                    return;
                }

                if (ScanForTargets()) // returns true if we have a target
                {
                    if (!RecalculateAttackPosition())
                    {
                        KeepFocussingTarget(); // attack target
                    }
                }
                else
                {
                    WalkToDestination(); // walk to destination (or target)
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

        protected void WalkToDestination()
        {
            if (_mainWaypoints.Count > _curMainWaypoint + 1)
            {
                if (Waypoints.Count == 1 || CurWaypoint >= Waypoints.Count && ++_curMainWaypoint < _mainWaypoints.Count)
                {
                    //CORE_INFO("Minion reached a point! Going to %f; %f", mainWaypoints[curMainWaypoint].X, mainWaypoints[curMainWaypoint].Y);
                    SetWaypoints(new List<Vector2>() { GetPosition(), _mainWaypoints[_curMainWaypoint] });

                    //TODO: Here we need a certain way to tell if the Minion is in the path/lane, else use pathfinding to return to the lane.
                    //I think in league when minion start chasing they save Current Position and
                    //when it stop chasing the minion return to the last saved position, and then continue main waypoints from there.

                    /*var path = _game.Map.NavGrid.GetPath(GetPosition(), _mainWaypoints[_curMainWaypoint + 1]);
                    if(path.Count > 1)
                    {
                        SetWaypoints(path);
                    }*/
                }
            }
        }

        protected void KeepFocussingTarget()
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
