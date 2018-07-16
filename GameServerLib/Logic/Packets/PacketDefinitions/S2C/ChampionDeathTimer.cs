using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class ChampionDeathTimer : BasePacket
    {
        public ChampionDeathTimer(Game game, Champion die)
            : base(game, PacketCmd.PKT_S2C_CHAMPION_DEATH_TIMER, die.NetId)
        {
            Write(die.RespawnTimer / 1000.0f); // Respawn timer, float
        }
    }
}