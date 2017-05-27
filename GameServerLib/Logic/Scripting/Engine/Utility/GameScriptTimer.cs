﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Scripting.CSharp
{
    //Timer class for GameScripts to be able to trigger events after a duration
    public class GameScriptTimer
    {
        float _duration = 0;
        float _currentTime = 0;
        Action _callback = null;
        bool _remove = false;
        public GameScriptTimer(float duration, Action callback)
        {
            _duration = duration;
            _callback = callback;
        }
        public void SetCallback(Action callback)
        {
            _callback = callback;
        }
        public void Update(float diff)
        {
            _currentTime += diff / 1000.0f;
            if (_currentTime >= _duration && !_remove)
            {
                _callback();
                _remove = true;
            }
        }

        public double getPercentageFinished()
        {
            return _currentTime / _duration * 100.0;
        }

        public float getCurrentTime()
        {
            return _currentTime;
        }

        public void SetTimeTo(float time)
        {
            _currentTime = time;
        }
        public void EndTimerNow()
        {
            _currentTime = _duration;
            _remove = true;
            _callback();
        }
        public void EndTimerWithoutCallback()
        {
            _currentTime = _duration;
            _remove = true;
        }
        public bool IsDead()
        {
            return _remove;
        }
    }
}
