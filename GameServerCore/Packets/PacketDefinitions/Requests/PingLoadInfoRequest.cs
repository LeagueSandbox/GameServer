namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class PingLoadInfoRequest : ICoreRequest
    {
        public int ClientID { get; }
        public long PlayerID { get; }
        public float Percentage { get; }
        public float ETA { get; }
        public ushort Count { get; }
        public ushort Ping { get; }
        public bool Ready { get; }

        public PingLoadInfoRequest(int clientId, long playerId, float percentage, float eta, ushort count, ushort ping, bool ready)
        {
            ClientID = clientId;
            PlayerID = playerId;
            Percentage = percentage;
            ETA = eta;
            Count = count;
            Ping = ping;
            Ready = ready;
        }
    }
}
