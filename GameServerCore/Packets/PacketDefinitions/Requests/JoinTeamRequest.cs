namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class JoinTeamRequest : ICoreRequest
    {
        public int ClientID { get; }
        public uint NetTeamID { get; }

        public JoinTeamRequest(int clientID, uint netTeamID)
        {
            ClientID = clientID;
            NetTeamID = netTeamID;
        }
    }
}