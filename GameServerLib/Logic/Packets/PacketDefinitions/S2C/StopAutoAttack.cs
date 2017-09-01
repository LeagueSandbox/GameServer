using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class StopAutoAttack : BasePacket
    {
        public StopAutoAttack(Unit attacker) 
            : base(PacketCmd.PKT_S2C_StopAutoAttack, attacker.NetId)
        {
            buffer.Write((byte)0); // Flag
            buffer.Write((int)0); // A netId
        }
    }
}