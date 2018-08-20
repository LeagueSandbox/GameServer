using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerCore.Logic.Domain.GameObjects
{
    public interface ITarget
    {
        float X { get; }
        float Y { get; }
        bool IsSimpleTarget { get; }
    }
}
