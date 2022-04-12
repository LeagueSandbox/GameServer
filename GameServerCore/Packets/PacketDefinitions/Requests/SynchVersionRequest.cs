namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class SynchVersionRequest : ICoreRequest
    {
        public int ClientID { get; }
        public string Version { get; }

        public SynchVersionRequest(int clientId, string version)
        {
            ClientID = clientId;
            Version = version;
        }
    }
}
