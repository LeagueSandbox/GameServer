﻿using System.Collections.Generic;
using System.Numerics;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.API;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public enum MinionSpawnPosition : uint
    {
        SPAWN_BLUE_TOP = 0xeb364c40,
        SPAWN_BLUE_BOT = 0x53b83640,
        SPAWN_BLUE_MID = 0xb7717140,
        SPAWN_RED_TOP = 0xe647d540,
        SPAWN_RED_BOT = 0x5ec9af40,
        SPAWN_RED_MID = 0xba00e840
    };

    public enum MinionSpawnType : byte
    {
        MINION_TYPE_MELEE = 0x00,
        MINION_TYPE_CASTER = 0x03,
        MINION_TYPE_CANNON = 0x02,
        MINION_TYPE_SUPER = 0x01
    };
    public class Minion : ObjAIBase
    {
        /// <summary>
        /// Const waypoints that define the minion's route
        /// </summary>
        protected List<Vector2> mainWaypoints;
        protected int curMainWaypoint = 0;
        public MinionSpawnPosition SpawnPosition { get; private set; }
        protected MinionSpawnType minionType;
        protected bool _AIPaused;

        public Minion(
            MinionSpawnType type,
            MinionSpawnPosition position,
            List<Vector2> mainWaypoints,
            uint netId = 0
        ) : base("", new MinionStats(), 40, 0, 0, 1100, netId)
        {
            minionType = type;
            SpawnPosition = position;
            this.mainWaypoints = mainWaypoints;
            curMainWaypoint = 0;
            _AIPaused = false;

            var spawnSpecifics = _game.Map.MapGameScript.GetMinionSpawnPosition(SpawnPosition);
            SetTeam(spawnSpecifics.Item1);
            setPosition(spawnSpecifics.Item2.X, spawnSpecifics.Item2.Y);

            _game.Map.MapGameScript.SetMinionStats(this); // Let the map decide how strong this minion has to be.

            // Set model
            Model = _game.Map.MapGameScript.GetMinionModel(spawnSpecifics.Item1, type);
            
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
        }

        public Minion(
            MinionSpawnType type,
            MinionSpawnPosition position,
            uint netId = 0
        ) : this(type, position, new List<Vector2>(), netId)
        {

        }

        public MinionSpawnType getType()
        {
            return minionType;
        }

        public void PauseAI(bool b)
        {
            _AIPaused = b;
        }
        public override void OnAdded()
        {
            base.OnAdded();
            _game.PacketNotifier.NotifyMinionSpawned(this, Team);
        }
        public override void update(float diff)
        {
            base.update(diff);

            if (!IsDead)
            {
                if (IsDashing || _AIPaused)
                {
                    return;
                }

                if (scanForTargets()) // returns true if we have a target
                {
                    keepFocussingTarget(); // fight target
                }
                else
                {
                    walkToDestination(); // walk to destination 
                }
            }
        }

        public override void onCollision(GameObject collider)
        {
            if (collider == TargetUnit) // If we're colliding with the target, don't do anything.
            {
                return;
            }

            base.onCollision(collider);
        }

        // AI tasks
        protected bool scanForTargets()
        {
            
                AttackableUnit nextTarget = null;
                var nextTargetPriority = 14;

                var _atabun = ApiFunctionManager.GetUnitsInRange(this, DETECT_RANGE, true);

                foreach (var it in _atabun)
                {
                    if (it.Team != Team && _game.ObjectManager.TeamHasVisionOn(Team, it))
                    {

                        var priority = (int)ClassifyTarget(it);  // get the priority.
                        if (priority < nextTargetPriority) // if the priority is lower than the target we checked previously
                        {
                            nextTarget = it;                // make him a potential target.
                            nextTargetPriority = priority;
                        }

                    }
                }

                if (nextTarget != null) // If we have a target
                {
                    TargetUnit = nextTarget; // Set the new target and refresh waypoints
                    _game.PacketNotifier.NotifySetTarget(this, nextTarget); // Take a look a that !!!
                    return true;
                }

            _game.PacketNotifier.NotifyStopAutoAttack(this);
            IsAttacking = false;

            return false;
            
        }



        protected void walkToDestination()
        {
            
            if (mainWaypoints.Count > curMainWaypoint + 1)
            {

                if (Target == null)
                {
                    if ((Waypoints.Count == 1) || (CurWaypoint == 10 && curMainWaypoint + 1 < mainWaypoints.Count))
                    {
                        //CORE_INFO("Minion reached a point! Going to %f; %f", mainWaypoints[curMainWaypoint].X, mainWaypoints[curMainWaypoint].Y);
                        List<Vector2> newWaypoints = new List<Vector2> { new Vector2(X, Y), mainWaypoints[curMainWaypoint] };
                        SetWaypoints(newWaypoints);
                    }
                    else
                    if ((Waypoints.Count == 1) || (CurWaypoint == 2 && ++curMainWaypoint < mainWaypoints.Count))
                    {
                        //CORE_INFO("Minion reached a point! Going to %f; %f", mainWaypoints[curMainWaypoint].X, mainWaypoints[curMainWaypoint].Y);
                        List<Vector2> newWaypoints = new List<Vector2> { new Vector2(X, Y), mainWaypoints[curMainWaypoint] };
                        SetWaypoints(newWaypoints);
                    }
                }

            }
            
        }



        protected void keepFocussingTarget()
        {
            if (IsAttacking && (TargetUnit == null || GetDistanceTo(TargetUnit) > DETECT_RANGE)) //Stats.Range.Total
            // If target is dead or out of range
            {
                _game.PacketNotifier.NotifyStopAutoAttack(this);
                IsAttacking = false;
            }
        }
    }
}
