namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class SurrenderRequest : ICoreRequest
    {
        public bool VotedYes { get; set; }

        public SurrenderRequest(bool vote)
        {
            VotedYes = vote;
        }
    }
}
