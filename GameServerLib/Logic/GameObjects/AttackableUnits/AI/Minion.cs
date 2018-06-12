using System.Collections.Generic;
using System.Numerics;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.GameObjects.Stats;

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
    }

    public enum MinionSpawnType : byte
    {
        MINION_TYPE_MELEE = 0x00,
        MINION_TYPE_CASTER = 0x03,
        MINION_TYPE_CANNON = 0x02,
        MINION_TYPE_SUPER = 0x01
    }

    public class Minion : ObjAIBase
    {
        /// <summary>
        /// Const waypoints that define the minion's route
        /// </summary>
        protected List<Vector2> _mainWaypoints;
        protected int _curMainWaypoint;
        public MinionSpawnPosition SpawnPosition { get; private set; }
        protected MinionSpawnType _minionType;
        protected bool _aiPaused;

        public Minion(
            MinionSpawnType type,
            MinionSpawnPosition position,
            List<Vector2> mainWaypoints,
            uint netId = 0
        ) : base("", 40, 0, 0, 1100, netId)
        {
            _minionType = type;
            SpawnPosition = position;
            _mainWaypoints = mainWaypoints;
            _curMainWaypoint = 0;
            _aiPaused = false;

            var spawnSpecifics = _game.Map.MapGameScript.GetMinionSpawnPosition(SpawnPosition);
            SetTeam(spawnSpecifics.Item1);
            setPosition(spawnSpecifics.Item2.X, spawnSpecifics.Item2.Y);

            _game.Map.MapGameScript.SetMinionStats(this); // Let the map decide how strong this minion has to be.

            // Set model
            Model = _game.Map.MapGameScript.GetMinionModel(spawnSpecifics.Item1, type);

            CharData.Load(Model);

            Stats.LoadStats(Model, CharData);

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

        public override void UpdateReplication()
        {
            ReplicationManager.UpdateFloat(Stats.CurrentHealth, 1, 0);
            ReplicationManager.UpdateFloat(Stats.TotalHealth, 1, 1);
            ReplicationManager.UpdateFloat(Stats.LifeTime, 1, 2);
            ReplicationManager.UpdateFloat(Stats.MaxLifeTime, 1, 3);
            ReplicationManager.UpdateFloat(Stats.LifeTimeTicks, 1, 4);
            ReplicationManager.UpdateFloat(Stats.TotalPar, 1, 5);
            ReplicationManager.UpdateFloat(Stats.CurrentPar, 1, 6);
            ReplicationManager.UpdateUint((uint)Stats.ActionState, 1, 7);
            ReplicationManager.UpdateBool(Stats.IsMagicImmune, 1, 8);
            ReplicationManager.UpdateBool(Stats.IsInvulnerable, 1, 9);
            ReplicationManager.UpdateBool(Stats.IsPhysicalImmune, 1, 10);
            ReplicationManager.UpdateBool(Stats.IsLifestealImmune, 1, 11);
            ReplicationManager.UpdateFloat(Stats.BaseAttackDamage, 1, 12);
            ReplicationManager.UpdateFloat(Stats.TotalArmor, 1, 13);
            ReplicationManager.UpdateFloat(Stats.TotalMagicResist, 1, 14);
            ReplicationManager.UpdateFloat(Stats.PercentAttackSpeedMod + Stats.PercentAttackSpeedDebuff, 1, 15);
            ReplicationManager.UpdateFloat(Stats.FlatAttackDamageMod, 1, 16);
            ReplicationManager.UpdateFloat(Stats.PercentAttackDamageMod, 1, 17);
            ReplicationManager.UpdateFloat(Stats.FlatAbilityPower, 1, 18);
            ReplicationManager.UpdateFloat(Stats.TotalHealthRegen, 1, 19);
            ReplicationManager.UpdateFloat(Stats.TotalParRegen, 1, 20);
            ReplicationManager.UpdateFloat(Stats.FlatMagicReduction, 1, 21);
            ReplicationManager.UpdateFloat(1 - Stats.PercentMagicReduction, 1, 22);
            ReplicationManager.UpdateFloat(0, 3, 0); // FlatSightRangeMod
            ReplicationManager.UpdateFloat(0, 3, 1); // PercentSightRangeMod
            ReplicationManager.UpdateFloat(Stats.TotalMovementSpeed, 3, 2);
            ReplicationManager.UpdateFloat(Stats.TotalSize, 3, 3);
            ReplicationManager.UpdateBool(Stats.IsTargetable, 3, 4);
            ReplicationManager.UpdateUint((uint)Stats.IsTargetableToTeam, 3, 5);
        }

        public MinionSpawnType getType()
        {
            return _minionType;
        }

        public void PauseAI(bool b)
        {
            _aiPaused = b;
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
                if (IsDashing || _aiPaused)
                {
                    return;
                }

                if (scanForTargets()) // returns true if we have a target
                {
                    keepFocussingTarget(); // fight target
                }
                else
                {
                    walkToDestination(); // walk to destination (or target)
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

        protected void walkToDestination()
        {
            if (_mainWaypoints.Count > _curMainWaypoint + 1)
            {
                if ((Waypoints.Count == 1) || (CurWaypoint == 2 && ++_curMainWaypoint < _mainWaypoints.Count))
                {
                    //CORE_INFO("Minion reached a point! Going to %f; %f", mainWaypoints[curMainWaypoint].X, mainWaypoints[curMainWaypoint].Y);
                    List<Vector2> newWaypoints = new List<Vector2> { new Vector2(X, Y), _mainWaypoints[_curMainWaypoint] };
                    SetWaypoints(newWaypoints);
                }
            }
        }
        protected void keepFocussingTarget()
        {
            if (IsAttacking && (TargetUnit == null || GetDistanceTo(TargetUnit) > Stats.TotalAttackRange))
            // If target is dead or out of range
            {
                _game.PacketNotifier.NotifyStopAutoAttack(this);
                IsAttacking = false;
            }
        }
    }
}
