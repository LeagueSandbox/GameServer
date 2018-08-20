using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameServerCore.Logic.Domain.GameObjects
{
    public interface IMonster : IObjAiBase
    {
        Vector2 Facing { get; }
        string Name { get; }
        string SpawnAnimation { get; }
        byte CampId { get; }
        byte CampUnk { get; }
        float SpawnAnimationTime { get; }
    }
}
