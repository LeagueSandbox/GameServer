using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class ChampionDie2 : BasePacket
    {
        public ChampionDie2(Game game, Champion die, float deathTimer) : 
            base(game, PacketCmd.PKT_S2C_CHAMPION_DIE, die.NetId)
        {
            // Not sure what the whole purpose of that packet is
            Write((float)deathTimer);
        }
    }
}