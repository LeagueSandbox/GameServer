namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    class SelectObjectRequest : ICoreRequest
    {
        public uint ClientId { get; }
        public uint NetIdSelected { get; }

        public SelectObjectRequest(uint client, uint netId)
        {
            ClientId = client;
            NetIdSelected = netId;
        }
    }
}
