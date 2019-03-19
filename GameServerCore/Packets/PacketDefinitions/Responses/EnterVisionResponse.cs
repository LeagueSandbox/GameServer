using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class EnterVisionResponse : ICoreResponse
    {
        public int UserId { get; }
        public IChampion Champion { get; }
        public EnterVisionResponse(int userId, IChampion champion)
        {
            UserId = userId;
            Champion = champion;
        }
    }
}