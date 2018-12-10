using GameServerCore.Packets.Enums;

namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class ChatMessageRequest : ICoreRequest
    {
        public string Message { get; }
        public ChatType Type { get; }

        public ChatMessageRequest(string message, ChatType type)
        {
            Message = message;
            Type = type;
        }
    }
}
