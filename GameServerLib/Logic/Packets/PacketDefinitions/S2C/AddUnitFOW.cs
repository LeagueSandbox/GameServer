using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class AddUnitFOW : BasePacket
    {
        public AddUnitFOW(AttackableUnit u)
            : base(PacketCmd.PKT_S2C_AddUnitFOW)
        {
            buffer.Write((int)u.NetId);
        }
    }
}