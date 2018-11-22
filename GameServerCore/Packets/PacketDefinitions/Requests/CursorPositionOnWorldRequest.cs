namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class CursorPositionOnWorldRequest : ICoreRequest
    {
        public uint NetId { get; }
        /// <summary>
        /// Maybe 2 bytes instead of 1 short?
        /// </summary>
        public short Unk1 { get; }
        public float X { get; }
        public float Z { get; }
        public float Y { get; }

        public CursorPositionOnWorldRequest(uint netId, short unk1, float x, float z, float y)
        {
            NetId = netId;
            Unk1 = unk1;
            X = x;
            Z = z;
            Y = y;
        }
    }
}
