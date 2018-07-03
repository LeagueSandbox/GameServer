using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class RemoveItem : BasePacket
    {
        public RemoveItem(AttackableUnit u, byte slot, short remaining)
            : base(PacketCmd.PKT_S2C_REMOVE_ITEM, u.NetId)
        {
            Write(slot);
            Write(remaining);
        }
    }
}