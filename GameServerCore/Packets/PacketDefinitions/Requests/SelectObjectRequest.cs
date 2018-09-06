namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    class SelectObjectRequest
    {
        public uint ClientId { get; }
        public uint NetIdSelected { get; }

        public SelectObjectRequest(uint client, long netId, uint versionNo, ulong checkId)
        {
            ClientId = ClientId;
            netId = NetIdSelected;
        }
    }
}
