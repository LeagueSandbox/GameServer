using System.Collections.Generic;
using GameServerCore.Content;
using GameServerCore.Domain;

namespace GameServerCore.Maps
{
    public interface IMap: IUpdate
    {
        List<IAnnounce> AnnouncerEvents { get; }
        INavGrid NavGrid { get; }
        int Id { get; }
        ICollisionHandler CollisionHandler { get; }
        void Init();
        IMapProperties MapProperties { get; }
        IMapProperties GetMapProperties(int mapId);
    }
}
