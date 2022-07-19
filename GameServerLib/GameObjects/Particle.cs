using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.Other;
using System.Numerics;

namespace LeagueSandbox.GameServer.GameObjects
{
    /// <summary>
    /// Class used for all in-game visual effects meant to be explicitly networked by the server (never spawned client-side).
    /// </summary>
    public class Particle : GameObject
    {
        // Function Vars
        private float _currentTime;

        /// <summary>
        /// Creator of this particle.
        /// </summary>
        public GameObject Caster { get; }
        /// <summary>
        /// Primary bind target.
        /// </summary>
        public GameObject BindObject { get; }
        /// <summary>
        /// Client-sided, internal name of the particle used in networking, usually always ends in .troy
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Secondary bind target. Null when not attached to anything.
        /// </summary>
        public GameObject TargetObject { get; }
        /// <summary>
        /// Position this object is spawned at.
        /// </summary>
        public Vector2 StartPosition { get; private set; }
        /// <summary>
        /// Position this object is aimed at and/or moving towards.
        /// </summary>
        public Vector2 EndPosition { get; private set; }
        /// <summary>
        /// Client-sided, internal name of the bone that this particle should be attached to on the owner, for networking.
        /// </summary>
        public string BoneName { get; }
        /// <summary>
        /// Client-sided, internal name of the bone that this particle should be attached to on the target, for networking.
        /// </summary>
        public string TargetBoneName { get; }
        /// <summary>
        /// Scale of the particle used in networking
        /// </summary>
        public float Scale { get; }
        /// <summary>
        /// Total game-time that this particle should exist for
        /// </summary>
        public float Lifetime { get; }
        /// <summary>
        /// The only team that should be able to see this particle.
        /// </summary>
        public TeamId SpecificTeam { get; }
        /// <summary>
        /// The only unit that should be able to see this particle.
        /// Only effective if this is a player controlled unit.
        /// </summary>
        public GameObject SpecificUnit { get; }
        /// <summary>
        /// Whether or not the particle should be titled along the ground towards its end position.
        /// Effectively uses the ground height for the end position.
        /// </summary>
        public bool FollowsGroundTilt { get; }
        /// <summary>
        /// Flags which determine how the particle behaves. Values unknown.
        /// </summary>
        public FXFlags Flags { get; }

        public override bool IsAffectedByFoW => true;
        public override bool SpawnShouldBeHidden => true;

        /// <summary>
        /// Prepares the Particle, setting up the information required for networking it to clients.
        /// This particle will spawn and stay on the specified GameObject target.
        /// </summary>
        /// <param name="game">Game instance.</param>
        /// <param name="caster">GameObject that caused this particle to spawn.</param>
        /// <param name="bindObj">Primary bind target.</param>
        /// <param name="target">Secondary bind target.</param>
        /// <param name="particleName">Name used by League of Legends interally (ex: DebugCircle.troy).</param>
        /// <param name="scale">Scale of the Particle.</param>
        /// <param name="boneName">Name used by League of Legends internally where the Particle should be attached. Only useful when the target is a GameObject.</param>
        /// <param name="targetBoneName">Bone of the target to attach to.</param>
        /// <param name="netId">NetID that should be forced onto the Particle. *NOTE*: Exceptions unhandled, expect crashes if NetID is already owned by a GameObject.</param>
        /// <param name="direction">3 dimensional vector representing the particle's orientation; unit vector forward.</param>
        /// <param name="followGroundTilt">Whether or not the particle should be titled along the ground towards its end position.</param>
        /// <param name="lifetime">Number of seconds the Particle should exist.</param>
        /// <param name="teamOnly">The only team that should be able to see this particle.</param>
        /// <param name="flags">Flags which determine how the particle behaves. Refer to FXFlags enum.</param>
        public Particle(Game game, GameObject caster, GameObject bindObj, GameObject target, string particleName, float scale = 1.0f, string boneName = "", string targetBoneName = "", uint netId = 0, Vector3 direction = new Vector3(), bool followGroundTilt = false, float lifetime = 0, TeamId teamOnly = TeamId.TEAM_NEUTRAL, GameObject unitOnly = null, FXFlags flags = FXFlags.GivenDirection)
               : base(game, target.Position, 0, 0, 0, netId, teamOnly)
        {
            Caster = caster;
            BindObject = bindObj;
            TargetObject = target;
            StartPosition = TargetObject.Position;
            BoneName = boneName;
            TargetBoneName = targetBoneName;
            Scale = scale;
            Direction = direction;
            Lifetime = lifetime;
            SpecificTeam = teamOnly;
            SpecificUnit = unitOnly;
            FollowsGroundTilt = followGroundTilt;
            Flags = flags;

            if (bindObj != null)
            {
                Team = bindObj.Team;
            }
            else if (caster != null)
            {
                Team = caster.Team;
            }

            if (particleName.Contains(".troy"))
            {
                Name = particleName;
            }
            else
            {
                Name = $"{particleName}.troy";
            }

            _game.ObjectManager.AddObject(this);
        }

        /// <summary>
        /// Prepares the Particle, setting up the information required for networking it to clients.
        /// This particle will spawn and stay at the specified position.
        /// </summary>
        /// <param name="game">Game instance.</param>
        /// <param name="caster">GameObject that caused this particle to spawn.</param>
        /// <param name="bindObj">GameObject that the particle should be bound to.</param>
        /// <param name="targetPos">Target position of this particle.</param>
        /// <param name="particleName">Name used by League of Legends interally (ex: DebugCircle.troy).</param>
        /// <param name="scale">Scale of the Particle.</param>
        /// <param name="boneName">Name used by League of Legends internally where the Particle should be attached. Only useful when the target is a GameObject.</param>
        /// <param name="targetBoneName">Bone of the target to attach to.</param>
        /// <param name="netId">NetID that should be forced onto the Particle. *NOTE*: Exceptions unhandled, expect crashes if NetID is already owned by a GameObject.</param>
        /// <param name="direction">3 dimensional vector representing the particle's orientation; unit vector forward.</param>
        /// <param name="followGroundTilt">Whether or not the particle should be titled along the ground towards its end position.</param>
        /// <param name="lifetime">Number of seconds the Particle should exist.</param>
        /// <param name="teamOnly">The only team that should be able to see this particle.</param>
        /// <param name="flags">Flags which determine how the particle behaves. Refer to FXFlags enum.</param>
        public Particle(Game game, GameObject caster, GameObject bindObj, Vector2 targetPos, string particleName, float scale = 1.0f, string boneName = "", string targetBoneName = "", uint netId = 0, Vector3 direction = new Vector3(), bool followGroundTilt = false, float lifetime = 0, TeamId teamOnly = TeamId.TEAM_NEUTRAL, GameObject unitOnly = null, FXFlags flags = FXFlags.GivenDirection)
               : base(game, targetPos, 0, 0, 0, netId, teamOnly)
        {
            Caster = caster;

            BindObject = bindObj;
            if (BindObject != null)
            {
                Position = BindObject.Position;
            }

            TargetObject = null;
            StartPosition = targetPos;
            BoneName = boneName;
            TargetBoneName = targetBoneName;
            Scale = scale;
            Direction = direction;
            Lifetime = lifetime;
            SpecificTeam = teamOnly;
            SpecificUnit = unitOnly;
            FollowsGroundTilt = followGroundTilt;
            Flags = flags;

            if (bindObj != null)
            {
                Team = bindObj.Team;
            }
            else if (caster != null)
            {
                Team = caster.Team;
            }

            if (particleName.Contains(".troy"))
            {
                Name = particleName;
            }
            else
            {
                Name = $"{particleName}.troy";
            }

            _game.ObjectManager.AddObject(this);
        }

        /// <summary>
        /// Prepares the Particle, setting up the information required for networking it to clients.
        /// This particle will spawn and stay at the specified position.
        /// </summary>
        /// <param name="game">Game instance.</param>
        /// <param name="caster">GameObject that caused this particle to spawn.</param>
        /// <param name="startPos">Position the particle will spawn at.</param>
        /// <param name="endPos">Position the particle will end at.</param>
        /// <param name="particleName">Name used by League of Legends interally (ex: DebugCircle.troy).</param>
        /// <param name="scale">Scale of the Particle.</param>
        /// <param name="boneName">Name used by League of Legends internally where the Particle should be attached. Only useful when the target is a GameObject.</param>
        /// <param name="targetBoneName">Bone of the target to attach to.</param>
        /// <param name="netId">NetID that should be forced onto the Particle. *NOTE*: Exceptions unhandled, expect crashes if NetID is already owned by a GameObject.</param>
        /// <param name="direction">3 dimensional vector representing the particle's orientation; unit vector forward.</param>
        /// <param name="followGroundTilt">Whether or not the particle should be titled along the ground towards its end position.</param>
        /// <param name="lifetime">Number of seconds the Particle should exist.</param>
        /// <param name="teamOnly">The only team that should be able to see this particle.</param>
        /// <param name="flags">Flags which determine how the particle behaves. Refer to FXFlags enum.</param>
        public Particle(Game game, GameObject caster, Vector2 startPos, Vector2 endPos, string particleName, float scale = 1.0f, string boneName = "", string targetBoneName = "", uint netId = 0, Vector3 direction = new Vector3(), bool followGroundTilt = false, float lifetime = 0, TeamId teamOnly = TeamId.TEAM_NEUTRAL, GameObject unitOnly = null, FXFlags flags = FXFlags.GivenDirection)
               : base(game, startPos, 0, 0, 0, netId, teamOnly)
        {
            Caster = caster;

            BindObject = null;
            TargetObject = null;
            StartPosition = startPos;
            EndPosition = endPos;
            BoneName = boneName;
            TargetBoneName = targetBoneName;
            Scale = scale;
            Direction = direction;
            Lifetime = lifetime;
            SpecificTeam = teamOnly;
            SpecificUnit = unitOnly;
            FollowsGroundTilt = followGroundTilt;
            Flags = flags;

            if (caster != null)
            {
                Team = caster.Team;
            }

            if (particleName.Contains(".troy"))
            {
                Name = particleName;
            }
            else
            {
                Name = $"{particleName}.troy";
            }

            _game.ObjectManager.AddObject(this);
        }

        /// <summary>
        /// Called by ObjectManager every tick.
        /// </summary>
        /// <param name="diff">Number of milliseconds since this tick occurred.</param>
        public override void Update(float diff)
        {
            _currentTime += diff / 1000.0f;
            if (_currentTime >= Lifetime && Lifetime >= 0)
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
                _game.PacketNotifier.NotifyFXKill(this);
            }
        }
    }
}
