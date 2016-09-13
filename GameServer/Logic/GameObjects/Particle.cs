using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class Particle : GameObject
    {
        public Champion Owner { get; private set; }
        public string Name { get; private set; }
        public string BoneName { get; private set; }
        public float Size { get; private set; }

        public Particle(Champion owner, Target t, string particleName, float size = 1.0f, string boneName = "", uint netId = 0)
               : base(t.X, t.Y, 0, 0, netId)
        {
            this.Owner = owner;
            this.Target = t;
            this.Name = particleName;
            this.BoneName = boneName;
            this.Size = size;
        }
    }
}
