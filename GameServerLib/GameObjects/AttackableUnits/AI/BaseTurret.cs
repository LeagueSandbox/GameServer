using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Force.Crc32;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.Buildings.AnimatedBuildings;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.Items;

namespace LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI
{
    /// <summary>
    /// Base class for Turret GameObjects.
    /// In League, turrets are separated into visual and AI objects, so this GameObject represents the AI portion,
    /// while the visual object is handled automatically by clients via packets.
    /// </summary>
    public class BaseTurret : ObjAiBase, IBaseTurret
    {
        protected float _globalGold = 250.0f;
        protected float _globalExp = 0.0f;

        /// <summary>
        /// Current lane this turret belongs to.
        /// </summary>
        public LaneID Lane { get; private set; }
        /// <summary>
        /// Internal name of this turret, used for packets so that clients know which visual turret to assign them to.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Supposed to be the NetID for the visual counterpart of this turret. Used only for packets.
        /// </summary>
        public uint ParentNetId { get; private set; }

        public BaseTurret(
            Game game,
            string name,
            string model,
            Vector2 position,
            TeamId team = TeamId.TEAM_BLUE,
            uint netId = 0,
            LaneID lane = LaneID.NONE
        ) : base(game, model, new Stats.Stats(), 88, position, 1200, netId, team)
        {
            ParentNetId = Crc32Algorithm.Compute(Encoding.UTF8.GetBytes(name)) | 0xFF000000;
            Name = name;
            Lane = lane;
            SetTeam(team);
            Inventory = InventoryManager.CreateInventory();
            Replication = new ReplicationAiTurret(this);
        }

        /// <summary>
        /// Simple target scanning function.
        /// </summary>
        /// TODO: Verify if this needs a rewrite or additions to account for special cases.
        public void CheckForTargets()
        {
            var units = _game.ObjectManager.GetUnitsInRange(Position, Stats.Range.Total, true); ;
            IAttackableUnit nextTarget = null;
            var nextTargetPriority = ClassifyUnit.DEFAULT;

            foreach (var u in units)
            {
                if (u.IsDead || u.Team == Team)
                {
                    continue;
                }

                // Note: this method means that if there are two champions within turret range,
                // The player to have been added to the game first will always be targeted before the others
                if (TargetUnit == null)
                {
                    var priority = ClassifyTarget(u);
                    if (priority < nextTargetPriority)
                    {
                        nextTarget = u;
                        nextTargetPriority = priority;
                    }
                }
                else
                {
                    // Is the current target a champion? If it is, don't do anything
                    if (!(TargetUnit is IChampion))
                    {
                        continue;
                    }
                    // Find the next champion in range targeting an enemy champion who is also in range
                    if (!(u is IChampion enemyChamp) || enemyChamp.TargetUnit == null)
                    {
                        continue;
                    }
                    if (!(enemyChamp.TargetUnit is IChampion enemyChampTarget) ||
                        Vector2.DistanceSquared(enemyChamp.Position, enemyChampTarget.Position) > enemyChamp.Stats.Range.Total * enemyChamp.Stats.Range.Total ||
                        Vector2.DistanceSquared(Position, enemyChampTarget.Position) > Stats.Range.Total * Stats.Range.Total)
                    {
                        continue;
                    }

                    nextTarget = enemyChamp; // No priority required
                    break;
                }
            }

            if (nextTarget == null)
            {
                return;
            }
            TargetUnit = nextTarget;
            _game.PacketNotifier.NotifySetTarget(this, nextTarget);
        }

        /// <summary>
        /// Called when this unit dies.
        /// </summary>
        /// <param name="killer">Unit that killed this unit.</param>
        public override void Die(IAttackableUnit killer)
        {
            foreach (var player in _game.ObjectManager.GetAllChampionsFromTeam(killer.Team))
            {
                var goldEarn = _globalGold;

                // Champions in Range within TURRET_RANGE * 1.5f will gain 150% more (obviously)
                if (Vector2.DistanceSquared(player.Position, Position) <= (Stats.Range.Total * 1.5f) * (Stats.Range.Total * 1.5f) && !player.IsDead)
                {
                    goldEarn = _globalGold * 2.5f;
                    if (_globalExp > 0)
                    {
                        player.Stats.Experience += _globalExp;
                    }
                }


                player.Stats.Gold += goldEarn;
                _game.PacketNotifier.NotifyAddGold(player, this, goldEarn);
            }

            _game.PacketNotifier.NotifyUnitAnnounceEvent(UnitAnnounces.TURRET_DESTROYED, this, killer);
            base.Die(killer);
        }

        /// <summary>
        /// Function called when this GameObject has been added to ObjectManager.
        /// </summary>
        public override void OnAdded()
        {
            base.OnAdded();
            _game.ObjectManager.AddTurret(this);
        }

        public override void OnCollision(IGameObject collider, bool isTerrain = false)
        {
            // TODO: Verify if we need this for things like SionR.
        }

        /// <summary>
        /// Overridden function unused by turrets.
        /// </summary>
        public override void RefreshWaypoints()
        {
        }

        /// <summary>
        /// Sets this turret's LaneID to the specified LaneID.
        /// Only sets if its current LaneID is NONE.
        /// Used for ObjectManager.
        /// </summary>
        /// <param name="newId"></param>
        public void SetLaneID(LaneID newId)
        {
            // Protect the current LaneID if it has already been set.
            if (Lane != LaneID.NONE)
            {
                return;
            }

            Lane = newId;
        }

        /// <summary>
        /// Called by ObjectManager every tick.
        /// </summary>
        /// <param name="diff">Amount of milliseconds passed since this tick started.</param>
        public override void Update(float diff)
        {
            if (!IsAttacking)
            {
                CheckForTargets();
            }

            // Lose focus of the unit target if the target is out of range
            if (TargetUnit != null && Vector2.DistanceSquared(Position, TargetUnit.Position) > Stats.Range.Total * Stats.Range.Total)
            {
                TargetUnit = null;
                _game.PacketNotifier.NotifySetTarget(this, null);
            }

            base.Update(diff);
            Replication.Update();
        }
    }
}
