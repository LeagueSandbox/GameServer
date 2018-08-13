using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerCore.Logic.Domain.GameObjects
{
    public interface IStatModifier
    {
        float BaseBonus { get; set; }
        float PercentBaseBonus { get; set; }
        float FlatBonus { get; set; }
        float PercentBonus { get; set; }
        bool StatModified { get; }
    }
}
