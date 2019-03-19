using GameServerCore.Domain;

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
