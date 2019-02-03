using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class AddXpResponse : ICoreResponse
    {
        public IChampion Champion { get; }
        public float Experience { get; }
        public AddXpResponse(IChampion champion, float experience)
        {
            Champion = champion;
            Experience = experience;
        }
    }
}