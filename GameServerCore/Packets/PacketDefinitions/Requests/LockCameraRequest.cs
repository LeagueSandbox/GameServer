namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class LockCameraRequest : ICoreRequest
    {
        public bool Locked { get; }
        public int ClientID { get; }

        public LockCameraRequest(bool locked, int clientId)
        {
            Locked = locked;
            ClientID = clientId;
        }
    }
}
