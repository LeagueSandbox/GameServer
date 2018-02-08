using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SetTarget2 : BasePacket
    {
        public SetTarget2(AttackableUnit attacker, AttackableUnit attacked) 
            : base(PacketCmd.PKT_S2C_SetTarget2, attacker.NetId)
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