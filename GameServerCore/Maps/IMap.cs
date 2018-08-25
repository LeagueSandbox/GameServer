using GameServerCore.Content;

namespace GameServerCore.Maps
{
    public interface IMap
    {
        INavGrid NavGrid { get; }
    }
}
