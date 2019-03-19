using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class ModelUpdateResponse : ICoreResponse
    {
        public IAttackableUnit Obj { get; }
        public ModelUpdateResponse(IAttackableUnit obj)
        {
            Obj = obj;
        }
    }
}