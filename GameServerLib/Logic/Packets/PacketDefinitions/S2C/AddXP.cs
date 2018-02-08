using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class AddXP : BasePacket
    {
        public AddXP(AttackableUnit u, float xp)
            : base(PacketCmd.PKT_S2C_AddXP)
        {
            buffer.Write(u.NetId);
            buffer.Write(xp);
        }
    }
}