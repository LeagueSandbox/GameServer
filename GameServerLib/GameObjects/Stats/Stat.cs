using GameServerCore.Domain.GameObjects;

namespace LeagueSandbox.GameServer.GameObjects.Stats
{
    public class Stat : IStat
    {
        private bool _modified;
        private float _baseValue;
        private float _baseBonus;
        private float _flatBonus;
        private float _percentBonus;
        private float _percentBaseBonus;

        public bool Modified
        {
            get { return _modified; }
            private set { _modified = value; }
        }

        public float BaseBonus
        {
            get => _baseBonus;
            set
            {
                Modified = true;
                _baseBonus = value;
            }
        }

        public float FlatBonus
        {
            get => _flatBonus;
            set
            {
                Modified = true; _flatBonus = value;
            }
        }

        public float BaseValue
        {
            get => _baseValue;
            set
            {
                Modified = true;
                _baseValue = value;
            }
        }

        public float PercentBonus
        {
            get => _percentBonus;
            set
            {
                Modified = true;
                _percentBonus = value;
            }
        }

        public float PercentBaseBonus
        {
            get => _percentBaseBonus;
            set
            {
                Modified = true;
                _percentBaseBonus = value;
            }
        }

        public float Total => ((BaseValue + BaseBonus) * (1 + PercentBaseBonus) + FlatBonus) * (1 + PercentBonus);

        public Stat(float baseValue, float baseBonus, float percentBaseBonus, float flatBonus, float percentBonus)
        {
            _baseValue = baseValue;
            _baseBonus = baseBonus;
            _percentBaseBonus = percentBaseBonus;
            _flatBonus = flatBonus;
            _percentBonus = percentBonus;
            Modified = false;
        }

        public Stat() : this(0, 0, 0, 0, 0)
        {

        }

        public bool ApplyStatModificator(IStatModifier statModifcator)
        {
            if (!statModifcator.StatModified)
            {
                return false;
            }
            BaseBonus += statModifcator.BaseBonus;
            PercentBaseBonus += statModifcator.PercentBaseBonus;
            FlatBonus += statModifcator.FlatBonus;
            PercentBonus += statModifcator.PercentBonus;

            return true;
        }

        public bool RemoveStatModificator(IStatModifier statModifcator)
        {
            if (!statModifcator.StatModified)
            {
                return false;
            }
            BaseBonus -= statModifcator.BaseBonus;
            PercentBaseBonus -= statModifcator.PercentBaseBonus;
            FlatBonus -= statModifcator.FlatBonus;
            PercentBonus -= statModifcator.PercentBonus;

            return true;
        }
    }
}
