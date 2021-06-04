namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class SyncSimTimeRequest : ICoreRequest
    {
        public int NetId { get; }
        public float ReceiveTime { get; }
        public float AckTime { get; }

        public SyncSimTimeRequest(int netId, float receiveTime, float ackTime)
        {
            NetId = netId;
            ReceiveTime = receiveTime;
            AckTime = ackTime;
        }
    }
}
