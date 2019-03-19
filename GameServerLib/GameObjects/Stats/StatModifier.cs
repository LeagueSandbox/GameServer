using System;
using GameServerCore.Domain.GameObjects;
using GameServerCore;

namespace LeagueSandbox.GameServer.GameObjects.Stats
{
    public class StatModifier : IStatModifier
    {
        public float BaseBonus { get; set; }
        public float PercentBaseBonus { get; set; }
        public float FlatBonus { get; set; }
        public float PercentBonus { get; set; }
        public bool StatModified => Math.Abs(BaseBonus) > Extensions.COMPARE_EPSILON ||
                                    Math.Abs(PercentBaseBonus) > Extensions.COMPARE_EPSILON ||
                                    Math.Abs(FlatBonus) > Extensions.COMPARE_EPSILON ||
                                    Math.Abs(PercentBonus) > Extensions.COMPARE_EPSILON;
    }
}
