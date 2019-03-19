using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class ItemBoughtResponse : ICoreResponse
    {
        public IAttackableUnit Unit { get; }
        public IItem Item { get; }
        public ItemBoughtResponse(IAttackableUnit u, IItem i)
        {
            Unit = u;
            Item = i;
        }
    }
}