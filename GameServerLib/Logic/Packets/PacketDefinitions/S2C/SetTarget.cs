using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SetTarget : BasePacket
    {
        public SetTarget(AttackableUnit attacker, AttackableUnit attacked)
            : base(PacketCmd.PKT_S2_C_SET_TARGET, attacker.NetId)
        {
            if (attacked != null)
            {
                _buffer.Write(attacked.NetId);
            }
            else
            {
                _buffer.Write(0);
            }
        }
    }
}