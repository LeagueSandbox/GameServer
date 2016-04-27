using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class Stat
    {
        public float ModifiedBase { get; set; }
        public float Bonus { get; set; }
        public float Total { get { return BaseValue + ModifiedBase + Bonus; } }
        public float BaseValue { get; set; }

        public Stat(float baseValue, float modifierBase, float bonus)
        {
            BaseValue = baseValue;
            ModifiedBase = modifierBase;
            Bonus = bonus;
        }

        public Stat() : this(0, 0, 0)
        {
            
        }
   }
}
