using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class ModifyShieldResponse : ICoreResponse
    {
        public IAttackableUnit Unit{ get;}
        public float Amount{ get;}
        public ShieldType Type{ get;}
        public ModifyShieldResponse(IAttackableUnit unit, float amount, ShieldType type)
        {
            Unit = unit;
            Amount = amount;
            Type = type;
      }
    }
}