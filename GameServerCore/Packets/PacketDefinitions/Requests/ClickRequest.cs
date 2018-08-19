namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class ClickRequest
    {
        public uint TargetNetId { get; }

        public ClickRequest(uint targetNetId)
        {
            TargetNetId = targetNetId;
        }
    }
}
