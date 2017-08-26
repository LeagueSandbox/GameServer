using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SetItemStacks : BasePacket
    {
        public SetItemStacks(Unit unit, byte slot, byte stack1, byte stack2)
            : base(PacketCmd.PKT_S2C_SetItemStacks, unit.NetId)
        {
            buffer.Write(slot);
            buffer.Write((byte)stack1); // Needs more research
            buffer.Write((byte)stack2); //
        }
    }
}