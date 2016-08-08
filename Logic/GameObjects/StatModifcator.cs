using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class StatModifcator
    {
        public float BaseBonus { get; set; }
        public float PercentBaseBonus { get; set; }
        public float FlatBonus { get; set; }
        public float PercentBonus { get; set; }
        public bool StatModified => (BaseBonus != 0 || PercentBonus != 0 || FlatBonus != 0 || PercentBonus != 0);
    }
}
