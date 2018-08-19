using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerCore.Logic.Domain
{
    public interface IReplicate
    {
        uint Value { get; }
        bool IsFloat { get; }
        bool Changed { get; }
    }
}
