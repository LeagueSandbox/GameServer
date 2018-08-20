namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class UseObjectRequest
    {
        public uint TargetNetId { get; }

        public UseObjectRequest(uint targetNetId)
        {
            TargetNetId = targetNetId;
        }
    }
}
