using System.Numerics;

namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class ViewRequest : ICoreRequest
    {
        public Vector3 CameraPosition { get; set; }
        public Vector3 CameraDirection { get; set; }
        public int ClientID { get; set; }
        public byte SyncID { get; set; }

        public ViewRequest(Vector3 cameraPos, Vector3 cameraDirection, int clientId, byte syncId)
        {
            CameraPosition = cameraPos;
            CameraDirection = cameraDirection;
            ClientID = clientId;
            SyncID = syncId;
        }
    }
}
