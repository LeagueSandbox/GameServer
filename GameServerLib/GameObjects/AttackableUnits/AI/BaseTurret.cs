using System.Numerics;
using System.Text;
using Force.Crc32;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeaguePackets.Game.Events;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.Inventory;

namespace LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI
{
    /// <summary>
    /// Base class for Turret GameObjects.
    /// In League, turrets are separated into visual and AI objects, so this GameObject represents the AI portion,
    /// while the visual object is handled automatically by clients via packets.
    /// </summary>
    public class BaseTurret : ObjAiBase, IBaseTurret
    {
        /// <summary>
        /// Current lane this turret belongs to.
        /// </summary>
        public LaneID Lane { get; private set; }
        /// <summary>
        /// MapObject that this turret was created from.
        /// </summary>
        public MapObject ParentObject { get; private set; }
        /// <summary>
        /// Internal name of this turret, used for packets so that clients know which visual turret to assign them to.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Supposed to be the NetID for the visual counterpart of this turret. Used only for packets.
        /// </summary>
        public uint ParentNetId { get; private set; }
        /// <summary>
        /// Region assigned to this turret for vision and collision.
        /// </summary>
        public IRegion BubbleRegion { get; private set; }

        public BaseTurret(
            Game game,
            string name,
            string model,
            Vector2 position,
            TeamId team = TeamId.TEAM_BLUE,
            uint netId = 0,
            LaneID lane = LaneID.NONE,
            MapObject mapObject = default,
            int skinId = 0,
            string aiScript = ""
        ) : base(game, model, new Stats.Stats(), position: position, visionRadius: 800, skinId: skinId, netId: netId, team: team, aiScript: aiScript)
        {
            ParentNetId = Crc32Algorithm.Compute(Encoding.UTF8.GetBytes(name)) | 0xFF000000;
            Name = name;
            Lane = lane;
            ParentObject = mapObject;
            SetTeam(team);
            Inventory = InventoryManager.CreateInventory(game.PacketNotifier);
            Replication = new ReplicationAiTurret(this);
        }



        /// <summary>
        /// Called when this unit dies.
        /// </summary>
        /// <param name="killer">Unit that killed this unit.</param>
        public override void Die(IDeathData data)
        {
            var announce = new OnTurretDie
            {
                AssistCount = 0,
                GoldGiven = 0.0f,
                OtherNetID = data.Killer.NetId
            };
            _game.PacketNotifier.NotifyS2C_OnEventWorld(announce, NetId);

            base.Die(data);
        }

        /// <summary>
        /// Function called when this GameObject has been added to ObjectManager.
        /// </summary>
        public override void OnAdded()
        {
            base.OnAdded();
            _game.ObjectManager.AddTurret(this);

            // TODO: Handle this via map script for LaneTurret and via CharScript for AzirTurret.
            BubbleRegion = new Region(_game, Team, Position, RegionType.Default, this, this, true, 800f, true, true, PathfindingRadius, lifetime: 25000.0f);
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
    }
}
