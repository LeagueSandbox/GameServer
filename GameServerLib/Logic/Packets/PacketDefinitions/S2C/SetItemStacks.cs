using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SetItemStacks : BasePacket
    {
        public SetItemStacks(AttackableUnit unit, byte slot, byte stack1, byte stack2)
            : base(PacketCmd.PKT_S2_C_SET_ITEM_STACKS, unit.NetId)
        {
            _buffer.Write(slot);
            _buffer.Write(stack1); // Needs more research
            _buffer.Write(stack2); //
        }
    }
}