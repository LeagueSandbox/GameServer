using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class AddXp : BasePacket
    {
        public AddXp(AttackableUnit u, float xp)
            : base(PacketCmd.PKT_S2_C_ADD_XP)
        {
            _buffer.Write(u.NetId);
            _buffer.Write(xp);
        }
    }
}