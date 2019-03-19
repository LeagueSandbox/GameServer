namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class GameTimerResponse : ICoreResponse
    {
        public int UserId { get; }
        public float Time { get; }
        public GameTimerResponse(int userId, float time)
        {
            UserId = userId;
            Time = time;
        }
    }
}