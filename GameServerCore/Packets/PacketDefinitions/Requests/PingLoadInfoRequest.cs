namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class PingLoadInfoRequest : ICoreRequest
    {
        public uint NetId;
        public int ClientId;
        public long UserId;
        public float Loaded;
        public float Unk2;
        public short Ping;
        public short Unk3;
        public byte Unk4;

        public PingLoadInfoRequest(uint netId, int clientId, long userId, float loaded, float unk2, short ping, short unk3, byte unk4)
        {
            NetId = netId;
            ClientId = clientId;
            UserId = userId;
            Loaded = loaded;
            Unk2 = unk2;
            Ping = ping;
            Unk3 = unk3;
            Unk4 = unk4;
        }
    }
}
