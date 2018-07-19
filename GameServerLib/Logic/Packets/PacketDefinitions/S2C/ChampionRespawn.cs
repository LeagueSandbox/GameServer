using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class ChampionRespawn : BasePacket
    {
        public ChampionRespawn(Game game, Champion c) :
            base(game, PacketCmd.PKT_S2C_CHAMPION_RESPAWN, c.NetId)
        {
            Write((float)c.X);
            Write((float)c.Y);
            Write((float)c.GetZ());
        }
    }
}