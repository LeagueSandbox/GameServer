using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.Other;
using System.Numerics;

namespace LeagueSandbox.GameServer.GameObjects
{
    public class Particle : GameObject, IParticle
    {
        private float _currentTime;

        public IGameObject Owner { get; }
        public string Name { get; }
        public string BoneName { get; }
        public float Scale { get; }
        public Vector3 Direction { get; }
        public float Lifetime { get; }
        public bool VisionAffected { get; }

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

        public override void Update(float diff)
        {
            _currentTime += diff / 1000.0f;
            if (_currentTime >= Lifetime && !IsToRemove())
            {
                SetToRemove();
            }
        }

        public float GetTimeAlive()
        {
            return _currentTime;
        }

        public override void OnRemoved()
        {
            base.OnRemoved();
            _game.PacketNotifier.NotifyFXKill(this);
        }
    }
}
