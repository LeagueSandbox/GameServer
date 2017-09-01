using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class RemoveItem : BasePacket
    {
        public RemoveItem(Unit u, byte slot, short remaining)
            : base(PacketCmd.PKT_S2C_RemoveItem, u.NetId)
        {
            buffer.Write(slot);
            buffer.Write(remaining);
        }
    }
}