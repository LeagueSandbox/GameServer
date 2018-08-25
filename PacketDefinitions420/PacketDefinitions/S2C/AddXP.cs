using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class AddXp : BasePacket
    {
        public AddXp(IAttackableUnit u, float xp)
            : base(PacketCmd.PKT_S2C_ADD_XP)
        {
            WriteNetId(u);
            Write((float)xp);
        }
    }
}