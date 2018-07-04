using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class StopAutoAttack : BasePacket
    {
        public StopAutoAttack(AttackableUnit attacker)
            : base(PacketCmd.PKT_S2C_STOP_AUTO_ATTACK, attacker.NetId)
        {
            Write((byte)0); // Flag
            Write(0); // A netId
        }
    }
}