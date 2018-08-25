using GameServerCore.Maps;

namespace GameServerCore
{
    public interface IGame
    {
        IMap Map { get; }
    }
}
