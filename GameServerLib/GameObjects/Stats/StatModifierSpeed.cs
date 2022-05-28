using System;
using GameServerCore.Domain.GameObjects;
using GameServerCore;

namespace LeagueSandbox.GameServer.GameObjects.Stats
{
    public class StatModifierSpeed : StatModifier, IStatModifierSpeed
    {
        public float SlowResist { get; set; }
        public override bool StatModified => Math.Abs(BaseValue) > Extensions.COMPARE_EPSILON ||
                                    Math.Abs(BaseBonus) > Extensions.COMPARE_EPSILON ||
                                    Math.Abs(PercentBaseBonus) > Extensions.COMPARE_EPSILON ||
                                    Math.Abs(FlatBonus) > Extensions.COMPARE_EPSILON ||
                                    Math.Abs(PercentBonus) > Extensions.COMPARE_EPSILON||
                                    Math.Abs(SlowResist) > Extensions.COMPARE_EPSILON;
    }
}
