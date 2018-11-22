namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class UseObjectRequest : ICoreRequest
    {
        public uint NetId { get; }
        public uint TargetNetId { get; }

        public UseObjectRequest(uint netId, uint targetNetId)
        {
            NetId = netId;
            TargetNetId = targetNetId;
        }
    }
}
