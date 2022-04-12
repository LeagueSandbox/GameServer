namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class ClickRequest : ICoreRequest
    {
        public int ClientID { get; }
        public uint SelectedNetID { get; }

        public ClickRequest(uint targetNetId, int clientId)
        {
            SelectedNetID = targetNetId;
            ClientID = clientId;
        }
    }
}
