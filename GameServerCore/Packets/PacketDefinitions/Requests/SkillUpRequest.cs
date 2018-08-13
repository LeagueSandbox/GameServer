namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class SkillUpRequest
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
