using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerCore.Logic.Domain.GameObjects
{
    public interface IBaseTurret : IObjAiBase
    {
        string Name { get; }
        uint ParentNetId { get; }
    }
}
