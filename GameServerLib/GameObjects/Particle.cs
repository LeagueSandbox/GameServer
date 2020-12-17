﻿using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.Other;
using System.Numerics;

namespace LeagueSandbox.GameServer.GameObjects
{
    /// <summary>
    /// Class used for all in-game visual effects meant to be explicitly networked by the server (never spawned client-side).
    /// </summary>
    public class Particle : GameObject, IParticle
    {
        // Function Vars
        private float _currentTime;

        /// <summary>
        /// Object which spawned or caused the particle to be instanced
        /// </summary>
        public IGameObject Owner { get; }
        /// <summary>
        /// Client-sided, internal name of the particle used in networking, usually always ends in .troy
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Client-sided, internal name of the bone that this particle should be attached to for networking
        /// </summary>
        public string BoneName { get; }
        /// <summary>
        /// Scale of the particle used in networking
        /// </summary>
        public float Scale { get; }
        /// <summary>
        /// 3 dimensional forward vector (where the particle faces) used in networking
        /// </summary>
        public Vector3 Direction { get; }
        /// <summary>
        /// Total game-time that this particle should exist for
        /// </summary>
        public float Lifetime { get; }
        /// <summary>
        /// Whether or not the particle should be affected by vision,
        /// false = always visible,
        /// true = visibility can be obstructed
        /// </summary>
        public bool VisionAffected { get; }

        /// <summary>
        /// Prepares the Particle, setting up the information required for networking it to clients.
        /// </summary>
        /// <param name="game">Game instance.</param>
        /// <param name="owner">Owner of this Particle instance.</param>
        /// <param name="t">Target of this Particle instance (can be single position or GameObject).</param>
        /// <param name="particleName">Name used by League of Legends interally (ex: DebugCircle.troy).</param>
        /// <param name="scale">Scale of the Particle.</param>
        /// <param name="boneName">Name used by League of Legends internally where the Particle should be attached. Only useful when the target is a GameObject.</param>
        /// <param name="netId">NetID that should be forced onto the Particle. *NOTE*: Exceptions unhandled, expect crashes if NetID is already owned by a GameObject.</param>
        /// <param name="direction">3 dimensional vector representing the particle's orientation; unit vector forward.</param>
        /// <param name="lifetime">Number of seconds the Particle should exist.</param>
        /// <param name="reqVision">Whether or not the Particle is affected by vision checks.</param>
        /// <param name="autoSend">Whether or not to automatically send the Particle packet to clients.</param>
        public Particle(Game game, IGameObject owner, ITarget t, string particleName, float scale = 1.0f, string boneName = "", uint netId = 0, Vector3 direction = new Vector3(), float lifetime = 0, bool reqVision = true, bool autoSend = true)
               : base(game, t.X, t.Y, 0, 0, netId)
        {
            Owner = owner;
            Target = t;
            Name = particleName;
            BoneName = boneName;
            Scale = scale;
            Direction = direction;
            Lifetime = lifetime;
            VisionAffected = reqVision;

            _game.ObjectManager.AddObject(this);

            if (autoSend)
            {
                _game.PacketNotifier.NotifyFXCreateGroup(this);
            }
        }

        /// <summary>
        /// Called by ObjectManager every tick.
        /// </summary>
        /// <param name="diff">Number of milliseconds since this tick occurred.</param>
        public override void Update(float diff)
        {
            _currentTime += diff / 1000.0f;
            if (_currentTime >= Lifetime && !IsToRemove())
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
        public override void OnRemoved()
        {
            base.OnRemoved();
            _game.PacketNotifier.NotifyFXKill(this);
        }
    }
}
