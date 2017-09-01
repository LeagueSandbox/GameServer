using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SetItemStacks2 : BasePacket
    {
        public SetItemStacks2(Unit unit, byte slot, byte stack)
            : base(PacketCmd.PKT_S2C_SetItemStacks2, unit.NetId)
        {
            buffer.Write(slot);
            buffer.Write((byte)stack); // Needs more research
        }
    }
}