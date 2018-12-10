namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class GameTimerUpdateResponse : ICoreResponse
    {
        public int UserId { get; }
        public float Time { get; }
        public GameTimerUpdateResponse(int userId, float time)
        {
            UserId = userId;
            Time = time;
        }
    }
}