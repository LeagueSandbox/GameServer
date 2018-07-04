using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SetTarget : BasePacket
    {
        public SetTarget(AttackableUnit attacker, AttackableUnit attacked)
            : base(PacketCmd.PKT_S2C_SET_TARGET, attacker.NetId)
        {
            WriteNetId(attacked);
        }
    }
}