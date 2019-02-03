namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class BlueTipClickedRequest : ICoreRequest
    {
        public uint PlayerNetId { get; }
        public uint NetId { get; }

        public BlueTipClickedRequest(uint playerNetId, uint netId)
        {
            PlayerNetId = playerNetId;
            NetId = netId;
        }
    }
}
