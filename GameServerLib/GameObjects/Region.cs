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
        /// <param name="collisionUnit">Unit which will use the collision radius of this region.</param>
        /// <param name="visionTarget">Bind target for vision.</param>
        /// <param name="scale">Scale of the Region.</param>
        /// <param name="netId">NetID that should be forced onto the Region. *NOTE*: Exceptions unhandled, expect crashes if NetID is already owned by a GameObject.</param>
        /// <param name="lifetime">Number of seconds the Region should exist.</param>
        public Region(Game game, TeamId team, Vector2 pos, RegionType type = RegionType.Default, IGameObject collisionUnit = null, IGameObject visionTarget = null, bool giveVision = false, float visionRadius = 0, bool revealStealth = false, bool hasCollision = false, float collisionRadius = 0, float grassRadius = 0, float scale = 1.0f, float addedSize = 0, float lifetime = 0, int clientId = 0)
               : base(game, pos, collisionRadius, visionRadius, team: team)
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

            RevealsStealth = revealStealth;

            _game.ObjectManager.AddObject(this);
        }

        public override void OnAdded()
        {
            if (HasCollision)
            {
                _game.Map.CollisionHandler.AddObject(this);
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