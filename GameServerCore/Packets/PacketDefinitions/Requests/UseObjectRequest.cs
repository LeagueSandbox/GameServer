namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class UseObjectRequest : ICoreRequest
    {
        public uint TargetNetID { get; }

        public UseObjectRequest(uint targetNetId)
        {
            TargetNetID = targetNetId;
        }
    }
}
