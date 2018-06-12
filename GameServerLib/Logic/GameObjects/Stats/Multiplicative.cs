using System.Collections.Generic;

namespace LeagueSandbox.GameServer.Logic.GameObjects.Stats
{
    public class Multiplicative
    {
        private List<float> _modifiers = new List<float>();
        public float TotalMultiplier { get; private set; } = 1;

        private void Recalculate()
        {
            var total = 1f;
            foreach (var mod in _modifiers)
            {
                total *= 1 + mod;
            }

            TotalMultiplier = total;
        }

        public void Add(float percent)
        {
            TotalMultiplier *= 1 + percent;
            _modifiers.Add(percent);
        }

        // can't divide directly since divisor might be 0
        public void Remove(float percent)
        {
            _modifiers.Remove(percent);
            Recalculate();
        }

        public static implicit operator float(Multiplicative mult)
        {
            return mult.TotalMultiplier;
        }
    }
}
