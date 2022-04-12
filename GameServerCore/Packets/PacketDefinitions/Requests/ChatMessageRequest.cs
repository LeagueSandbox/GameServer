using GameServerCore.Enums;

namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class ChatMessageRequest : ICoreRequest
    {
        public int ClientID { get; }
        public uint NetID { get; }
        public bool Localized { get; }
        public string Params { get; }
        public string Message { get; }
        public ChatType ChatType { get; }

        public ChatMessageRequest(string message, ChatType type, string param, bool localized, uint netId, int clientId)
        {
            Message = message;
            ChatType = type;
            Params = param;
            Localized = localized;
            NetID = netId;
            ClientID = clientId;
        }
    }
}
