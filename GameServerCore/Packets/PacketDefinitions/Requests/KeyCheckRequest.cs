namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class KeyCheckRequest : ICoreRequest
    {
        public byte[] PartialKey { get; } = new byte[3];
        public uint PlayerNo { get; }
        public long UserId { get; }
        public uint VersionNo { get; }
        public ulong CheckId { get; }

        public KeyCheckRequest(uint playerNo, long userId, uint versionNo, ulong checkId)
        {
            PlayerNo = playerNo;
            UserId = userId;
            VersionNo = versionNo;
            CheckId = checkId;
        }
    }
}
