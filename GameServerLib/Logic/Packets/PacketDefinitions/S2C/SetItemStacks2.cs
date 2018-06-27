using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SetItemStacks2 : BasePacket
    {
        public SetItemStacks2(AttackableUnit unit, byte slot, byte stack)
            : base(PacketCmd.PKT_S2_C_SET_ITEM_STACKS2, unit.NetId)
        {
            _buffer.Write(slot);
            _buffer.Write((byte)stack); // Needs more research
        }
    }
}