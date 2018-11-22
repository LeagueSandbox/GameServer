namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class SynchVersionRequest : ICoreRequest
    {
        public int NetId { get; }
        public int Unk1 { get; }
        public string Version { get; }

        public SynchVersionRequest(int netId, int unk1, string version)
        {
            NetId = netId;
            Unk1 = unk1;
            Version = version;
        }
    }
}
