using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServerCore.Logic.Maps;

namespace GameServerCore.Logic
{
    public interface IGame
    {
        IMap Map { get; }
    }
}
