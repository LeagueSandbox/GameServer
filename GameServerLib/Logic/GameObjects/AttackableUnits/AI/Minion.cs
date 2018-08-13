using System.Collections.Generic;
using System.Numerics;
using GameServerCore.Logic.Domain.GameObjects;
using GameServerCore.Logic.Enums;
using LeagueSandbox.GameServer.Logic.GameObjects.Missiles;
using LeagueSandbox.GameServer.Logic.GameObjects.Other;
using LeagueSandbox.GameServer.Logic.GameObjects.Stats;

namespace LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.AI
{
    public class Minion : ObjAiBase, IMinion
    {
        /// <summary>
        /// Const waypoints that define the minion's route
        /// </summary>
        protected List<Vector2> _mainWaypoints;
        protected int _curMainWaypoint;
        public MinionSpawnPosition SpawnPosition { get; private set; }
        public MinionSpawnType MinionSpawnType { get; protected set; }
        protected bool _aiPaused;

        private int HitBox => 60;

        public Minion(
            Game game,
            MinionSpawnType spawnType,
            MinionSpawnPosition position,
            List<Vector2> mainWaypoints,
            uint netId = 0
        ) : base(game, "", new Stats.Stats(), 40, 0, 0, 1100, netId)
        {
            MinionSpawnType = spawnType;
            SpawnPosition = position;
            _mainWaypoints = mainWaypoints;
            _curMainWaypoint = 0;
            _aiPaused = false;

            var spawnSpecifics = _game.Map.MapGameScript.GetMinionSpawnPosition(SpawnPosition);
            SetTeam(spawnSpecifics.Item1);
            SetPosition(spawnSpecifics.Item2.X, spawnSpecifics.Item2.Y);

            _game.Map.MapGameScript.SetMinionStats(this); // Let the map decide how strong this minion has to be.

            // Set model
            Model = _game.Map.MapGameScript.GetMinionModel(spawnSpecifics.Item1, spawnType);

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
            Replication = new ReplicationMinion(this);
        }

        public Minion(
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
            _game.PacketNotifier.NotifyMinionSpawned(this, Team);
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
                    KeepFocussingTarget(); // fight target
                }
                else
                {
                    WalkToDestination(); // walk to destination (or target)
                }
            }

            Replication.Update();
        }

        public override void OnCollision(GameObject collider)
        {
            if (collider == null || collider == TargetUnit) // If we're colliding with the target, don't do anything.
            {
                return;
            }

            if (collider.GetType() == typeof(Minion))
            {
                Vector2 newPos = new Vector2(X + 120, Y + 120);
                if (SpawnPosition == MinionSpawnPosition.SPAWN_BLUE_MID)
                {
                    newPos = new Vector2(X + 120, Y + 50);
                }
                try
                {
                    Move(250, newPos);
                }
                catch
                {
                    //Minion died
                }
            }

            base.OnCollision(collider);
        }

        // AI tasks
        protected bool ScanForTargets()
        {
            AttackableUnit nextTarget = null;
            var nextTargetPriority = 14;

            var objects = _game.ObjectManager.GetObjects();
            foreach (var it in objects)
            {
                var u = it.Value as AttackableUnit;

                // Targets have to be:
                if (u == null ||                          // a unit
                    u.IsDead ||                          // alive
                    u.Team == Team ||                    // not on our team
                    GetDistanceTo(u) > DETECT_RANGE ||   // in range
                    !_game.ObjectManager.TeamHasVisionOn(Team, u)) // visible to this minion
                    continue;                             // If not, look for something else

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

            return false;
        }

        protected void WalkToDestination()
        {
            if (_mainWaypoints.Count > _curMainWaypoint + 1)
            {
                if (Waypoints.Count == 1 || CurWaypoint == 2 && ++_curMainWaypoint < _mainWaypoints.Count)
                {
                    //CORE_INFO("Minion reached a point! Going to %f; %f", mainWaypoints[curMainWaypoint].X, mainWaypoints[curMainWaypoint].Y);
                    var newWaypoints = new List<Vector2> { new Vector2(X, Y), _mainWaypoints[_curMainWaypoint] };
                    SetWaypoints(newWaypoints);
                }
            }
        }

        protected void KeepFocussingTarget()
        {
            if (IsAttacking && (TargetUnit == null || GetDistanceTo(TargetUnit) > Stats.Range.Total))
            // If target is dead or out of range
            {
                _game.PacketNotifier.NotifyStopAutoAttack(this);
                IsAttacking = false;
            }
        }
    }
}
