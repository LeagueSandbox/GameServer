using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.Other;

namespace LeagueSandbox.GameServer.GameObjects
{
    public class Particle : GameObject, IParticle
    {
        public IChampion Owner { get; }
        public string Name { get; }
        public string BoneName { get; }
        public float Size { get; }

        public Particle(Game game, IChampion owner, ITarget t, string particleName, float size = 1.0f, string boneName = "", uint netId = 0)
               : base(game, t.X, t.Y, 0, 0, netId)
        {
            Owner = owner;
            Target = t;
            Name = particleName;
            BoneName = boneName;
            Size = size;
        }
    }
}
