namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class SurrenderRequest : ICoreRequest
    {
        public bool VotedYes { get; }

        public SurrenderRequest(bool vote)
        {
            VotedYes = vote;
        }
    }
}
