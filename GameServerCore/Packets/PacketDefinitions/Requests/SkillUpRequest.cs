namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class SkillUpRequest : ICoreRequest
    {
        public byte Slot { get; }
        public bool IsEvolve { get; }

        public SkillUpRequest(byte slot, bool isEvolve)
        {
            Slot = slot;
            IsEvolve = isEvolve;
        }
    }
}
