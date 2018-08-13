using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class AddXp : BasePacket
    {
        public AddXp(AttackableUnit u, float xp)
            : base(PacketCmd.PKT_S2C_ADD_XP)
        {
            WriteNetId(u);
            Write((float)xp);
        }
    }
}