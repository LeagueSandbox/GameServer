using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerCore.Enums
{
    public enum NetTeam : uint
    {
        Unknown = 0,
        Unassigned = 99,
        Order = 100,
        Chaos = 200,
    }
}
