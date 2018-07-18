using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class AddGold : BasePacket
    {
        public AddGold(Game game, Champion richMan, AttackableUnit died, float gold)
            : base(game, PacketCmd.PKT_S2C_ADD_GOLD, richMan.NetId)
        {
            WriteNetId(richMan);
            WriteNetId(died);
            Write(gold);
        }
    }
}