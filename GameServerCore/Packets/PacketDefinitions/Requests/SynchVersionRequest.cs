namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class SynchVersionRequest : ICoreRequest
    {
        public int NetId { get; }
        public uint ClientId { get; }
        public string Version { get; }

        public SynchVersionRequest(int netId, uint clientId, string version)
        {
            NetId = netId;
            ClientId = clientId;
            Version = version;
        }
    }
}
