using GameServerCore.NetInfo;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    class SkillUpResponse : ICoreResponse
    {
        public ClientInfo Client{ get; }
        public byte Skill{ get; }
        public byte Level{ get; }
        public byte PointsLeft { get; }
        public SkillUpResponse(ClientInfo client, byte skill, byte level, byte pointsLeft)
        {
            Client = client;
            Skill = skill;
            Level = level;
            PointsLeft = pointsLeft;
        }
    }
}
