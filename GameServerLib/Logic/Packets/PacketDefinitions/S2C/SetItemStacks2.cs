using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SetItemStacks2 : BasePacket
    {
        public SetItemStacks2(Game game, AttackableUnit unit, byte slot, byte stack)
            : base(game, PacketCmd.PKT_S2C_SET_ITEM_STACKS2, unit.NetId)
        {
            Write(slot);
            Write(stack); // Needs more research
        }
    }
}