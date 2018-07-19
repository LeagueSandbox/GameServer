using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class AddUnitFow : BasePacket
    {
        public AddUnitFow(Game game, AttackableUnit u)
            : base(game, PacketCmd.PKT_S2C_ADD_UNIT_FOW)
        {
            WriteNetId(u);
        }
    }
}