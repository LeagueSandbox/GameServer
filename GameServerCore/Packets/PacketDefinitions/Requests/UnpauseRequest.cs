namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class UnpauseRequest : ICoreRequest
    {
        public int ClientID { get; }
        public bool Delayed { get;  }

        public UnpauseRequest(int clientId, bool delayed)
        {
            ClientID = clientId;
            Delayed = delayed;
        }
    }
}
