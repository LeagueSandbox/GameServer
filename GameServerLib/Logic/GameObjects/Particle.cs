using GameServerCore.Logic.Domain.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Logic.GameObjects.Other;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class Particle : GameObject, IParticle
    {
        public Champion Owner { get; private set; }
        public string Name { get; private set; }
        public string BoneName { get; private set; }
        public float Size { get; private set; }

        IChampion IParticle.Owner => Owner;

        public Particle(Game game, Champion owner, Target t, string particleName, float size = 1.0f, string boneName = "", uint netId = 0)
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
