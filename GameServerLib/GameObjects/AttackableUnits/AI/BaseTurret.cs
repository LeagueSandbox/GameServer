using System.Numerics;
using System.Text;
using Force.Crc32;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
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
        /// MapObject that this turret was created from.
        /// </summary>
        public IMapObject ParentObject { get; private set; }
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
            LaneID lane = LaneID.NONE,
            IMapObject mapObject = null,
            int skinId = 0,
            string aiScript = ""
        ) : base(game, model, new Stats.Stats(), 88, position, 1200, skinId, netId, team, aiScript)
        {
            ParentNetId = Crc32Algorithm.Compute(Encoding.UTF8.GetBytes(name)) | 0xFF000000;
            Name = name;
            Lane = lane;
            ParentObject = mapObject;
            SetTeam(team);
            Inventory = InventoryManager.CreateInventory(game.PacketNotifier, game.ScriptEngine);
            Replication = new ReplicationAiTurret(this);
        }



        /// <summary>
        /// Called when this unit dies.
        /// </summary>
        /// <param name="killer">Unit that killed this unit.</param>
        public override void Die(IDeathData data)
        {
            foreach (var player in _game.ObjectManager.GetAllChampionsFromTeam(data.Killer.Team))
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
                _game.PacketNotifier.NotifyUnitAddGold(player, this, goldEarn);
            }

            _game.PacketNotifier.NotifyUnitAnnounceEvent(UnitAnnounces.TURRET_DESTROYED, this, data.Killer);
            base.Die(data);
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
        public override void RefreshWaypoints(float idealRange)
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
            Lane = newId;
        }

        /// <summary>
        /// Called by ObjectManager every tick.
        /// </summary>
        /// <param name="diff">Amount of milliseconds passed since this tick started.</param>
        public override void Update(float diff)
        {
            base.Update(diff);
        }
    }
}
