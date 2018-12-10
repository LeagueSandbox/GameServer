namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class HeartbeatRequest : ICoreRequest
    {
        public int NetId { get; }
        public float ReceiveTime { get; }
        public float AckTime { get; }

        public HeartbeatRequest(int netId, float receiveTime, float ackTime)
        {
            NetId = netId;
            ReceiveTime = receiveTime;
            AckTime = ackTime;
        }
    }
}
