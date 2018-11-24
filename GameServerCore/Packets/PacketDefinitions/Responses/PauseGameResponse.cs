namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class PauseGameResponse : ICoreResponse
    {
        public int Seconds { get; }
        public bool ShowWindow { get; }
        public PauseGameResponse(int seconds, bool showWindow)
        {
            Seconds = seconds;
            ShowWindow = showWindow;
        }
    }
}