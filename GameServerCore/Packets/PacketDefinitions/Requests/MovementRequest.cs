using GameServerCore.Packets.Enums;

namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class MovementRequest : ICoreRequest
    {
        public int NetIdHeader { get; }
        public MoveType Type { get; } //byte
        public float X { get; }
        public float Y { get; }
        public uint TargetNetId { get; }
        public byte CoordCount { get; }
        public int NetId { get; }
        public byte[] MoveData { get; }

        public MovementRequest(int netIdHeader, MoveType type, float x, float y, uint targetNetId, byte coordCount, int netId, byte[] moveData)
        {
            NetIdHeader = netIdHeader;
            Type = type;
            X = x;
            Y = y;
            TargetNetId = targetNetId;
            CoordCount = coordCount;
            NetId = netId;
            MoveData = moveData;
        }
    }
}
