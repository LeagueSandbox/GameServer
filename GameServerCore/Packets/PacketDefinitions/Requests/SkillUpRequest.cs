namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class SkillUpRequest : ICoreRequest
    {
        public uint NetId { get; }
        public byte Skill { get; }

        public SkillUpRequest(uint netId, byte skill)
        {
            NetId = netId;
            Skill = skill;
        }
    }
}
