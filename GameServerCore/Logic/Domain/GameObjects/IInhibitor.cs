using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServerCore.Logic.Enums;

namespace GameServerCore.Logic.Domain.GameObjects
{
    public interface IInhibitor : IObjAnimatedBuilding
    {
        InhibitorState InhibitorState { get; }
    }
}
