using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SetItemStacks : BasePacket
    {
        public SetItemStacks(Game game, AttackableUnit unit, byte slot, byte stack1, byte stack2)
            : base(game, PacketCmd.PKT_S2C_SET_ITEM_STACKS, unit.NetId)
        {
            Write(slot);
            Write(stack1); // Needs more research
            Write(stack2); //
        }
    }
}