using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class Stat
    {
        private bool _modified;
        private float _baseValue;
        private float _baseBonus;
        private float _flatBonus;
        private float _percentBonus;
        private float _percentBaseBonus;

        public bool Modified
        {
            get
            {
                if (_modified)
                {
                    _modified = false;
                }
                else
                {
                    return false;
                }
                return true;
            }
        }

        public float BaseBonus { get { return _baseBonus; } set { _modified = true; _baseBonus = value; } }
        public float FlatBonus { get { return _flatBonus; } set { _modified = true; _flatBonus = value; } }
        public float BaseValue { get { return _baseValue; } set { _modified = true; _baseValue = value; } }
        public float PercentBonus { get { return _percentBonus; } set { _modified = true; _percentBonus = value; } }
        public float PercentBaseBonus { get { return _percentBaseBonus; } set { _modified = true; _percentBaseBonus = value; } }
        
        public float Total { get { return ((BaseValue + BaseBonus) * (1+PercentBaseBonus) + FlatBonus)*(1+PercentBonus); } }

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
