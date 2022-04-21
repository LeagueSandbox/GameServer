using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using GameServerCore;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.Packets;

namespace LeagueSandbox.GameServer.GameObjects
{
    /// <summmary>
    /// Base class for all objects.
    /// GameObjects normally follow these guidelines of functionality: Position, Direction, Collision, Vision, Team, and Networking.
    /// </summmary>
    public class GameObject : IGameObject
    {
        // Crucial Vars (keep in mind Game is everywhere, which could be an issue for the future)
        protected Game _game;
        protected NetworkIdManager _networkIdManager;

        // Function Vars
        protected bool _toRemove;
        protected bool _movementUpdated;
        private Dictionary<TeamId, bool> _visibleByTeam;
        private HashSet<int> _spawnedForPlayers = new HashSet<int>();
        private Dictionary<int, bool> _visibleForPlayers = new Dictionary<int, bool>();
        /// <summary>
        /// A set of players with vision of this GameObject.
        /// Can be iterated through.
        /// </summary>
        public IEnumerable<int> VisibleForPlayers
        {
            get
            {
                foreach(var kv in _visibleForPlayers)
                {
                    if(kv.Value)
                    {
                        yield return kv.Key;
                    }
                }
            }
        }

        /// <summary>
        /// Comparison variable for small distance movements.
        /// </summary>
        public static readonly uint MOVEMENT_EPSILON = 5; //TODO: Verify if this should be changed

        /// <summary>
        ///  Identifier unique to this game object.
        /// </summary>
        public uint NetId { get; }
        /// <summary>
        /// Radius of the circle which is used for collision detection between objects or terrain.
        /// </summary>
        public float CollisionRadius { get; protected set; }
        /// <summary>
        /// Radius of the circle which is used for pathfinding around objects and terrain.
        /// </summary>
        public float PathfindingRadius { get; protected set; }
        /// <summary>
        /// Position of this GameObject from a top-down view.
        /// </summary>
        public Vector2 Position { get; protected set; }
        /// <summary>
        /// 3D orientation of this GameObject (based on ground-level).
        /// </summary>
        public Vector3 Direction { get; protected set; }
        /// <summary>
        /// Used to synchronize movement between client and server. Is currently assigned Env.TickCount.
        /// </summary>
        public int SyncId { get; }
        /// <summary>
        /// Team identifier, refer to TeamId enum.
        /// </summary>
        public TeamId Team { get; protected set; }
        /// <summary>
        /// Radius of the circle which is used for vision; detecting if objects are visible given terrain, and if so, networked to the player (or team) that owns this game object.
        /// </summary>
        public float VisionRadius { get; protected set; }

        /// <summary>
        /// Instantiation of an object which represents the base class for all objects in League of Legends.
        /// </summary>
        public GameObject(Game game, Vector2 position, float collisionRadius = 40f, float pathingRadius = 40f, float visionRadius = 0f, uint netId = 0, TeamId team = TeamId.TEAM_NEUTRAL)
        {
            _game = game;
            _networkIdManager = game.NetworkIdManager;
            if (netId != 0)
            {
                NetId = netId; // Custom netId
            }
            else
            {
                NetId = _networkIdManager.GetNewNetId(); // base class assigns a netId
            }
            Position = position;
            Direction = Vector3.Zero;
            SyncId = Environment.TickCount; // TODO: use movement manager to generate this
            CollisionRadius = collisionRadius;
            PathfindingRadius = pathingRadius;
            VisionRadius = visionRadius;

            _visibleByTeam = new Dictionary<TeamId, bool>();
            var teams = Enum.GetValues(typeof(TeamId)).Cast<TeamId>();
            foreach (var t in teams)
            {
                _visibleByTeam.Add(t, false);
            }

            Team = team;
            _movementUpdated = false;
            _toRemove = false;
        }

        /// <summary>
        /// Called by ObjectManager after AddObject (usually right after instatiation of GameObject).
        /// </summary>
        public virtual void OnAdded()
        {
            _game.Map.CollisionHandler.AddObject(this);
            _game.ObjectManager.AddVisionProvider(this, Team);
        }

        /// <summary>
        /// Called by ObjectManager every tick.
        /// </summary>
        /// <param name="diff">Number of milliseconds that passed before this tick occurred.</param>
        public virtual void Update(float diff)
        {
        }

        /// <summary>
        /// Whether or not the object should be removed from the game (usually both server and client-side). Refer to ObjectManager.
        /// </summary>
        public bool IsToRemove()
        {
            return _toRemove;
        }

        /// <summary>
        /// Will cause ObjectManager to remove the object (usually) both server and client-side next update.
        /// </summary>
        public virtual void SetToRemove()
        {
            _toRemove = true;
        }

        /// <summary>
        /// Called by ObjectManager after the object has been SetToRemove.
        /// </summary>
        public virtual void OnRemoved()
        {
            _game.Map.CollisionHandler.RemoveObject(this);
            _game.ObjectManager.RemoveVisionProvider(this, Team);
        }

        /// <summary>
        /// Refers to the height that the object is at in 3D space.
        /// </summary>
        public virtual float GetHeight()
        {
            return _game.Map.NavigationGrid.GetHeightAtLocation(Position.X, Position.Y);
        }

        /// <summary>
        /// Gets the position of this GameObject in 3D space, where the Y value represents height.
        /// Mostly used for packets.
        /// </summary>
        /// <returns>Vector3 position.</returns>
        public Vector3 GetPosition3D()
        {
            return new Vector3(Position.X, GetHeight(), Position.Y);
        }

        /// <summary>
        /// Sets the server-sided position of this object.
        /// </summary>
        public virtual void SetPosition(float x, float y)
        {
            SetPosition(new Vector2(x, y));
        }

        /// <summary>
        /// Sets the server-sided position of this object.
        /// </summary>
        public virtual void SetPosition(Vector2 vec)
        {
            Position = vec;
        }

        /// <summary>
        /// Sets the collision radius of this GameObject.
        /// </summary>
        /// <param name="newRadius">Radius to set.</param>
        public void SetCollisionRadius(float newRadius)
        {
            CollisionRadius = newRadius;
        }

        /// <summary>
        /// Sets this GameObject's current orientation (only X and Z are used in movement).
        /// </summary>
        public void FaceDirection(Vector3 newDirection, bool isInstant = true, float turnTime = 0.08333f)
        {
            if (newDirection == Vector3.Zero || float.IsNaN(newDirection.X) || float.IsNaN(newDirection.Y) || float.IsNaN(newDirection.Z))
            {
                return;
            }

            Direction = newDirection;
            if (_game.ObjectManager.GetObjectById(NetId) != null)
            {
                _game.PacketNotifier.NotifyFaceDirection(this, newDirection, isInstant, turnTime);
            }
        }

        /// <summary>
        /// Whether or not the specified object is colliding with this object.
        /// </summary>
        /// <param name="o">An object that could be colliding with this object.</param>
        public virtual bool IsCollidingWith(IGameObject o)
        {
            return Vector2.DistanceSquared(new Vector2(Position.X, Position.Y), o.Position) < (CollisionRadius + o.CollisionRadius) * (CollisionRadius + o.CollisionRadius);
        }

        /// <summary>
        /// Called by ObjectManager when the object is ontop of another object or when the object is inside terrain.
        /// </summary>
        public virtual void OnCollision(IGameObject collider, bool isTerrain = false)
        {
            // TODO: Verify if we should trigger events here.

            if (isTerrain)
            {
                // Escape functionality should be moved to GameObject.OnCollision.
                // only time we would collide with terrain is if we are inside of it, so we should teleport out of it.
                Vector2 exit = _game.Map.NavigationGrid.GetClosestTerrainExit(Position, PathfindingRadius + 1.0f);
                SetPosition(exit);
            }
        }

        /// <summary>
        /// Sets the object's team.
        /// </summary>
        /// <param name="team">TeamId.BLUE/PURPLE/NEUTRAL</param>
        public virtual void SetTeam(TeamId team)
        {
            _game.ObjectManager.RemoveVisionProvider(this, Team);
            Team = team;
            _game.ObjectManager.AddVisionProvider(this, Team);
            if (_game.IsRunning)
            {
                _game.PacketNotifier.NotifySetTeam(this as IAttackableUnit);
            }
        }

        /// <summary>
        /// Whether or not the object is within vision of the specified team.
        /// </summary>
        /// <param name="team">A team which could have vision of this object.</param>
        public bool IsVisibleByTeam(TeamId team)
        {
            return _visibleByTeam[team];
        }

        /// <summary>
        /// Sets the object as visible to a specified team.
        /// Should be called in the ObjectManager. By itself, it only affects the return value of IsVisibleByTeam.
        /// </summary>
        /// <param name="team">A team which could have vision of this object.</param>
        /// <param name="visible">New value.</param>
        public void SetVisibleByTeam(TeamId team, bool visible = true)
        {
            _visibleByTeam[team] = visible;
        }

        /// <summary>
        /// Whether or not the object is visible for the specified player.
        /// <summary>
        /// <param name="userId">The player in relation to which the value is obtained</param>
        public bool IsVisibleForPlayer(int userId)
        {
            return _visibleForPlayers.GetValueOrDefault(userId, false);
        }

        /// <summary>
        /// Sets the object as visible and or not to a specified player.
        /// Should be called in the ObjectManager. By itself, it only affects the return value of IsVisibleForPlayer.
        /// <summary>
        /// <param name="userId">The player for which the value is set.</param>
        /// <param name="visible">New value.</param>
        public void SetVisibleForPlayer(int userId, bool visible = true)
        {
            _visibleForPlayers[userId] = visible;
        }

        /// <summary>
        /// Whether or not the object is spawned on the player's client side.
        /// <summary>
        /// <param name="userId">The player in relation to which the value is obtained</param>
        public bool IsSpawnedForPlayer(int userId)
        {
            return _spawnedForPlayers.Contains(userId);
        }

        /// <summary>
        /// Sets the object as spawned on the player's client side.
        /// Should be called in the ObjectManager. By itself, it only affects the return value of IsSpawnedForPlayer.
        /// <summary>
        /// <param name="userId">The player for which the value is set.</param>
        public void SetSpawnedForPlayer(int userId)
        {
            _spawnedForPlayers.Add(userId);
        }

        /// <summary>
        /// Sets the position of this GameObject to the specified position.
        /// </summary>
        /// <param name="x">X coordinate to set.</param>
        /// <param name="y">Y coordinate to set.</param>
        public virtual void TeleportTo(float x, float y)
        {
            var position = _game.Map.NavigationGrid.GetClosestTerrainExit(new Vector2(x, y), PathfindingRadius + 1.0f);
            
            SetPosition(position);

            // TODO: Find a suitable function for this. Maybe modify NotifyWaypointGroup to accept simple objects. 
            _game.PacketNotifier.NotifyEnterVisibilityClient(this);
            _movementUpdated = false;
        }

        /// <summary>
        /// Forces this GameObject to perform the given internally named animation.
        /// </summary>
        /// <param name="animName">Internal name of an animation to play.</param>
        /// <param name="timeScale">How fast the animation should play. Default 1x speed.</param>
        /// <param name="startTime">Time in the animation to start at.</param>
        /// TODO: Verify if this description is correct, if not, correct it.
        /// <param name="speedScale">How much the speed of the GameObject should affect the animation.</param>
        /// <param name="flags">Animation flags. Refer to AnimationFlags enum.</param>
        public void PlayAnimation(string animName, float timeScale = 1.0f, float startTime = 0, float speedScale = 0, AnimationFlags flags = 0)
        {
            _game.PacketNotifier.NotifyS2C_PlayAnimation(this, animName, flags, timeScale, startTime, speedScale);
        }

        /// <summary>
        /// Forces this GameObject's current animations to pause/unpause.
        /// </summary>
        /// <param name="pause">Whether or not to pause/unpause animations.</param>
        public void PauseAnimation(bool pause)
        {
            _game.PacketNotifier.NotifyS2C_PauseAnimation(this, pause);
        }

        /// <summary>
        /// Forces this GameObject to stop playing the specified animation (or optionally all animations) with the given parameters.
        /// </summary>
        /// <param name="animation">Internal name of the animation to stop playing. Set blank/null if stopAll is true.</param>
        /// <param name="stopAll">Whether or not to stop all animations. Only works if animation is empty/null.</param>
        /// <param name="fade">Whether or not the animation should fade before stopping.</param>
        /// <param name="ignoreLock">Whether or not locked animations should still be stopped.</param>
        public void StopAnimation(string animation, bool stopAll = false, bool fade = false, bool ignoreLock = true)
        {
            _game.PacketNotifier.NotifyS2C_StopAnimation(this, animation, stopAll, fade, ignoreLock);
        }
    }
}
