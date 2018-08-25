using System;

namespace LeagueSandbox.GameServer.Scripting.CSharp
{
    //Timer class for GameScripts to be able to trigger events after a duration
    public class GameScriptTimer
    {
        private float _duration;
        private float _currentTime;
        private Action _callback;
        private bool _remove;
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

        public double GetPercentageFinished()
        {
            return _currentTime / _duration * 100.0;
        }

        public float GetCurrentTime()
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
