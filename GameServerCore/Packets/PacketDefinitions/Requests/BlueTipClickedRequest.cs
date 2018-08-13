namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class BlueTipClickedRequest
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
