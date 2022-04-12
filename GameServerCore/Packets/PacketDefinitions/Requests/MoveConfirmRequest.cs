namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class MoveConfirmRequest : ICoreRequest
    {
        public int SyncID { get; }
        public byte TeleportCount { get; }

        public MoveConfirmRequest(int syncId, byte teleporCount)
        {
            SyncID = syncId;
            TeleportCount = teleporCount;
        }
    }
}
