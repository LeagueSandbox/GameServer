using GameServerCore.Enums;
using System.Collections.Generic;

namespace GameServerCore.Domain
{
    public interface IMapScriptData
    {
        Dictionary<GameObjectTypes, List<MapObject>> MapObjects { get; }
    }
}
