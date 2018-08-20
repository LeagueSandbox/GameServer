using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerCore.Logic.Domain.GameObjects
{
    public interface IStat
    {
        bool Modified { get; }
        float BaseBonus { get; }
        float FlatBonus { get; set; }
        float BaseValue { get; set; }
        float PercentBonus { get; }
        float PercentBaseBonus { get; }
        float Total { get; }
    }
}
