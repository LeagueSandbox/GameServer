using GameServerCore.Enums;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class DebugMessageTeamResponse : ICoreResponse
    {
        public TeamId Team { get; }
        public string Message { get; }
        public DebugMessageTeamResponse(TeamId team, string message)
        {
            Team = team;
            Message = message;
        }
    }
}