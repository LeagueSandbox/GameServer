namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class AutoAttackOptionRequest : ICoreRequest
    {
        public bool Activated { get; }

        public AutoAttackOptionRequest(bool activated)
        {
            Activated = activated;
        }
    }
}
