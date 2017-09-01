using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SetTarget : BasePacket
    {
        public SetTarget(Unit attacker, Unit attacked)
            : base(PacketCmd.PKT_S2C_SetTarget, attacker.NetId)
        {
            if (attacked != null)
            {
                buffer.Write(attacked.NetId);
            }
            else
            {
                buffer.Write((int)0);
            }
        }
    }
}