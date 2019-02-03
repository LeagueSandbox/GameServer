using GameServerCore.Packets.PacketDefinitions.Requests;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class PingLoadInfoResponse : ICoreResponse
    {
        public PingLoadInfoRequest Request { get; }
        public long UserId { get; }
        public PingLoadInfoResponse(PingLoadInfoRequest request, long userId)
        {
            Request = request;
            UserId = userId;
        }
    }
}