using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using System.Collections.Generic;
using System.Numerics;

namespace GameServerCore.Domain
{
    public interface IMapData
    {
        public Dictionary<string, IMapObject> MapObjects { get; }
    }
}