namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class KeyCheckRequest : ICoreRequest
    {
        public byte[] PartialKey { get; } = new byte[3];
        public uint ClientID { get; }
        public ulong PlayerID { get; }
        public uint VersionNo { get; }
        public ulong CheckId { get; }

        public KeyCheckRequest(ulong playerNo, uint clientId, uint versionNo, ulong checkId)
        {
            PlayerID = playerNo;
            ClientID = clientId;
            VersionNo = versionNo;
            CheckId = checkId;
        }
    }
}
