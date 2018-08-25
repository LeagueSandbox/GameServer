using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class ReplaceStoreItem : BasePacket
    {
        public ReplaceStoreItem(IAttackableUnit u, uint replacedItemHash, uint newItemHash)
            : base(PacketCmd.PKT_S2C_REPLACE_STORE_ITEM, u.NetId)
        {
            Write(replacedItemHash);
            Write(newItemHash);
        }
    }
}