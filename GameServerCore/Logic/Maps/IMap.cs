using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServerCore.Logic.Content;

namespace GameServerCore.Logic.Maps
{
    public interface IMap
    {
        INavGrid NavGrid { get; }
    }
}
