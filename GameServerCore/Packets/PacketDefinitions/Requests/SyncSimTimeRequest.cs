namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class SyncSimTimeRequest : ICoreRequest
    {
        public float TimeLastServer { get; }
        public float TimeLastClient { get; }

        public SyncSimTimeRequest(float timeLastServer, float timeLastClient)
        {
            TimeLastServer = timeLastServer;
            TimeLastClient = timeLastClient;
        }
    }
}
