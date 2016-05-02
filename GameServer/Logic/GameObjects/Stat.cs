using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class Stat
    {
        public float BaseBonus { get; set; }
        public float PercentBaseBonus { get; set; }
        public float FlatBonus { get; set; }
        public float PercentBonus { get; set; }
        public float Total { get { return ((BaseValue + BaseBonus) * (1+PercentBaseBonus) + FlatBonus)*(1+PercentBonus); } }
        public float BaseValue { get; set; }

        public Stat(float baseValue, float baseBonus, float percentBaseBonus, float flatBonus, float percentBonus)
        {
            BaseValue = baseValue;
            BaseBonus = baseBonus;
            PercentBaseBonus = percentBaseBonus;
            FlatBonus = flatBonus;
            PercentBonus = percentBonus;
        }

        public Stat() : this(0, 0, 0, 0, 0)
        {
            
        }

        public bool ApplyStatModificator(StatModifcator statModifcator)
        {
            if (!statModifcator.StatModified)
                return false;
            BaseBonus += statModifcator.BaseBonus;
            PercentBaseBonus += statModifcator.PercentBaseBonus;
            FlatBonus += statModifcator.FlatBonus;
            PercentBonus += statModifcator.PercentBonus;
            return true;
        }

        public bool RemoveStatModificator(StatModifcator statModifcator)
        {
            if (!statModifcator.StatModified)
                return false;
            BaseBonus -= statModifcator.BaseBonus;
            PercentBaseBonus -= statModifcator.PercentBaseBonus;
            FlatBonus -= statModifcator.FlatBonus;
            PercentBonus -= statModifcator.PercentBonus;
            return true;
        }
   }
}
