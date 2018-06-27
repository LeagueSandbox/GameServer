using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class AddUnitFow : BasePacket
    {
        public AddUnitFow(AttackableUnit u)
            : base(PacketCmd.PKT_S2C_ADD_UNIT_FOW)
        {
            _buffer.Write((int)u.NetId);
        }
    }
}