using GameServerCore.Packets.PacketDefinitions.Requests;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class ViewResponseResponse : ICoreResponse
    {
        public int UserId { get; }
        // FIXME: uses ViewRequest - avoid that
        public ViewRequest Request { get; }
        public ViewResponseResponse(int userId, ViewRequest request)
        {
            UserId = userId;
            Request = request;
        }
    }
}