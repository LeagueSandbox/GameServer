using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.Other;
using System.Numerics;

namespace LeagueSandbox.GameServer.GameObjects
{
    /// <summary>
    /// Class used for all in-game visual effects meant to be explicitly networked by the server (never spawned client-side).
    /// </summary>
    /// TODO: Possibly turn this into a simple data storage object and put it in GameObject as a RegionParameters variable (or something similar).
    public class Region : GameObject, IRegion
    {
        // Function Vars
        private float _currentTime;

        public int Type { get; }
        public IGameObject CollisionUnit { get; }
        public int OwnerClientID { get; }
        public uint VisionNetID { get; }
        public uint VisionBindNetID { get; }
        /// <summary>
        /// Total game-time that this region should exist for
        /// </summary>
        public float Lifetime { get; }
        public float GrassRadius { get; }
        /// <summary>
        /// Scale of the region used in networking
        /// </summary>
        public float Scale { get; }
        public float AdditionalSize { get; }
        public bool HasCollision { get; }
        public bool GrantVision { get; }
        public bool RevealsStealth { get; }

        /// <summary>
        /// Prepares the Particle, setting up the information required for networking it to clients.
        /// This particle will spawn and stay on the specified GameObject target.
        /// </summary>
        /// <param name="game">Game instance.</param>
        /// <param name="team">Team this region should be on.</param>
        /// <param name="pos">Position to spawn this region at.</param>
        /// <param name="type">Type of region to spawn. Values unknown. Default recommended.</param>
        /// <param name="collisionUnit">Unit which will use the collision radius of this region.</param>
        /// <param name="visionTarget">Bind target for vision.</param>
        /// <param name="giveVision">Whether or not this region should have vision.</param>
        /// <param name="visionRadius">Size of the vision area of this region.</param>
        /// <param name="revealStealth">Whether or not this region should reveal stealthed units when they within the vision radius.</param>
        /// <param name="hasCollision">Whether or not this region should have collision.</param>
        /// <param name="collisionRadius">Size of the collision area of this region.</param>
        /// <param name="grassRadius">Size of the grass area of this region. Currently non-functional.</param>
        /// <param name="scale">Scale of the Region.</param>
        /// <param name="addedSize"></param>
        /// <param name="lifetime">Number of seconds the Region should exist.</param>
        /// <param name="clientId">ClientID of the player that owns this region.</param>
        public Region
        (
            Game game,
            TeamId team,
            Vector2 pos,
            RegionType type = RegionType.Default,
            IGameObject collisionUnit = null,
            IGameObject visionTarget = null,
            bool giveVision = false,
            float visionRadius = 0,
            bool revealStealth = false,
            bool hasCollision = false,
            float collisionRadius = 0,
            float grassRadius = 0,
            float scale = 1.0f,
            float addedSize = 0,
            float lifetime = 0,
            int clientId = 0
        ): base(game, pos, 0, collisionRadius, visionRadius, team: team)
        {
            Type = (int)type;
            CollisionUnit = collisionUnit;
            OwnerClientID = clientId;
            VisionNetID = _game.NetworkIdManager.GetNewNetId();
            if (visionTarget != null)
            {
                VisionBindNetID = visionTarget.NetId;
            }

            Lifetime = lifetime;
            GrassRadius = grassRadius;
            Scale = scale;
            AdditionalSize = addedSize;
            HasCollision = hasCollision;
            GrantVision = giveVision;

            if (!GrantVision)
            {
                VisionRadius = 0;
            }

            if (Scale > 0)
            {
                PathfindingRadius *= Scale;
                VisionRadius *= Scale;
            }

            if (AdditionalSize > 0)
            {
                PathfindingRadius += AdditionalSize;
                VisionRadius += AdditionalSize;
            }

            RevealsStealth = revealStealth;

            _game.ObjectManager.AddObject(this);
        }

        public override void OnAdded()
        {
            if (HasCollision)
            {
                _game.Map.CollisionHandler.AddObject(this);
            }
            _game.ObjectManager.AddVisionProvider(this, Team);
            RegisterVision();
        }

        public override void OnRemoved()
        {
            if (HasCollision)
            {
                _game.Map.CollisionHandler.RemoveObject(this);
            }
            _game.ObjectManager.RemoveVisionProvider(this, Team);
            UnregisterVision();
        }

        public override void SetTeam(TeamId team)
        {
            UnregisterVision();
            base.SetTeam(team);
            RegisterVision();
        }

        /// <summary>
        /// Additionally registers vision for both teams, if necessary.
        /// </summary>
        void RegisterVision()
        {
            // NEUTRAL Regions give global vision.
            if (Team == TeamId.TEAM_NEUTRAL)
            {
                _game.ObjectManager.AddVisionProvider(this, TeamId.TEAM_BLUE);
                _game.ObjectManager.AddVisionProvider(this, TeamId.TEAM_PURPLE);
            }
        }

        void UnregisterVision()
        {
            if (Team == TeamId.TEAM_NEUTRAL)
            {
                _game.ObjectManager.RemoveVisionProvider(this, TeamId.TEAM_BLUE);
                _game.ObjectManager.RemoveVisionProvider(this, TeamId.TEAM_PURPLE);
            }
        }

        /// <summary>
        /// Called by ObjectManager when the object is ontop of another object or when the object is inside terrain.
        /// </summary>
        public override void OnCollision(IGameObject collider, bool isTerrain = false)
        {
        }

        /// <summary>
        /// Called by ObjectManager every tick.
        /// </summary>
        /// <param name="diff">Number of milliseconds since this tick occurred.</param>
        public override void Update(float diff)
        {
            if (Lifetime == -1f)
            {
                return;
            }

            _currentTime += diff / 1000.0f;
            if (_currentTime >= Lifetime)
            {
                SetToRemove();
            }
        }

        /// <summary>
        /// Returns the total game-time passed since the particle was added to ObjectManager
        /// </summary>
        public float GetTimeAlive()
        {
            return _currentTime;
        }

        /// <summary>
        /// Actions that should be performed after the Particle is removed from ObjectManager.
        /// </summary>
        public override void SetToRemove()
        {
            if (!IsToRemove())
            {
                base.SetToRemove();
                _game.PacketNotifier.NotifyRemoveRegion(this);
            }
        }
    }
}