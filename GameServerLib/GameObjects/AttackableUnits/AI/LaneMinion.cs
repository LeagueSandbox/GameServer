using System.Collections.Generic;
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
        /// <summary>
        /// Name of the Barracks that spawned this lane minion.
        /// </summary>
        public string BarracksName { get; }
        public MinionSpawnType MinionSpawnType { get; }

        public LaneMinion(
            Game game,
            MinionSpawnType spawnType,
            string barracksName,
            List<Vector2> mainWaypoints,
            string model,
            uint netId = 0,
            TeamId team = TeamId.TEAM_BLUE
        ) : base(game, null, new Vector2(), model, model, netId, team)
        {
            IsLaneMinion = true;
            MinionSpawnType = spawnType;
            BarracksName = barracksName;
            _mainWaypoints = mainWaypoints;
            _curMainWaypoint = 0;
            _aiPaused = false;

            var spawnSpecifics = _game.Map.MapProperties.GetMinionSpawnPosition(BarracksName);
            SetPosition(spawnSpecifics.Item2.X, spawnSpecifics.Item2.Y);

            _game.Map.MapProperties.SetMinionStats(this); // Let the map decide how strong this minion has to be.

            StopMovement();

            MoveOrder = OrderType.Hold;
            Replication = new ReplicationLaneMinion(this);
        }

        public LaneMinion(
            Game game,
            MinionSpawnType spawnType,
            string position,
            string model,
            uint netId = 0,
            TeamId team = TeamId.TEAM_BLUE
        ) : this(game, spawnType, position, new List<Vector2>(), model, netId, team)
        {
        }

        public override void OnAdded()
        {
            base.OnAdded();
        }

        public override void Update(float diff)
        {
            base.Update(diff);
        }

        public override bool AIMove()
        {
            // TODO: Use unique LaneMinion AI instead of normal Minion AI and add here for return values.
            if (ScanForTargets()) // returns true if we have a target
            {
                if (!RecalculateAttackPosition())
                {
                    KeepFocusingTarget(); // attack/follow target
                }
                return false;
            }

            // If we have lane path instructions from the map
            if (_mainWaypoints.Count > 0 && TargetUnit == null)
            {
                WalkToDestination();
            }
            return true;
        }

        public void WalkToDestination()
        {
            // TODO: Use the methods used in this function for any other minion pathfinding (attacking, targeting, etc).

            var mainWaypointCell = _game.Map.NavigationGrid.GetCellIndex(_mainWaypoints[_curMainWaypoint].X, _mainWaypoints[_curMainWaypoint].Y);
            var lastWaypointCell = 0;

            if (Waypoints.Count > 0)
            {
                lastWaypointCell = _game.Map.NavigationGrid.GetCellIndex(Waypoints[Waypoints.Count - 1].X, Waypoints[Waypoints.Count - 1].Y);
            }

            // First, we make sure we are pathfinding to our current main waypoint.
            if (lastWaypointCell != mainWaypointCell)
            {
                // Pathfind to the next waypoint.
                var path = new List<Vector2>() { Position, _mainWaypoints[_curMainWaypoint] };
                var tempPath = _game.Map.NavigationGrid.GetPath(Position, _mainWaypoints[_curMainWaypoint]);
                if (tempPath != null)
                {
                    path = tempPath;
                }

                SetWaypoints(path);
                UpdateMoveOrder(OrderType.MoveTo);

                //TODO: Here we need a certain way to tell if the Minion is in the path/lane, else use pathfinding to return to the lane.
                //I think in league when minion start chasing they save Current Position and
                //when it stop chasing the minion return to the last saved position, and then continue main waypoints from there.

                /*var path = _game.Map.NavGrid.GetPath(GetPosition(), _mainWaypoints[_curMainWaypoint + 1]);
                if(path.Count > 1)
                {
                    SetWaypoints(path);
                }*/
            }

            var waypointSuccessRange = CollisionRadius;
            var nearestObjects = _game.Map.CollisionHandler.QuadDynamic.GetNearestObjects(this);

            // This is equivalent to making any colliding minions equal to a single minion to save on pathfinding resources.
            foreach (IGameObject obj in nearestObjects)
            {
                if (obj is ILaneMinion minion)
                {
                    // If the closest minion is in collision range, add its collision radius to the waypoint success range.
                    if (GameServerCore.Extensions.IsVectorWithinRange(minion.Position, Position, waypointSuccessRange))
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
            if (GameServerCore.Extensions.IsVectorWithinRange(Position, _mainWaypoints[_curMainWaypoint], waypointSuccessRange) && _mainWaypoints.Count > _curMainWaypoint + 1)
            {
                ++_curMainWaypoint;

                // Pathfind to the next waypoint.
                var path = new List<Vector2>() { Position, _mainWaypoints[_curMainWaypoint] };
                var tempPath = _game.Map.NavigationGrid.GetPath(Position, _mainWaypoints[_curMainWaypoint]);
                if (tempPath != null)
                {
                    path = tempPath;
                }

                SetWaypoints(path);
                UpdateMoveOrder(OrderType.AttackMove);
            }
        }

        // TODO: Override KeepFocusingTarget and use unique LaneMinion AI
    }
}
