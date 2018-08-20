namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class AutoAttackOptionRequest
    {
        public int NetId { get; }
        public bool Activated { get; }

        public AutoAttackOptionRequest(int netId, bool activated)
        {
            NetId = netId;
            Activated = activated;
        }
    }
}
