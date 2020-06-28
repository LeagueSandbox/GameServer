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
        public string BarracksName { get; } // barracks name of the position
        public MinionSpawnType MinionSpawnType { get; }

        public LaneMinion(
            Game game,
            MinionSpawnType spawnType,
            string position,
            List<Vector2> mainWaypoints,
            uint netId = 0
        ) : base(game, null, 0, 0, "", "", 1100, netId)
        {
            IsLaneMinion = true;
            MinionSpawnType = spawnType;
            BarracksName = position;
            _mainWaypoints = mainWaypoints;
            _curMainWaypoint = 0;
            _aiPaused = false;

            var spawnSpecifics = _game.Map.MapProperties.GetMinionSpawnPosition(BarracksName);
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
            string position,
            uint netId = 0
        ) : this(game, spawnType, position, new List<Vector2>(), netId)
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
            if (base.AIMove())
            {
                WalkToDestination();
                return true;
            }
            return false;
        }

        // TODO: Override ScanForTargets and use unique LaneMinion AI

        public void WalkToDestination()
        {
            if (_mainWaypoints.Count > _curMainWaypoint + 1)
            {
                if (Waypoints.Count == 1 || WaypointIndex >= Waypoints.Count && ++_curMainWaypoint < _mainWaypoints.Count)
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

        // TODO: Override KeepFocusingTarget and use unique LaneMinion AI
    }
}
