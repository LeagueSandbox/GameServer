using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.API
{
    public enum CrowdControlType
    {
        Blind, Stun, Snare
    }

    public class UnitCrowdControl
    {
        CrowdControlType _type;
        float _duration;
        float _currentTime = 0;
        bool _remove = false;
        public UnitCrowdControl(CrowdControlType type, float duration = -1)
        {
            _type = type;
            _duration = duration;
        }
        public void Update(float diff)
        {
            _currentTime += diff / 1000.0f;
            if (_currentTime >= _duration && !_remove && _duration != -1)
            {
                _remove = true;
            }
        }
        public void SetForRemoval()
        {
            _remove = true;
        }
        public bool IsDead()
        {
            return _remove;
        }
        public bool IsTypeOf(CrowdControlType type)
        {
            return type == _type;
        }
    }
}
