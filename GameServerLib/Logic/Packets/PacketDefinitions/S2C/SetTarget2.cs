using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SetTarget2 : BasePacket
    {
        public SetTarget2(AttackableUnit attacker, AttackableUnit attacked)
            : base(PacketCmd.PKT_S2_C_SET_TARGET2, attacker.NetId)
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