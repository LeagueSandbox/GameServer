using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class RemoveBuffResponse : ICoreResponse
    {
        public IAttackableUnit Unit { get; }
        public string BuffName { get; }
        public byte Slot { get;}
        public RemoveBuffResponse(IAttackableUnit u, string buffName, byte slot = 1)
        {
                Unit = u;
                BuffName = buffName;
                Slot = slot;
        }
    }
}