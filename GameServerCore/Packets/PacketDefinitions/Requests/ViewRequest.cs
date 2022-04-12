using System.Numerics;

namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class ViewRequest : ICoreRequest
    {
        public Vector3 CameraPosition { get; }
        public Vector3 CameraDirection { get; }
        public int ClientID { get; }
        public byte SyncID { get; }

        public ViewRequest(Vector3 cameraPos, Vector3 cameraDirection, int clientId, byte syncId)
        {
            CameraPosition = cameraPos;
            CameraDirection = cameraDirection;
            ClientID = clientId;
            SyncID = syncId;
        }
    }
}
