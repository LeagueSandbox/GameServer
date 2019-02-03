using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class LevelPropSpawnResponse : ICoreResponse
    {
        public int UserId { get; }
        public ILevelProp LevelProp { get; }
        public LevelPropSpawnResponse(int userId, ILevelProp levelProp)
        {
            UserId = userId;
            LevelProp = levelProp;
        }
    }
}