namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class KeyCheckRequest : ICoreRequest
    {
        public byte[] PartialKey { get; } = new byte[3];
        public int ClientID { get; }
        public long PlayerID { get; }
        public uint VersionNo { get; }
        public ulong CheckSum { get; }

        public KeyCheckRequest(long playerNo, int clientId, uint versionNo, ulong checkSum)
        {
            PlayerID = playerNo;
            ClientID = clientId;
            VersionNo = versionNo;
            CheckSum = checkSum;
        }
    }
}
