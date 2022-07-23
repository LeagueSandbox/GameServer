using System;
using GameServerCore;

namespace LeagueSandbox.GameServer.GameObjects.StatsNS
{
    public class StatModifier
    {
        public float BaseValue { get; set; }
        public float BaseBonus { get; set; }
        public float PercentBaseBonus { get; set; }
        public float FlatBonus { get; set; }
        public float PercentBonus { get; set; }
        public virtual bool StatModified => Math.Abs(BaseValue) > Extensions.COMPARE_EPSILON ||
                                    Math.Abs(BaseBonus) > Extensions.COMPARE_EPSILON ||
                                    Math.Abs(PercentBaseBonus) > Extensions.COMPARE_EPSILON ||
                                    Math.Abs(FlatBonus) > Extensions.COMPARE_EPSILON ||
                                    Math.Abs(PercentBonus) > Extensions.COMPARE_EPSILON;
    }
}
