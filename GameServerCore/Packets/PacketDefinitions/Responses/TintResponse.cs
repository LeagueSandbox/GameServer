using GameServerCore.Content;
using GameServerCore.Enums;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class TintResponse : ICoreResponse
    {
        public TeamId Team { get; }
        public bool Enable { get; }
        public float Speed { get; }
        public Color Color { get; }
        public TintResponse(TeamId team, bool enable, float speed, Color color)
        {
            Team = team;
            Enable = enable;
            Speed = speed;
            Color = color;
        }
    }
};