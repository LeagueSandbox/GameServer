using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class ChampionDeathTimer : BasePacket
    {
        public ChampionDeathTimer(Champion die)
            : base(PacketCmd.PKT_S2_C_CHAMPION_DEATH_TIMER, die.NetId)
        {
            _buffer.Write(die.RespawnTimer / 1000.0f); // Respawn timer, float
        }
    }
}