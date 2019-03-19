namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class ClickRequest : ICoreRequest
    {
        public uint TargetNetId { get; }

        public ClickRequest(uint targetNetId)
        {
            TargetNetId = targetNetId;
        }
    }
}
