using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    class AddBuffResponse : ICoreResponse
    {
        public IBuff Buff { get; }
        public AddBuffResponse(IBuff nbuff)
        {
            Buff = nbuff;
        }
    }
}
