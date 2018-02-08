using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class ReplaceStoreItem : BasePacket
    {
        public ReplaceStoreItem(AttackableUnit u, uint replacedItemHash, uint newItemHash)
            : base(PacketCmd.PKT_S2C_ReplaceStoreItem, u.NetId)
        {
            buffer.Write((uint)replacedItemHash);
            buffer.Write((uint)newItemHash);
        }
    }
}