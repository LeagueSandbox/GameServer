using System;
using GameServerCore.Domain.GameObjects;

namespace LeagueSandbox.GameServer.GameObjects.Stats
{
    public class StatModifier : IStatModifier
    {
        public float BaseBonus { get; set; }
        public float PercentBaseBonus { get; set; }
        public float FlatBonus { get; set; }
        public float PercentBonus { get; set; }
        public bool StatModified => Math.Abs(BaseBonus) > float.Epsilon * 3 ||
                                    Math.Abs(PercentBaseBonus) > float.Epsilon * 3 ||
                                    Math.Abs(FlatBonus) > float.Epsilon * 3 ||
                                    Math.Abs(PercentBonus) > float.Epsilon * 3;
    }
}
