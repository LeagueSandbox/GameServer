using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class BuyItemResponse : ICoreResponse
    {
        public int UserId { get; }
        public IChampion Champion { get; }
        public IItem ItemInstance { get; }
        public BuyItemResponse(int userId, IChampion champion, IItem itemInstance)
        {
            UserId = userId;
            Champion = champion;
            ItemInstance = itemInstance;
        }
    }
};