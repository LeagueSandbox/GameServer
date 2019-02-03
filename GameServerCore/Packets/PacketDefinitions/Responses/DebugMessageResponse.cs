namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class DebugMessageResponse : ICoreResponse
    {
        public int UserId { get; }
        public string Message { get; }
        public DebugMessageResponse(int userId, string message)
        {
            UserId = userId;
            Message = message;
        }
    }
}