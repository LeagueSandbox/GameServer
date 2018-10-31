using System;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class StatModifcator
    {
        public float BaseBonus { get; set; }
        public float PercentBaseBonus { get; set; }
        public float FlatBonus { get; set; }
        public float PercentBonus { get; set; }
        public bool StatModified => (
            Math.Abs(BaseBonus) > float.Epsilon * 3 ||
            Math.Abs(PercentBonus) > float.Epsilon * 3 ||
            Math.Abs(FlatBonus) > float.Epsilon * 3 ||
            Math.Abs(PercentBonus) > float.Epsilon * 3
        );
    }
}
